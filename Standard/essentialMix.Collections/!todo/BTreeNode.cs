using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace essentialMix.Collections
{
	[DebuggerDisplay("{Degree}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class BTreeNode<TNode, TEntry, T>
		
	{
		protected BTreeNode(int degree)
		{
			Degree = degree;
		}

		public int Degree { get; }
	}
}