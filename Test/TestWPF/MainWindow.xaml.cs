using System;
using System.Windows;

namespace TestWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <inheritdoc />
		public MainWindow()
		{
			InitializeComponent();
		}

		/// <inheritdoc />
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			Dispatcher.InvokeShutdown();
		}
	}
}
