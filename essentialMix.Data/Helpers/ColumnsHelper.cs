using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using essentialMix.Extensions;
using essentialMix.Data.Annotations;

namespace essentialMix.Data.Helpers;

public static class ColumnsHelper
{
	public static IEnumerable<TColumn> GetColumns<TColumn>(object obj, Predicate<TColumn> columnFilter = null) { return GetColumns(obj, DataViewType.None, columnFilter); }

	public static IEnumerable<TColumn> GetColumns<TColumn>(object obj, DataViewType viewType, Predicate<TColumn> columnFilter = null)
	{
		switch (obj)
		{
			case null:
				return Enumerable.Empty<TColumn>();
			case DataTable dt:
				if (columnFilter != null && !typeof(TColumn).Is<DataColumn>()) throw new ArgumentException($"Column filter must receive an argument of type '{nameof(DataColumn)}' or a type derived from it.");
				return DataColumns(dt);
			default:
				if (columnFilter != null && !typeof(TColumn).Is<PropertyInfo>()) throw new ArgumentException($"Column filter must receive an argument of type '{nameof(PropertyInfo)}' or a type derived from it.");
				return Properties();
		}

		IEnumerable<TColumn> DataColumns(DataTable dt)
		{
			IEnumerable<TColumn> columns = columnFilter == null
												? dt.Columns.OfType<TColumn>()
												: dt.Columns.OfType<TColumn>()
													.Where(c => columnFilter(c));

			foreach (TColumn column in columns)
				yield return column;
		}

		IEnumerable<TColumn> Properties()
		{
			Type templateType = obj.AsType();
			IEnumerable<PropertyInfo> properties = templateType.GetProperties(essentialMix.Constants.BF_PUBLIC_INSTANCE);
				
			if (viewType != DataViewType.None)
			{
				properties = properties.Where(p =>
				{
					DataViewTypeAttribute dataViewType = p.GetAttribute<DataViewTypeAttribute>(true);
					return dataViewType == null || dataViewType.ViewType.FastHasFlag(viewType);
				});
			}

			IEnumerable<TColumn> propertiesCast = properties.Cast<TColumn>();
			if (columnFilter != null) propertiesCast = propertiesCast.Where(p => columnFilter(p));

			foreach (TColumn column in propertiesCast)
				yield return column;
		}
	}
}