using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Other.TylerBrinkley.Enumeration
{
	public interface IEnumInfoCommon
	{
		[NotNull]
		Type EnumType { get; }
		TypeCode TypeCode { get; }
		Type UnderlyingType { get; }
		bool IsFlagEnum { get; }

		int GetCount(EnumMemberSelection selection = EnumMemberSelection.All);
		int GetFlagCount();
		IEnumerable<string> GetNames(EnumMemberSelection selection = EnumMemberSelection.All);
		IEnumerable<string> GetDisplayNames(EnumMemberSelection selection = EnumMemberSelection.All);
	}
}