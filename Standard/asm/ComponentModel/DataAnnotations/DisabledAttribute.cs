using System;

namespace asm.ComponentModel.DataAnnotations
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field)]
	public class DisabledAttribute : Attribute
	{
	}
}
