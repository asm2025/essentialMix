using System.Reflection;
using asm.Extensions;
using asm.Helpers;
using Microsoft.Extensions.Logging;

namespace WiXComponents.ViewModels
{
	/// <inheritdoc />
	public abstract class AppViewModel : ViewModelBase
	{
		private static readonly string __title;
		private static readonly string __company;
		private static readonly string __version;

		static AppViewModel()
		{
			Assembly asm = AssemblyHelper.GetEntryAssembly();
			__title = asm.GetTitle();
			__company = asm.GetCompany();
			__version = asm.GetFileVersion();
		}

		/// <inheritdoc />
		protected AppViewModel(ILogger logger)
			: base(logger)
		{
		}

		public string Title => __title;

		public string Company => __company;

		public string Version => __version;
	}
}