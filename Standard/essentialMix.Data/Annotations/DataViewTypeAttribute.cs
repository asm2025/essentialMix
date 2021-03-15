using System;

namespace essentialMix.Data.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public class DataViewTypeAttribute : Attribute
	{
		public DataViewTypeAttribute(DataViewType viewType) { ViewType = viewType; }

		public DataViewType ViewType { get; set; }
	}
}
