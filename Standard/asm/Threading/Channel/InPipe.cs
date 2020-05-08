using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;
using JetBrains.Annotations;

namespace asm.Threading.Channel
{
	internal class InPipe : MutexFreePipe
	{
		private readonly Action<byte[]> _onMessage;
		private int _lastMessageProcessed;
		private int _bufferCount;

		public InPipe([NotNull] string name, bool createBuffer, Action<byte[]> onMessage) : base(name, createBuffer)
		{
			_onMessage = onMessage;
			new Thread(Go).Start();
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			NewMessageSignal.Set();
			base.Dispose(disposing);
		}

		private void Go()
		{
			int spinCycles = 0;

			while (true)
			{
				int? latestMessageID = GetLatestMessageID();
				if (latestMessageID == null) return; // We've been disposed.

				if (latestMessageID > _lastMessageProcessed)
				{
					Thread.MemoryBarrier(); // We need this because of lock-free implementation						
					byte[] msg = GetNextMessage();
					if (msg == null) return;
					if (msg.Length > 0) _onMessage?.Invoke(msg); // Zero-length msg will be a buffer continuation 
					spinCycles = 1000;
				}

				if (spinCycles == 0)
				{
					NewMessageSignal.WaitOne();
					if (IsDisposed) return;
				}
				else
				{
					Thread.MemoryBarrier(); // We need this because of lock-free implementation		
					spinCycles--;
				}
			}
		}

		private unsafe int? GetLatestMessageID()
		{
			return IsDisposedOrDisposing
						? (int?)null
						: *(int*)Buffer.Pointer;
		}

		private unsafe byte[] GetNextMessage()
		{
			_lastMessageProcessed++;
			if (IsDisposed) return null;

			byte* offsetPointer = Buffer.Pointer + Offset;
			int* msgPointer = (int*)offsetPointer;

			int msgLength = *msgPointer;

			Offset += MESSAGE_HEADER_LENGTH;
			offsetPointer += MESSAGE_HEADER_LENGTH;

			if (msgLength == 0)
			{
				Buffer.Accessor.Write(4, true); // Signal that we no longer need file				
				Buffer.Dispose();
				string newName = $"{Name}.{++_bufferCount}";
				Buffer = new SafeMemoryMappedFile(MemoryMappedFile.OpenExisting(newName));
				Offset = STARTING_OFFSET;
				return new byte[0];
			}

			Offset += msgLength;

			//MMF.Accessor.ReadArray (Offset, msg, 0, msg.Length);    // too slow			
			byte[] msg = new byte[msgLength];
			Marshal.Copy(new IntPtr(offsetPointer), msg, 0, msg.Length);
			return msg;
		}
	}
}