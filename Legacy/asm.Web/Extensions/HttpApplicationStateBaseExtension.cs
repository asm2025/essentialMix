using System;
using System.Linq;
using System.Web;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Web.Extensions
{
	public static class HttpApplicationStateBaseExtension
	{
		public static T GetValueOf<T>([NotNull] this HttpApplicationStateBase thisValue, [NotNull] string name, T defaultValue = default(T))
		{
			T result = defaultValue;
			if (name.Length == 0) return result;
			thisValue.Lock();

			string key = thisValue.AllKeys.FirstOrDefault(k => k.IsSame(name));
			if (key != null) result = thisValue[key].To(result);

			thisValue.UnLock();
			return result;
		}

		public static T GetValueOf<T>([NotNull] this HttpApplicationStateBase thisValue, int index, T defaultValue = default(T))
		{
			T result = defaultValue;
			thisValue.Lock();
			if (thisValue.Keys.Count > 0 && index.InRangeRx(0, thisValue.Keys.Count)) result = thisValue.Get(index).To(result);
			thisValue.UnLock();
			return result;
		}

		public static void SetValueOf<T>([NotNull] this HttpApplicationStateBase thisValue, [NotNull] string name, T value)
		{
			if (name.Length == 0) throw new ArgumentNullException(nameof(name));
			thisValue.Lock();

			string key = thisValue.AllKeys.FirstOrDefault(k => k.IsSame(name));

			if (value == null || value.Equals(default(T)))
			{
				if (key != null) thisValue.Remove(key);
			}
			else
			{
				key ??= name;
				thisValue[key] = value;
			}

			thisValue.UnLock();
		}
	}
}