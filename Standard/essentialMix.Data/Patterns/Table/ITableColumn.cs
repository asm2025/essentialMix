using System;
using JetBrains.Annotations;

namespace essentialMix.Data.Patterns.Table
{
	public interface ITableColumn : ITableColumnSettings
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
		string Label { get; }
		string HeaderTitle { get; }
		int Size { get; set; }
		bool PrimaryKey { get; set; }
		bool Unique { get; set; }
		bool RowId { get; set; }
		bool AllowDbNull { get; set; }
		bool ReadOnly { get; set; }
		bool Aliased { get; set; }
		bool Expression { get; set; }
	
		ITableColumn Copy(string newName = null, Type newDataType = null);
	}
}