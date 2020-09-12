using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using asm.Extensions;
using asm.Exceptions;
using JetBrains.Annotations;

namespace asm.ComponentModel
{
	/// <inheritdoc />
	public class EnumDisplayNameConverter : EnumConverter
	{
		private readonly Type _enumType;

		/// <inheritdoc />
		public EnumDisplayNameConverter([NotNull] Type type) 
			: base(type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			if (!type.IsEnum) throw new NotEnumTypeException();
			_enumType = type;
		}

		/// <inheritdoc />
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) { return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType); }

		/// <inheritdoc />
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			switch (value)
			{
				case null:
					return null;
				case string s:
					try
					{
						foreach (FieldInfo fi in _enumType.GetFields())
						{
							string dna = fi.GetDisplayName(fi.Name);
							if (s == dna) return Enum.Parse(_enumType, fi.Name);
						}

						return Enum.Parse(_enumType, s);
					}
					catch
					{
						// ignored
					}

					break;
			}

			return base.ConvertFrom(context, culture, value);
		}

		/// <inheritdoc />
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string) || base.CanConvertFrom(context, destinationType);
		}

		/// <inheritdoc />
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value == null) return null;
			if (destinationType != _enumType) return base.ConvertTo(context, culture, value, destinationType);

			try
			{
				string name = Enum.GetName(_enumType, value);
				return _enumType.GetField(name).GetDisplayName(name);
			}
			catch
			{
				return base.ConvertTo(context, culture, value, destinationType);
			}
		}
	}
}