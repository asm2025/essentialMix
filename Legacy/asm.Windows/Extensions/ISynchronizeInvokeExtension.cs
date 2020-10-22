using System;
using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class ISynchronizeInvokeExtension
	{
		public static void InvokeIf([NotNull] this ISynchronizeInvoke thisValue, MethodInvoker action)
		{
			if (thisValue.InvokeRequired)
				thisValue.Invoke(action, Array.Empty<object>());
			else
				action();
		}
	}
}