using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using JetBrains.Annotations;

namespace essentialMix.Core.WPF.Controls;

public class SelectableTextBlock : TextBlock
{
	static SelectableTextBlock()
	{
		FocusableProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata(true));
		// remove the focus rectangle around the control
		FocusVisualStyleProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata(null));
		DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata(typeof(SelectableTextBlock)));
		TextEditorWrapper.RegisterCommandHandlers(typeof(SelectableTextBlock), true, true, true);
	}

	/// <inheritdoc />
	public SelectableTextBlock()
	{
		TextEditorWrapper.CreateFor(this);
	}

	/// <inheritdoc />
	public SelectableTextBlock([NotNull] Inline inline)
		: base(inline)
	{
		TextEditorWrapper.CreateFor(this);
	}
}