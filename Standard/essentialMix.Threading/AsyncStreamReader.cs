//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--== 
/*============================================================
** 
** Class:  AsyncStreamReader 
**
** Purpose: For reading text from streams using a particular 
** encoding in an asynchronous manner used by the process class
**
**
===========================================================*/

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;

namespace essentialMix.Threading
{
	/// <summary>
	/// This is a modified version of Microsoft's internal class AsyncStreamReader
	/// http://www.dotnetframework.org/default.aspx/DotNET/DotNET/8@0/untmp/whidbey/REDBITS/ndp/fx/src/Services/Monitoring/system/Diagnosticts/AsyncStreamReader@cs/1/AsyncStreamReader@cs
	/// </summary>
	public sealed class AsyncStreamReader : Disposable
	{
		internal const int BUFFER_DEFAULT = StreamHelper.BUFFER_DEFAULT;  // Byte buffer size
		private const int BUFFER_MIN = StreamHelper.BUFFER_MIN;

		[NotNull]
		// ReSharper disable once NotAccessedField.Local
		private readonly Process _process;
		private readonly Decoder _decoder;
		private readonly byte[] _byteBuffer;
		private readonly char[] _charBuffer;

		// Delegate to call user function.
		private readonly Action<string> _userCallBack;

		// Internal Cancel operation 
		private bool _cancelOperation;
		private ManualResetEventSlim _eofEvent;
		private Stream _baseStream;

		public AsyncStreamReader([NotNull] Process process, [NotNull] Stream stream, [NotNull] Action<string> callback, [NotNull] Encoding encoding)
			: this(process, stream, callback, encoding, BUFFER_DEFAULT)
		{
		}

		// Creates a new AsyncStreamReader for the given stream.  The 
		// character encoding is set by encoding and the buffer size,
		// in number of 16-bit characters, is set by bufferSize. 
		public AsyncStreamReader([NotNull] Process process, [NotNull] Stream stream, [NotNull] Action<string> callback, [NotNull] Encoding encoding, int bufferSize)
		{
			_process = process;
			_baseStream = stream;
			CurrentEncoding = encoding;
			_userCallBack = callback;
			_decoder = encoding.GetDecoder();
			if (bufferSize < BUFFER_MIN) bufferSize = BUFFER_MIN;
			_byteBuffer = new byte[bufferSize];
			int maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
			_charBuffer = new char[maxCharsPerBuffer];
			_cancelOperation = false;
			_eofEvent = new ManualResetEventSlim(false);
		}

		public void Close()
		{
			Dispose(true);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ObjectHelper.Dispose(ref _baseStream);
				ObjectHelper.Dispose(ref _eofEvent);
			}
			base.Dispose(disposing);
		}

		public Encoding CurrentEncoding { get; }

		public Stream BaseStream => _baseStream;

		// User calls BeginRead to start the asynchronous read
		public void BeginRead()
		{
			_cancelOperation = false;
			BaseStream.BeginRead(_byteBuffer, 0, _byteBuffer.Length, ReadBuffer, null);
		}

		public void CancelOperation()
		{
			_cancelOperation = true;
		}

		// This is the async callback function. Only one thread could/should call this.
		private void ReadBuffer(IAsyncResult ar)
		{
			if (_cancelOperation || BaseStream == null) return;

			int byteLen;

			try
			{
				byteLen = (int)BaseStream?.EndRead(ar);
			}
			catch (IOException)
			{
				// We should ideally consume errors from operations getting cancelled
				// so that we don't crash the unsuspecting parent with an unhandled exc. 
				// This seems to come in 2 forms of exceptions (depending on platform and scenario),
				// namely OperationCanceledException and IOException (for error code that we don't 
				// map explicitly). 
				byteLen = 0; // Treat this as EOF
			}
			catch (OperationCanceledException)
			{
				// We should consume any OperationCanceledException from child read here
				// so that we don't crash the parent with an unhandled exc
				byteLen = 0; // Treat this as EOF 
			}

			if (byteLen == 0)
			{
				// We're at EOF, we won't call this function again from here on.
				_eofEvent.Set();
			}
			else
			{
				int charLen = _decoder.GetChars(_byteBuffer, 0, byteLen, _charBuffer, 0);
				if (charLen > 0) _userCallBack(new string(_charBuffer, 0, charLen));
				BaseStream?.BeginRead(_byteBuffer, 0, _byteBuffer.Length, ReadBuffer, null);
			}
		}

		// Wait until we hit EOF. This is called from Process.WaitForExit 
		// We will lose some information if we don't do this.
		public void WaitUtilEof()
		{
			if (_eofEvent == null) return;
			_eofEvent.Wait();
			ObjectHelper.Dispose(ref _eofEvent);
		}
	}
}