using System;
using System.Collections;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;

namespace asm.Caching
{
	public sealed class CacheKeyBuilder
	{
		private static readonly string NULL_STRING = Guid.NewGuid().ToString();
		private readonly StringBuilder _builder = new StringBuilder();

		/// <summary>
		/// Adds the given value to the key
		/// </summary>
		[NotNull]
		public CacheKeyBuilder By(object value)
		{
			_builder.Append('{'); // wrap each value in curly braces

			// DateTime is covered by IConvertible, but the default ToString() implementation
			// doesn't have enough granularity to distinguish between unequal DateTimes
			switch (value)
			{
				case null:
					_builder.Append(NULL_STRING);
					break;
				case DateTime dateTimeValue:
					_builder.Append(dateTimeValue.Ticks);
					break;
				case IConvertible convertibleValue:
					_builder.Append(convertibleValue.ToString(CultureInfo.InvariantCulture));
					break;
				case Type typeValue:
					_builder.Append(typeValue.GUID);
					break;
				case IEnumerable enumerableValue:
					foreach (object element in enumerableValue)
						By(element);
					break;
				case ICacheKey cacheKeyValue:
					cacheKeyValue.BuildCacheKey(this);
					break;
				default:
					throw new ArgumentException($"{value.GetType()} cannot be a cache key");
			}

			_builder.Append('}');
			return this;
		}

		[NotNull]
		public override string ToString() { return _builder.ToString(); }
	}
}