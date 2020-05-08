using System;
using System.Data;

namespace asm.Data.Patterns.Provider
{
	public class DataSettings
	{
		public DataTable Schema;
		public Action<DataTable> OnMissingPrimaryKey;
		public Func<DataColumn, DataRow, object> OnNeedValue;
		public Predicate<DataColumn> ColumnsFilter;
		public Predicate<DataRow> RowsFilter;
	}
}