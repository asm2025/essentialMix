using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using asm.Collections;

namespace asm.Extensions
{
	public static class LockableExtension
	{
		[NotNull] public static ProtectableList AsLockable([NotNull] this IList thisValue) { return new ProtectableList(thisValue); }

		[NotNull] public static ProtectableList<T> AsLockable<T>([NotNull] this IList<T> thisValue) { return new ProtectableList<T>(thisValue); }
	}
}