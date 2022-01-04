using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using essentialMix.Core.WPF.Helpers;
using essentialMix.Helpers;
using Image = System.Windows.Controls.Image;

namespace essentialMix.Core.WPF.Controls
{
	/// <inheritdoc cref="Button" />
	[TemplatePart(Name = "PART_Image", Type = typeof(Image))]
	[TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentControl))]
	public class UACShieldButton : Button
	{
		public static readonly DependencyProperty IconRequestedProperty = DependencyProperty.Register(nameof(IconRequested), typeof(bool), typeof(UACShieldButton), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender, OnIconRequestedChanged));
		public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(ImageSource), typeof(UACShieldButton), new FrameworkPropertyMetadata(ImageSourceHelper.FromSystem(SHSTOCKICONID.SIID_SHIELD), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender, OnIconChanged));

		private Image _image;

		static UACShieldButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(UACShieldButton), new FrameworkPropertyMetadata(typeof(UACShieldButton)));
		}

		/// <inheritdoc />
		public UACShieldButton()
		{
		}

		public bool IconRequested
		{
			get => (bool)GetValue(IconRequestedProperty);
			set => SetValue(IconRequestedProperty, value);
		}

		public ImageSource Icon
		{
			get => (ImageSource)GetValue(IconProperty);
			set => SetValue(IconProperty, value);
		}

		/// <inheritdoc />
		public override void OnApplyTemplate()
		{
			_image = Template.FindName("PART_Image", this) as Image;
			base.OnApplyTemplate();
		}

		private static void OnIconRequestedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			UACShieldButton button = (UACShieldButton)sender;
			if (button?._image == null) return;
			button._image.Visibility = (bool)args.NewValue && !WindowsIdentityHelper.HasElevatedPrivileges
									? Visibility.Visible
									: Visibility.Collapsed;
		}

		private static void OnIconChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			UACShieldButton button = (UACShieldButton)sender;
			if (button?._image == null) return;
			button._image.Source = args.NewValue as ImageSource;
			button._image.Visibility = (bool)sender.GetValue(IconRequestedProperty) && !WindowsIdentityHelper.HasElevatedPrivileges ? Visibility.Visible
											: Visibility.Collapsed;
		}
	}
}
