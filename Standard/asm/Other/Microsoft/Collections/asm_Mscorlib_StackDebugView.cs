using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace asm.Other.Microsoft.Collections
{
	/*
	 * VS IDE can't differentiate between types with the same name from different
	 * assembly. So we need to use different names for collection debug view for
	 * collections in this solution assemblies.
	 */
	internal sealed class asm_Mscorlib_StackDebugView<T>
	{
		private readonly Stack<T> _stack;

		public asm_Mscorlib_StackDebugView([NotNull] Stack<T> stack)
		{
			_stack = stack;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		[NotNull]
		public T[] Items => _stack.ToArray();
	}
}