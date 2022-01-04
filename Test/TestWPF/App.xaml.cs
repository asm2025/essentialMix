using System.Windows;
using TestWPF.ViewModels;

namespace TestWPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	/// <inheritdoc />
	public App()
	{
	}

	/// <inheritdoc />
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		MainViewModel viewModel = new MainViewModel();
		MainWindow window = new MainWindow(viewModel);
		window.Show();
	}
}