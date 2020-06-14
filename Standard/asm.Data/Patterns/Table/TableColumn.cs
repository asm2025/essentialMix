using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Patterns.Layout;
using asm.Patterns.String;

namespace asm.Data.Patterns.Table
{
	[DebuggerDisplay("{Name}")]
	[Serializable]
	public class TableColumn : ITableColumn
	{
		private string _text;

		public TableColumn([NotNull] string name, [NotNull] Type dataType)
		{
			name = name.Trim();
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
			Name = _text = name;
			DataType = dataType;
			DataTypeName = dataType.ToString();
			SetTypeDefaults(dataType);
		}

		/// <inheritdoc />
		public string Name { get; }

		/// <inheritdoc />
		public string Value => Name;

		/// <inheritdoc />
		public string Key => Name;

		/// <inheritdoc />
		public Type DataType { get; }

		/// <inheritdoc />
		public string DataTypeName { get; }

		public bool Formattable { get; set; }

		/// <inheritdoc />
		public string Text
		{
			get => _text;
			set => _text = value ?? Name;
		}

		/// <inheritdoc />
		public string Label => Text;

		/// <inheritdoc />
		public string HeaderTitle => Text;

		/// <inheritdoc />
		public string HeaderAbbr { get; set; }

		/// <inheritdoc />
		public int Size { get; set; }

		/// <inheritdoc />
		public bool PrimaryKey { get; set; }

		/// <inheritdoc />
		public bool Unique { get; set; }

		/// <inheritdoc />
		public bool RowId { get; set; }

		/// <inheritdoc />
		public bool AllowDbNull { get; set; }

		/// <inheritdoc />
		public bool Sortable { get; set; }

		/// <inheritdoc />
		public bool Searchable { get; set; }

		/// <inheritdoc />
		public bool ReadOnly { get; set; }

		/// <inheritdoc />
		public bool Aliased { get; set; }

		/// <inheritdoc />
		public bool Expression { get; set; }

		/// <inheritdoc />
		public bool Hidden { get; set; }

		/// <inheritdoc />
		public int Order { get; set; }

		/// <inheritdoc />
		public HorizontalAlignment Align { get; set; }

		/// <inheritdoc />
		public int? Weight { get; set; }

		public TableColumnFormatting? Formatting { get; set; }

		public string CustomFormat { get; set; }

		public TextCasing TextCasing { get; set; }

		public string Class { get; set; }

		public string ThClass { get; set; }
		public object ThStyle { get; set; }
		public string TdClass { get; set; }
		public string Variant { get; set; }
		public bool IsRowHeader { get; set; }

		[NotNull]
		public ITableColumn Copy(string newName = null, Type newDataType = null)
		{
			newName = newName?.Trim();
			if (string.IsNullOrEmpty(newName)) newName = null;
			newName ??= Name;
			if (newDataType == null) newDataType = DataType;
			TableColumn column = new TableColumn(newName, newDataType)
			{
				Text = Text,
				HeaderAbbr = HeaderAbbr,
				Size = Size,
				PrimaryKey = PrimaryKey,
				Unique = Unique,
				RowId = RowId,
				AllowDbNull = AllowDbNull,
				ReadOnly = ReadOnly,
				Aliased = Aliased,
				Expression = Expression,
				Hidden = Hidden,
				Order = Order,
				Align = Align,
				Weight = Weight,
				Formatting = Formatting,
				CustomFormat = CustomFormat,
				TextCasing = TextCasing,
				Class = Class,
				ThClass = ThClass,
				ThStyle = ThStyle,
				TdClass = TdClass,
				Variant = Variant,
				IsRowHeader = IsRowHeader,
			};

			if (newDataType != DataType) column.SetTypeDefaults(newDataType);
			return column;
		}

		private void SetTypeDefaults(Type type)
		{
			type = type.ResolveType();
			ISet<Type> interfaces = new HashSet<Type>(type.GetInterfaces(false));
			Formattable = interfaces.Contains(typeof(IFormattable));
			Sortable = interfaces.Contains(typeof(IComparable)) || interfaces.Any(e => e.IsGenericType && e.GetGenericTypeDefinition() == typeof(IComparable<>));
			Searchable = interfaces.Any(e => e.IsGenericType && e.GetGenericTypeDefinition() == typeof(IEquatable<>));
		}
	}
}