using essentialMix.Patterns.Layout;
using essentialMix.Patterns.Text;

namespace essentialMix.Data.Patterns.Table;

public interface ITableColumnSettings
{
	bool? Formattable { get; set; }
	string Text { get; set; }
	string HeaderAbbr { get; set; }
	bool? Sortable { get; set; }
	bool? Searchable { get; set; }
	bool? Hidden { get; set; }
	int? Order { get; set; }
	HorizontalAlignment? Align { get; set; }
	int? Weight { get; set; }
	TableColumnFormatting? Formatting { get; set; }
	string CustomFormat { get; set; }
	TextCasing? TextCasing { get; set; }
	string Class { get; set; }
	string ThClass { get; set; }
	object ThStyle { get; set; }
	string TdClass { get; set; }
	string Variant { get; set; }
	bool? IsRowHeader { get; set; }
	void Apply(ITableColumnSettings column);
}