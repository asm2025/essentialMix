using System;
using JetBrains.Annotations;
using essentialMix.Patterns.Layout;
using essentialMix.Patterns.Text;

namespace essentialMix.Data.Patterns.Table
{
	public interface ITableColumn
	{
		[NotNull]
		string Name { get; }
		
		[NotNull]
		string Value { get; }
		
		[NotNull]
		string Key { get; }
		
		[NotNull]
		Type DataType { get; }
		
		[NotNull]
		string DataTypeName { get; }
		bool Formattable { get; set; }
		string Text { get; set; }
		string Label { get; }
		string HeaderTitle { get; }
		string HeaderAbbr { get; set; }
		int Size { get; set; }
		bool PrimaryKey { get; set; }
		bool Unique { get; set; }
		bool RowId { get; set; }
		bool AllowDbNull { get; set; }
		bool Sortable { get; set; }
		bool Searchable { get; set; }
		bool ReadOnly { get; set; }
		bool Aliased { get; set; }
		bool Expression { get; set; }
		bool Hidden { get; set; }
		int Order { get; set; }
		HorizontalAlignment Align { get; set; }
		int? Weight { get; set; }
		TableColumnFormatting? Formatting { get; set; }
		string CustomFormat { get; set; }
		TextCasing TextCasing { get; set; }
		string Class { get; set; }
		string ThClass { get; set; }
		object ThStyle { get; set; }
		string TdClass { get; set; }
		string Variant { get; set; }
		bool IsRowHeader { get; set; }
	
		ITableColumn Copy(string newName = null, Type newDataType = null);
	}
}