using System;
using asm.Extensions;
using asm.Patterns.Layout;
using asm.Patterns.Text;

namespace asm.Data.Patterns.Table
{
	[Serializable]
	public struct TableColumnSettings : ICloneable
	{
		public static TableColumnSettings Empty { get; } = new TableColumnSettings();

		public bool? Formattable { get; set; }
		public string Text { get; set; }
		public string HeaderAbbr { get; set; }
		public bool? Sortable { get; set; }
		public bool? Searchable { get; set; }
		public bool? Hidden { get; set; }
		public int? Order { get; set; }
		public HorizontalAlignment? Align { get; set; }
		public int? Weight { get; set; }

		public TableColumnFormatting? Formatting { get; set; }

		public string CustomFormat { get; set; }

		public TextCasing? TextCasing { get; set; }

		public string Class { get; set; }
		
		public string ThClass { get; set; }
		public object ThStyle { get; set; }
		public string TdClass { get; set; }
		public string Variant { get; set; }
		public bool? IsRowHeader { get; set; }

		/// <inheritdoc />
		public object Clone() { return this.CloneMemberwise(); }

		public void Apply(ITableColumn column)
		{
			if (Formattable.HasValue) column.Formattable = Formattable == true;
			if (Text != null) column.Text = Text;
			if (HeaderAbbr != null) column.HeaderAbbr = HeaderAbbr;
			if (Sortable.HasValue) column.Sortable = Sortable == true;
			if (Searchable.HasValue) column.Searchable = Searchable == true;
			if (Hidden.HasValue) column.Hidden = Hidden == true;
			if (Order.HasValue) column.Order = Order.Value;
			if (Align.HasValue) column.Align = Align.Value;
			if (Weight.HasValue) column.Weight = Weight.Value;
			if (Formatting.HasValue) column.Formatting = Formatting;
			if (!string.IsNullOrEmpty(CustomFormat)) column.CustomFormat = CustomFormat;
			if (TextCasing.HasValue) column.TextCasing = TextCasing.Value;
			if (Class != null) column.Class = Class;
			if (ThClass != null) column.ThClass = ThClass;
			if (ThStyle != null) column.ThStyle = ThStyle;
			if (TdClass != null) column.TdClass = TdClass;
			if (Variant != null) column.Variant = Variant;
			if (IsRowHeader.HasValue) column.IsRowHeader = IsRowHeader == true;
		}

		public static void Merge(ref TableColumnSettings target, TableColumnSettings? overrides)
		{
			if (!overrides.HasValue) return;

			TableColumnSettings settings = overrides.Value;
			if (settings.Formattable.HasValue) target.Formattable = settings.Formattable == true;
			if (settings.Text != null) target.Text = settings.Text;
			if (settings.HeaderAbbr != null) target.HeaderAbbr = settings.HeaderAbbr;
			if (settings.Sortable.HasValue) target.Sortable = settings.Sortable == true;
			if (settings.Searchable.HasValue) target.Searchable = settings.Searchable == true;
			if (settings.Hidden.HasValue) target.Hidden = settings.Hidden == true;
			if (settings.Order.HasValue) target.Order = settings.Order.Value;
			if (settings.Align.HasValue) target.Align = settings.Align.Value;
			if (settings.Weight.HasValue) target.Weight = settings.Weight.Value;
			if (settings.Formatting.HasValue) target.Formatting = settings.Formatting;
			if (!string.IsNullOrEmpty(settings.CustomFormat)) target.CustomFormat = settings.CustomFormat;
			if (settings.TextCasing.HasValue) target.TextCasing = settings.TextCasing.Value;
			if (settings.Class != null) target.Class = settings.Class;
			if (settings.ThClass != null) target.ThClass = settings.ThClass;
			if (settings.ThStyle != null) target.ThStyle = settings.ThStyle;
			if (settings.TdClass != null) target.TdClass = settings.TdClass;
			if (settings.Variant != null) target.Variant = settings.Variant;
			if (settings.IsRowHeader.HasValue) target.IsRowHeader = settings.IsRowHeader == true;
		}
	}
}