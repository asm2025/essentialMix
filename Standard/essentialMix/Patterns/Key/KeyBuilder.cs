using System;
using System.Collections;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Key
{
	public sealed class KeyBuilder
	{
		private readonly StringBuilder _builder = new StringBuilder();

		/// <summary>
		/// Adds the given value to the key
		/// </summary>
		[NotNull]
		public KeyBuilder By(object value)
		{
			_builder.Append('{'); // wrap each value in curly braces

			// DateTime is covered by IConvertible, but the default ToString() implementation
			// doesn't have enough granularity to distinguish between unequal DateTimes
			switch (value)
			{
				case null:
					_builder.Append(Guid.NewGuid());
					break;
				case System.DateTime dateTime:
					_builder.Append(dateTime.Ticks);
					break;
				case IConvertible convertible:
					_builder.Append(convertible.ToString(CultureInfo.InvariantCulture));
					break;
				case Type type:
					_builder.Append(type.GUID);
					break;
				case IEnumerable enumerable:
					_builder.Append(enumerable.GetHashCode());
					break;
				case IKeyBuilder keyBuilder:
					keyBuilder.Build(this);
					break;
				default:
					throw new ArgumentException($"{value.GetType()} cannot be a key");
			}

			_builder.Append('}');
			return this;
		}

		[NotNull]
		public override string ToString() { return _builder.ToString(); }
	}
}