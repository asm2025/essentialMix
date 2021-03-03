using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using asm.Data.Helpers;
using asm.Extensions;
using asm.Helpers;

namespace CmpIdRep
{
	internal class Program
	{
		private static int Main(string[] args)
		{
			//if (args == null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
			//{
			//	Console.WriteLine("No input file was provided.");
			//	return 1;
			//}

			//string fileName = PathHelper.Trim(args[0]);

			//if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
			//{
			//	Console.WriteLine("File not found.");
			//	return 1;
			//}

			//List<string> files = new List<string>();

			//try
			//{
			//	XmlDocument doc = XmlDocumentHelper.LoadFile(fileName);
			//	XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
			//	manager.AddNamespace("msbld", "http://schemas.microsoft.com/developer/msbuild/2003");

			//	// Finds all of the files included in the project.
			//	XmlNodeList nodes = doc.SelectNodes("/")
			//}
			//catch (Exception ex)
			//{
			//	Console.WriteLine(ex.CollectMessages());
			//	return 1;
			//}

			//string path = Path.GetDirectoryName(fileName) ?? Directory.GetCurrentDirectory();
			return 0;
		}
	}
}
