using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class RichTextBoxExtension
{
	public static void CaretToEnd([NotNull] this RichTextBox thisValue)
	{
		thisValue.InvokeIf(() =>
		{
			thisValue.SelectionStart = thisValue.TextLength;
			thisValue.SelectionLength = 0;
		});
	}

	public static void AppendText([NotNull] this RichTextBox thisValue, char value)
	{
		thisValue.InvokeIf(() => thisValue.SelectedText += value);
	}

	public static void AppendText([NotNull] this RichTextBox thisValue, char value, Color color)
	{
		thisValue.InvokeIf(() =>
		{
			thisValue.SelectionColor = color;
			thisValue.SelectedText += value;
			thisValue.SelectionColor = thisValue.ForeColor;
		});
	}

	public static void AppendText([NotNull] this RichTextBox thisValue, string value, Color color)
	{
		thisValue.InvokeIf(() =>
		{
			thisValue.SelectionColor = color;
			thisValue.AppendText(value);
			thisValue.SelectionColor = thisValue.ForeColor;
		});
	}
}