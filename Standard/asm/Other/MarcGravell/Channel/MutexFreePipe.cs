using System;
using System.IO.MemoryMappedFiles;
using System.Threading;
using asm.Helpers;
using asm.Patterns.Object;
using asm.Threading;
using JetBrains.Annotations;

namespace asm.Other.MarcGravell.Channel
{
	internal abstract class MutexFreePipe : Disposable, IDisposable
	{
		protected const int MINIMUM_BUFFER_SIZE = 0x10000;
		protected static readonly int MESSAGE_HEADER_LENGTH = Constants.INT_SIZE;
		protected static readonly int STARTING_OFFSET = Constants.INT_SIZE + Constants.BOOL_SIZE;

		public readonly string Name;

		protected EventWaitHandle NewMessageSignal;
		protected SafeMemoryMappedFile Buffer;
		protected int Offset, Length;

		protected MutexFreePipe([NotNull] string name, bool createBuffer)
		{
			name = name.Trim();
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
			Name = name;

			MemoryMappedFile mmFile = createBuffer
				? MemoryMappedFile.CreateNew(name + ".0", MINIMUM_BUFFER_SIZE, MemoryMappedFileAccess.ReadWrite)
				: MemoryMappedFile.OpenExisting(name + ".0");

			Buffer = new SafeMemoryMappedFile(mmFile);
			NewMessageSignal = new EventWaitHandle(false, EventResetMode.AutoReset, name + ".signal");

			Length = Buffer.Length;
			Offset = STARTING_OFFSET;
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ObjectHelper.Dispose(ref Buffer);
				ObjectHelper.Dispose(ref NewMessageSignal);
			}

			base.Dispose(disposing);
		}
	}
}