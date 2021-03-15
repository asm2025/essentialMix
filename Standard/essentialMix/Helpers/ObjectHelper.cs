using System;

namespace essentialMix.Helpers
{
	public static class ObjectHelper
	{
		public static void Dispose<T>(ref T value)
			where T : class
		{
			switch (value)
			{
				case null:
					return;
				case IDisposable disposable:
					disposable.Dispose();
					break;
			}

			value = null;
		}

		public static void Dispose<T>(T value)
			where T : class
		{
			switch (value)
			{
				case null:
					return;
				case IDisposable disposable:
					disposable.Dispose();
					break;
			}
		}

		public static bool IsCompatible<T>(object value)
		{
			// Non-null values are fine. Only accept nulls if T is a class or Nullable<U>.
			// Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
			return value is T || value == null && default(T) == null;
		}
	}
}