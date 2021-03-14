using System;
using System.Windows;
using System.Windows.Input;
using WiXComponents.ViewModels;

namespace WiXComponents.Views
{
	/// <summary>
	/// Interaction logic for MainView.xaml
	/// </summary>
	public partial class MainView : Window
	{
		public MainView(MainViewModel viewModel)
		{
			DataContext = viewModel;
			InitializeComponent();
		}

		/// <inheritdoc />
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			Dispatcher.InvokeShutdown();
		}

		/// <inheritdoc />
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			DragMove();
			base.OnMouseLeftButtonDown(e);
		}

		private void MinimizeButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
