using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace essentialMix.Core.WPF.Controls
{
	/// <inheritdoc />
	[TemplatePart(Name = "PART_TextBlock", Type = typeof(TextBox))]
	[TemplatePart(Name = "PART_Button", Type = typeof(Button))]
	public class SelectFolder : Control
	{
		public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(SelectFolder), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(SelectFolder), new FrameworkPropertyMetadata("Select folder...", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		public static readonly DependencyProperty RootFolderProperty = DependencyProperty.Register(nameof(RootFolder), typeof(Environment.SpecialFolder), typeof(SelectFolder), new FrameworkPropertyMetadata(Environment.SpecialFolder.Desktop, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		public static readonly DependencyProperty SelectedPathProperty = DependencyProperty.Register(nameof(SelectedPath), typeof(string), typeof(SelectFolder), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		public static readonly DependencyProperty BrowseCommandProperty = DependencyProperty.Register(nameof(BrowseCommand), typeof(ICommand), typeof(SelectFolder), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

		static SelectFolder()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectFolder), new FrameworkPropertyMetadata(typeof(SelectFolder)));
		}

		public SelectFolder()
		{
		}

		public bool IsReadOnly
		{
			get => (bool)GetValue(IsReadOnlyProperty);
			set => SetValue(IsReadOnlyProperty, value);
		}

		public string Title
		{
			get => (string)GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}

		public Environment.SpecialFolder RootFolder
		{
			get => (Environment.SpecialFolder)GetValue(RootFolderProperty);
			set => SetValue(RootFolderProperty, value);
		}

		public string SelectedPath
		{
			get => (string)GetValue(SelectedPathProperty);
			set => SetValue(SelectedPathProperty, value);
		}

		public ICommand BrowseCommand
		{
			get => (ICommand)GetValue(BrowseCommandProperty);
			set => SetValue(BrowseCommandProperty, value);
		}
	}
}
