using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using asm.Threading;
using JetBrains.Annotations;

namespace asm.Other.MarcGravell.Channel
{
	internal class OutPipe : MutexFreePipe
	{
		private readonly List<SafeMemoryMappedFile> _oldBuffers = new List<SafeMemoryMappedFile>();
		private int _messageNumber;
		private int _bufferCount;

		public OutPipe([NotNull] string name, bool createBuffer) : base(name, createBuffer) { }

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			foreach (SafeMemoryMappedFile buffer in _oldBuffers)
				buffer.Dispose();

			base.Dispose(disposing);
		}

		public int PendingBuffers => _oldBuffers.Count;

		public void Write([NotNull] byte[] data)
		{
			ThrowIfDisposedOrDisposing();
			if (data.Length > Length - Offset - 8)
			{
				// Not enough space left in the shared memory buffer to write the message.
				WriteContinuation(data.Length);
			}

			WriteMessage(data);
			NewMessageSignal.Set(); // Signal reader that a message is available
		}

		private unsafe void WriteMessage([NotNull] byte[] block)
		{
			byte* ptr = Buffer.Pointer;
			byte* offsetPointer = ptr + Offset;

			int* msgPointer = (int*)offsetPointer;
			*msgPointer = block.Length;

			Offset += MESSAGE_HEADER_LENGTH;
			offsetPointer += MESSAGE_HEADER_LENGTH;

			if (block.Length > 0)
			{
				//MMF.Accessor.WriteArray (Offset, block, 0, block.Length);   // Horribly slow. No. No. No.
				Marshal.Copy(block, 0, new IntPtr(offsetPointer), block.Length);
				Offset += block.Length;
			}

			// Write the latest message number to the start of the buffer:
			int* iptr = (int*)ptr;
			*iptr = ++_messageNumber;
		}

		private void WriteContinuation(int messageSize)
		{
			// First, allocate a new buffer:		
			string newName = Name + "." + ++_bufferCount;
			int newLength = Math.Max(messageSize * 10, MINIMUM_BUFFER_SIZE);
			SafeMemoryMappedFile newFile = new SafeMemoryMappedFile(MemoryMappedFile.CreateNew(newName, newLength, MemoryMappedFileAccess.ReadWrite));

			// Write a message to the old buffer indicating the address of the new buffer:
			WriteMessage(new byte[0]);

			// Keep the old buffer alive until the reader has indicated that it's seen it:
			_oldBuffers.Add(Buffer);

			// Make the new buffer current:
			Buffer = newFile;
			Length = newFile.Length;
			Offset = STARTING_OFFSET;

			// Release old buffers that have been read:	
			foreach (SafeMemoryMappedFile buffer in _oldBuffers.Take(_oldBuffers.Count - 1))
			{
				if (!buffer.Accessor.ReadBoolean(4)) continue;
				_oldBuffers.Remove(buffer);
				buffer.Dispose();
			}
		}
	}
}