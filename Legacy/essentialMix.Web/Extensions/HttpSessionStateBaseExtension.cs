using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.SessionState;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class HttpSessionStateBaseExtension
	{
		[SuppressMessage("ReSharper", "ConvertIfStatementToNullCoalescingExpression")]
		public static T Get<T>(this HttpSessionState thisValue, [NotNull] string name, [NotNull] Func<T> onDefault)
		{
			T value;

			if (thisValue != null)
			{
				lock(thisValue.SyncRoot)
				{
					value = (T)thisValue[name];

					if (ReferenceEquals(value, null))
					{
						value = onDefault();
						thisValue[name] = value;
					}
				}
			}
			else
			{
				value = onDefault();
			}

			return value;
		}

		public static T Get<T>(this HttpSessionState thisValue, [NotNull] string name, T defaultValue = default(T))
		{
			return Get(thisValue, name, () => defaultValue);
		}

		public static void Set<T>([NotNull] this HttpSessionState thisValue, [NotNull] string name, T value)
		{
			lock (thisValue.SyncRoot)
			{
				thisValue[name] = value;
			}
		}
	}
}