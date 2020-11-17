using System;
using System.Text;
using asm.Helpers;

// ReSharper disable UnusedMember.Local
namespace TestCoreApp
{
	internal class Program
	{
		private static void Main()
		{
			Console.OutputEncoding = Encoding.UTF8;

			Uri uri = UriHelper.Combine("/files/images/users/", "test page.html");
			Console.WriteLine(uri);
			ConsoleHelper.Pause();
		}
	}
}
