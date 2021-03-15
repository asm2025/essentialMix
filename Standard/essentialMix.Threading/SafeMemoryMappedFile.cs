using System.IO.MemoryMappedFiles;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;

namespace essentialMix.Threading
{
	public class SafeMemoryMappedFile : Disposable
	{
		private MemoryMappedFile _mmFile;
		private MemoryMappedViewAccessor _accessor;
		private unsafe byte* _pointer;

		public unsafe SafeMemoryMappedFile(MemoryMappedFile mmFile)
		{
			_mmFile = mmFile;
			_accessor = _mmFile.CreateViewAccessor();
			_pointer = (byte*)_accessor.SafeMemoryMappedViewHandle.DangerousGetHandle().ToPointer();
			Length = (int)_accessor.Capacity;
		}

		/// <inheritdoc />
		protected override unsafe void Dispose(bool disposing)
		{
			if (disposing)
			{
				ObjectHelper.Dispose(ref _accessor);
				ObjectHelper.Dispose(ref _mmFile);
				_pointer = null;
			}

			base.Dispose(disposing);
		}

		public int Length { get; }

		public MemoryMappedViewAccessor Accessor
		{
			get
			{
				ThrowIfDisposedOrDisposing();
				return _accessor;
			}
		}

		public unsafe byte* Pointer
		{
			get
			{
				ThrowIfDisposedOrDisposing();
				return _pointer;
			}
		}
	}
}