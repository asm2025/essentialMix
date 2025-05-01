using System;

namespace essentialMix.Data.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class DataViewTypeAttribute(DataViewType viewType) : Attribute
{
	public DataViewType ViewType { get; set; } = viewType;
}