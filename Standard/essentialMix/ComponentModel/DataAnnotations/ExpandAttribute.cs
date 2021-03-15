using System;

namespace essentialMix.ComponentModel.DataAnnotations
{
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property)]
	public class ExpandAttribute : Attribute
	{
		/// <inheritdoc />
		public ExpandAttribute()
			: this(true)
		{
		}

		/// <inheritdoc />
		public ExpandAttribute(bool expanded)
		{
			Expanded = expanded;
		}

		public bool Expanded { get; set; }
	}
}
