using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;

namespace essentialMix.Threading
{
	//https://blogs.msdn.microsoft.com/salvapatuel/2009/06/08/working-with-memory-mapped-files-in-net-4/
	public sealed class SharedMemory<T> : Disposable
		where T : struct
	{
		private readonly string _lockName;

		private Mutex _mutex;
		private bool _isLocked;
		private MemoryMappedFile _mmf;
		private MemoryMappedViewAccessor _accessor;

		public SharedMemory([NotNull] string name, long size)
			: this(name, 0, size, MemoryMappedFileAccess.ReadWrite)
		{
		}

		public SharedMemory([NotNull] string name, long offset, long size)
			: this(name, offset, size, MemoryMappedFileAccess.ReadWrite)
		{
		}

		public SharedMemory([NotNull] string name, long size, MemoryMappedFileAccess access)
			: this(name, 0, size, access)
		{
		}

		public SharedMemory([NotNull] string name, long offset, long size, MemoryMappedFileAccess access)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (size < 1) throw new ArgumentOutOfRangeException(nameof(size));
			_lockName = string.Concat(name.ToUpper(), "_LOCK");
			Name = name;
			Offset = offset;
			Size = size;
			Access = access;
		}

		public string Name { get; }
		public long Offset { get; }
		public long Size { get; }
		public MemoryMappedFileAccess Access { get; }
		public bool IsCreated { get; private set; }
		public bool IsLocked => _isLocked;

		public T Data
		{
			get
			{
				ThrowIfDisposed();
				_accessor.Read(0, out T dataStruct);
				return dataStruct;
			}
			set
			{
				ThrowIfDisposed();

				try
				{
					_mutex.WaitOne();
					_accessor.Write(0, ref value);
				}
				finally
				{
					_mutex.ReleaseMutex();
				}
			}
		}

		public bool Open()
		{
			ThrowIfDisposed();
			if (IsCreated) throw new InvalidOperationException("Memory block is already created.");

			try
			{
				// Create named MMF
				_mmf = MemoryMappedFile.CreateOrOpen(Name, Size, Access);

				// Create accessors to MMF
				_accessor = _mmf.CreateViewAccessor(Offset, Size, Access);

				// Create lock
				_mutex = new Mutex(true, _lockName, out _isLocked);
				IsCreated = true;
				return true;
			}
			catch
			{
				return false;
			}
		}

		public bool OpenFromFile([NotNull] string path)
		{
			ThrowIfDisposed();
			if (IsCreated) throw new InvalidOperationException("Memory block is already created.");
			path = path.Trim();
			if (path.Length == 0) throw new ArgumentNullException(nameof(path));

			try
			{
				// Create named MMF
				_mmf = MemoryMappedFile.CreateFromFile(path, FileMode.OpenOrCreate, Name, Size, Access);

				// Create accessors to MMF
				_accessor = _mmf.CreateViewAccessor(Offset, Size, Access);

				// Create lock
				_mutex = new Mutex(true, _lockName, out _isLocked);
				IsCreated = true;
				return true;
			}
			catch
			{
				return false;
			}
		}

		public bool OpenFromFile([NotNull] FileStream stream, bool leaveOpen = false, HandleInheritability inheritability = HandleInheritability.None)
		{
			ThrowIfDisposed();
			if (IsCreated) throw new InvalidOperationException("Memory block is already created.");

			try
			{
				// Create named MMF
				_mmf = MemoryMappedFile.CreateFromFile(stream, Name, Size, Access, inheritability, leaveOpen);

				// Create accessors to MMF
				_accessor = _mmf.CreateViewAccessor(Offset, Size, Access);

				// Create lock
				_mutex = new Mutex(true, _lockName, out _isLocked);
				IsCreated = true;
				return true;
			}
			catch
			{
				return false;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ObjectHelper.Dispose(ref _accessor);
				ObjectHelper.Dispose(ref _mmf);
				if (_mutex != null)
				{
					_mutex.ReleaseMutex();
					_mutex.Close();
					ObjectHelper.Dispose(ref _mutex);
				}
			}
			base.Dispose(disposing);
		}
	}
}