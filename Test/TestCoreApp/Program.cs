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

			Uri uri = new Uri("http://localhost?p1=Value&p2=A%20B%26p3%3DValue2");
			Console.WriteLine(uri);

			ConsoleHelper.Pause();
		}
	}
}
