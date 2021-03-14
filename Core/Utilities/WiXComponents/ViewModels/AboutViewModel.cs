using System.Reflection;
using asm.Extensions;
using asm.Helpers;

namespace WiXComponents.ViewModels
{
	public class AboutViewModel
	{
		private static readonly string __title;
		private static readonly string __company;
		private static readonly string __version;
		
		static AboutViewModel()
		{
			Assembly asm = AssemblyHelper.GetEntryAssembly();
			__title = asm.GetTitle();
			__company = asm.GetCompany();
			__version = asm.GetFileVersion();
		}

		public AboutViewModel() 
		{
		}

		public string Title => __title;
		public string Company => __company;
		public string Version => __version;
	}
}