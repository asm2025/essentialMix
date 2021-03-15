using System.Windows.Forms;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class NumericUpDownExtension
	{
		public static void Reset([NotNull] this NumericUpDown thisValue, bool disable = false)
		{
			thisValue.Minimum = 0;
			thisValue.Maximum = 100;
			thisValue.Increment = 1;
			thisValue.Value = 0;
			if (!disable) return;
			thisValue.Enabled = false;
		}
	}
}