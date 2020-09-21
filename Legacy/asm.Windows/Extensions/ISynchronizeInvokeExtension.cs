using System;
using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class ISynchronizeInvokeExtension
	{
		private static readonly object[] EMPTY_PARAMS = Array.Empty<object>();

		public static void InvokeIf([NotNull] this ISynchronizeInvoke thisValue, MethodInvoker action)
		{
			//bug ??? lost in threads
			if (thisValue.InvokeRequired)
				thisValue.Invoke(action, EMPTY_PARAMS);
			else
				action();
		}
	}
}