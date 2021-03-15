using System;

namespace essentialMix.ComponentModel.DataAnnotations
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field)]
	public class DisabledAttribute : Attribute
	{
	}
}
