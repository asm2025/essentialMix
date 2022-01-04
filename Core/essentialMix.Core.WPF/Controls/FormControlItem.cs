using System.Windows;
using System.Windows.Controls;

namespace essentialMix.Core.WPF.Controls;

public class FormControlItem : ContentControl
{
	public static DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(ContentControl), new FrameworkPropertyMetadata());

	static FormControlItem()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(FormControlItem), new FrameworkPropertyMetadata(typeof(FormControlItem)));
	}

	/// <inheritdoc />
	public FormControlItem() 
	{
	}

	public string Label
	{
		get => (string)GetValue(LabelProperty);
		set => SetValue(LabelProperty, value);
	}
}