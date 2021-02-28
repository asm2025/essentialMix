using System.Windows;
using WIXToolsetComponents.ViewModel;

namespace WIXToolsetComponents.View
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			Model = new MainViewModel();
			DataContext = Model;
			InitializeComponent();
		}

		public MainViewModel Model { get; }
	}
}
