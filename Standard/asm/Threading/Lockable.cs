using JetBrains.Annotations;

namespace asm.Threading
{
	public class Lockable<T>
		where T : class
	{
		[NotNull]
		private readonly object _syncRoot = new object();

		private T _value;

		/// <inheritdoc />
		public Lockable() 
			: this(null)
		{
		}

		public Lockable(T value)
		{
			_value = value;
		}

		public T Value
		{
			get => _value;
			set
			{
				lock(_syncRoot)
				{
					_value = value;
				}
			}
		}

		public static implicit operator T([NotNull] Lockable<T> lockable) { return lockable.Value; }
	}
}