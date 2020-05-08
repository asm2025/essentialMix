using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.IO
{
	/// <summary>
	/// PipeStream is an odd little hybrid between the client and server ends of a NamedPipe,
	/// and a global::System.IO.Stream subclass.  Basically, it simplifies the unnecessarily horrific process
	/// of implementing named pipe support in .Net.   (If you doubt this, try it the hard way... we'll wait.)
	/// Usage idiom:
	/// Server side
	/// -----------
	/// 1. Call PipeStream.Create, specify inbound, outbound, or both
	/// 2. Call Listen().  This will block until a client connects.  Sorry, the alternatives
	/// are ugly.  Use a thread.
	/// 3. Call DataAvailable() in a loop with Read(), Write, ReadLine(), etc. until IsConnected turns false.
	/// 4. Call Listen() again to wait for the next client.
	/// Client side
	/// -----------
	/// 1. Call Open()
	/// 2. Call DataAvailable(), Read(), Write(), etc. until you're done,
	/// then call Close();
	/// And yes, you can attach TextReader and TextWriter instances to this stream.
	/// Server side caveat:
	/// The idiom described above only supports a single client at a time.  If you need
	/// to support multiple clients, multiple calls to Create()/Listen() in separate threads is the
	/// recommended approach.
	/// There is a test driver class at the end of this file which can be cannibalized for sample usage code.
	/// </summary>
	public class PipeStream : Stream
	{
		public enum PeerType
		{
			Client,
			Server
		}

		public enum Direction
		{
			InboundOnly = (int)Win32.PIPE_ACCESS_INBOUND,
			OutboundOnly = (int)Win32.PIPE_ACCESS_OUTBOUND,
			Bidirectional = (int)(Win32.PIPE_ACCESS_INBOUND + Win32.PIPE_ACCESS_OUTBOUND)
		}

		private const string PREFIX = @"\\.\pipe\";

		private readonly PeerType _peerType;

		private IntPtr _readHandle;
		private IntPtr _writeHandle;
		private FileAccess _mode;

		public PipeStream(string name, FileAccess mode)
		{
			name = name?.Trim();

			if (string.IsNullOrEmpty(name)) name = null;
			else if (!name.StartsWith(PREFIX, StringComparison.OrdinalIgnoreCase)) name = PREFIX + name;

			_readHandle = _writeHandle = IntPtr.Zero;
			_peerType = PeerType.Client;
			Name = name;
			Open(mode);
		}

		public PipeStream(IntPtr handle, FileAccess mode)
		{
			_readHandle = _writeHandle = handle;
			_mode = mode;
			_peerType = PeerType.Client;
		}

		protected PipeStream()
		{
			_readHandle = _writeHandle = IntPtr.Zero;
			_mode = 0;
			_peerType = PeerType.Server;
		}

		public override bool CanRead => (_mode & FileAccess.Read) > 0;

		public override bool CanWrite => (_mode & FileAccess.Write) > 0;

		public override bool CanSeek => false;

		public override long Length => throw new NotSupportedException("PipeStream does not support seeking");

		public override long Position
		{
			get => throw new NotSupportedException("PipeStream does not support seeking");
			set { }
		}

		public override void Flush()
		{
			if (_readHandle.IsInvalidHandle()) throw new ObjectDisposedException("PipeStream", "The stream has already been closed");
			Win32.FlushFileBuffers(_readHandle);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			buffer.Length.ValidateRange(offset, ref count);
			if (!CanRead) throw new NotSupportedException("The stream does not support reading");
			if (_readHandle.IsZero()) throw new ObjectDisposedException("PipeStream", "The stream has already been closed");

			// first read the data into an internal buffer since ReadFile cannot read into a buf at
			// a specified offset
			byte[] buf = buffer;

			if (offset > 0) buf = new byte[count];
			uint ucount = (uint)count;
			if (!Win32.ReadFile(_readHandle, buf, ucount, out uint read, IntPtr.Zero)) throw new Win32Exception(Marshal.GetLastWin32Error(), "ReadFile failed");
			if (offset == 0) return (int)read;
			Array.Copy(buf, 0, buffer, offset, read);
			return (int)read;
		}

		public override void Close()
		{
			Win32.CloseHandle(_readHandle);
			Win32.CloseHandle(_writeHandle);
			_readHandle = IntPtr.Zero;
			_writeHandle = IntPtr.Zero;
		}

		public override void SetLength(long length) { throw new NotSupportedException("PipeStream doesn't support SetLength"); }

		public override void Write(byte[] buffer, int offset, int count)
		{
			buffer.Length.ValidateRange(offset, ref count);
			if (!CanWrite) throw new NotSupportedException("The stream does not support writing");
			if (_writeHandle.IsZero()) throw new ObjectDisposedException("PipeStream", "The stream has already been closed");

			// copy data to internal buffer to allow writing from a specified offset
			if (offset != 0)
			{
				byte[] buf = new byte[count];
				Array.Copy(buffer, offset, buf, 0, count);
				buffer = buf;
			}

			bool result = Win32.WriteFile(_readHandle, buffer, (uint)count, out uint written, IntPtr.Zero);

			if (!result)
			{
				int err = Marshal.GetLastWin32Error();
				throw new Win32Exception(err, "Writing to the stream failed");
			}

			if (written < count) throw new IOException("Unable to write entire buffer to stream");
		}

		public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException("PipeStream doesn't support seeking"); }

		/// <summary>
		/// Returns true if client is connected.  Should only be called after Listen() succeeds.
		/// </summary>
		/// <returns></returns>
		public bool IsConnected
		{
			get
			{
				if (_peerType != PeerType.Server) throw new NotSupportedException();
				if (Win32.ConnectNamedPipe(_readHandle, IntPtr.Zero)) return false;
				return (uint)Marshal.GetLastWin32Error() == Win32.ResultWin32.ERROR_PIPE_CONNECTED;
			}
		}

		public bool DataAvailable
		{
			get
			{
				if (!CanRead) return false;
				uint bytesRead = 0, avail = 0, thismsg = 0;
				bool result = Win32.PeekNamedPipe(_readHandle, null, 0, ref bytesRead, ref avail, ref thismsg);
				return result && avail > 0;
			}
		}

		public string Name { get; private set; }

		public IntPtr ReadHandle => _readHandle;

		public IntPtr WriteHandle => _writeHandle;

		/// <summary>
		/// Opens the client side of a pipe.
		/// </summary>
		/// <param name="mode">Read, Write, or ReadWrite - must be compatible with server-side creation mode</param>
		public void Open(FileAccess mode)
		{
			Win32.FileAccessEnum pipemode = 0;

			if (mode.HasFlag(FileAccess.Read)) pipemode |= Win32.FileAccessEnum.FILE_GENERIC_READ;
			if (mode.HasFlag(FileAccess.Write)) pipemode |= Win32.FileAccessEnum.FILE_GENERIC_WRITE;

			Win32.SECURITY_ATTRIBUTES sa = new Win32.SECURITY_ATTRIBUTES();
			IntPtr handle = Win32.CreateFile(Name, pipemode, 0, ref sa, Win32.CreationDispositionEnum.OpenExisting, 0, IntPtr.Zero);

			if (handle == Win32.INVALID_HANDLE_VALUE)
			{
				int err = Marshal.GetLastWin32Error();
				throw new Win32Exception(err, $"Open failed, win32 error code {err}, pipe name '{Name}' ");
			}

			_mode = mode;
			_readHandle = _writeHandle = handle;
		}

		public bool Listen()
		{
			if (_peerType != PeerType.Server) throw new NotSupportedException();
			Win32.DisconnectNamedPipe(_readHandle);
			if (Win32.ConnectNamedPipe(_readHandle, IntPtr.Zero)) return true;
			uint lastErr = (uint)Marshal.GetLastWin32Error();
			return lastErr == Win32.ResultWin32.ERROR_PIPE_CONNECTED;
		}

		/// <summary>
		/// Server only: disconnect the pipe.  For most applications, you should just call Listen()
		/// instead, which automatically does a disconnect of any old connection.
		/// </summary>
		public void Disconnect()
		{
			if (_peerType != PeerType.Server) throw new NotSupportedException();
			Win32.DisconnectNamedPipe(_readHandle);
		}

		/// <summary>
		/// Create a named pipe instance.
		/// </summary>
		/// <param name="name">Local name (the part after \\.\pipe\) or null for anonymous pipe</param>
		/// <param name="mode"></param>
		/// <param name="bufferSize"></param>
		/// <param name="inherit"></param>
		[NotNull]
		public static PipeStream Create(string name, Direction mode, bool inherit = true, uint bufferSize = 0)
		{
			name = name?.Trim();

			if (string.IsNullOrEmpty(name)) name = null;
			else if (!name.StartsWith(PREFIX, StringComparison.OrdinalIgnoreCase)) name = PREFIX + name;

			if (bufferSize < StreamHelper.BUFFER_DEFAULT) bufferSize = StreamHelper.BUFFER_DEFAULT;

			Win32.SECURITY_ATTRIBUTES saAttr = new Win32.SECURITY_ATTRIBUTES
			{
				bInheritHandle = inherit,
				lpSecurityDescriptor = IntPtr.Zero
			};

			PipeStream self;

			if (name == null)
			{
				if (!Win32.CreatePipe(out IntPtr rh, out IntPtr wh, ref saAttr, bufferSize)) throw new Win32Exception("Error creating pipe. Internal error: " + Marshal.GetLastWin32Error());

				Win32.SetHandleInformation(rh, Win32.HandleFlagsEnum.INHERIT, inherit ? Win32.HandleFlagsEnum.INHERIT : 0);
				Win32.SetHandleInformation(wh, Win32.HandleFlagsEnum.INHERIT, inherit ? Win32.HandleFlagsEnum.INHERIT : 0);
				self = new PipeStream {Name = null, _readHandle = rh, _writeHandle = wh};
			}
			else
			{
				IntPtr handle = Win32.CreateNamedPipe(name, (uint)mode, Win32.PIPE_TYPE_BYTE | Win32.PIPE_WAIT,
					Win32.PIPE_UNLIMITED_INSTANCES, 0, bufferSize, Win32.NMPWAIT_WAIT_FOREVER, ref saAttr);
				if (handle == Win32.INVALID_HANDLE_VALUE) throw new Win32Exception("Error creating named pipe " + name + ". Internal error: " + Marshal.GetLastWin32Error());

				Win32.SetHandleInformation(handle, Win32.HandleFlagsEnum.INHERIT, inherit ? Win32.HandleFlagsEnum.INHERIT : 0);
				self = new PipeStream {Name = name, _readHandle = handle, _writeHandle = handle};
			}

			switch (mode)
			{
				case Direction.InboundOnly:
					self._mode = FileAccess.Read;
					break;
				case Direction.OutboundOnly:
					self._mode = FileAccess.Write;
					break;
				case Direction.Bidirectional:
					self._mode = FileAccess.ReadWrite;
					break;
			}

			return self;
		}
	}
}