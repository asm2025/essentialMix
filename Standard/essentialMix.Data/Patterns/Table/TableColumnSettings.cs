using System;
using essentialMix.Extensions;
using essentialMix.Patterns.Layout;
using essentialMix.Patterns.Text;

namespace essentialMix.Data.Patterns.Table;

[Serializable]
public struct TableColumnSettings : ITableColumnSettings, ICloneable
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

	public void Apply(ITableColumnSettings column)
	{
		ITableColumnSettings target = this;
		Apply(ref target, column);
	}

	public static void Apply(ref ITableColumnSettings target, ITableColumnSettings overrides)
	{
		if (overrides == null) return;
		ApplyHeaderProps(ref target, overrides);
		ApplyStyle(ref target, overrides);

		static void ApplyHeaderProps(ref ITableColumnSettings target, ITableColumnSettings overrides)
		{
			if (overrides.Text != null) target.Text = overrides.Text;
			if (overrides.HeaderAbbr != null) target.HeaderAbbr = overrides.HeaderAbbr;
			if (overrides.Sortable.HasValue) target.Sortable = overrides.Sortable == true;
			if (overrides.Searchable.HasValue) target.Searchable = overrides.Searchable == true;
			if (overrides.Hidden.HasValue) target.Hidden = overrides.Hidden == true;
			if (overrides.Order.HasValue) target.Order = overrides.Order.Value;
			if (overrides.IsRowHeader.HasValue) target.IsRowHeader = overrides.IsRowHeader == true;
		}

		static void ApplyStyle(ref ITableColumnSettings target, ITableColumnSettings overrides)
		{
			if (overrides.Formattable.HasValue) target.Formattable = overrides.Formattable == true;
			if (overrides.Align.HasValue) target.Align = overrides.Align.Value;
			if (overrides.Weight.HasValue) target.Weight = overrides.Weight.Value;
			if (overrides.Formatting.HasValue) target.Formatting = overrides.Formatting;
			if (!string.IsNullOrEmpty(overrides.CustomFormat)) target.CustomFormat = overrides.CustomFormat;
			if (overrides.TextCasing.HasValue) target.TextCasing = overrides.TextCasing.Value;
			if (overrides.Class != null) target.Class = overrides.Class;
			if (overrides.ThClass != null) target.ThClass = overrides.ThClass;
			if (overrides.ThStyle != null) target.ThStyle = overrides.ThStyle;
			if (overrides.TdClass != null) target.TdClass = overrides.TdClass;
			if (overrides.Variant != null) target.Variant = overrides.Variant;
		}
	}
}