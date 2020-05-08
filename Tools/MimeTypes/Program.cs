using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using asm.Collections;
using asm.Extensions;
using asm.Helpers;
using CsvHelper;
using Humanizer;

namespace MimeTypes
{
	internal class Program
	{
		/*
		 * the subfolder where the csv files resides
		 * to update these files, download them from
		 * https://www.iana.org/assignments/media-types/media-types.xhtml
		 */
		private const string DATA_DIR = "Data";
		private const string OUTPUT_FILE = "MediaTypeNames.cs";

		private static readonly string[] EXCLUDE =
		{
			"obsoleted",
			"deprecated"
		};

		private static readonly IReadOnlyDictionary<char, string> NUMBERS = new ReadOnlyDictionary<char, string>(new Dictionary<char, string>
		{
			{ '0', "Zero" },
			{ '1', "One" },
			{ '2', "Two" },
			{ '3', "Three" },
			{ '4', "Four" },
			{ '5', "Five" },
			{ '6', "Six" },
			{ '7', "Seven" },
			{ '8', "Eight" },
			{ '9', "Nine" }
		});

		private static void Main()
		{
			Console.OutputEncoding = Encoding.UTF8;
			
			string csvLocation = Path.Combine(Directory.GetCurrentDirectory(), DATA_DIR);
			if (File.Exists(OUTPUT_FILE)) File.Delete(OUTPUT_FILE);

			Tree<string, string> mimes = new Tree<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach (string file in Directory.EnumerateFiles(csvLocation, "*.csv"))
			{
				ProcessFile(file, mimes);
			}

			if (mimes.Count == 0) return;

			Console.WriteLine($"{Environment.NewLine}Writing file '{OUTPUT_FILE}'...{Environment.NewLine}");
			StreamWriter streamWriter = null;

			try
			{
				streamWriter = new StreamWriter(OUTPUT_FILE, false, Encoding.UTF8);
				streamWriter.WriteLine(@"namespace asm.Web
{
	public static class MediaTypeNames
	{");
				
				foreach (Tree<string, string> root in EnumerateNode(mimes))
				{
					Console.WriteLine($"{Environment.NewLine}Writing '{root.Key}' values...{Environment.NewLine}");
					WriteEntry(streamWriter, root, 0);
					streamWriter.Flush();
				}

				streamWriter.WriteLine(@"	}
}");

				Console.WriteLine("OK, done :)");
				
				Process.Start("notepad.exe",  Path.Combine(Directory.GetCurrentDirectory(), OUTPUT_FILE));
			}
			finally
			{
				ObjectHelper.Dispose(ref streamWriter);
			}

			static void ProcessFile(string fileName, Tree<string, string> root)
			{
				string baseName = Path.GetFileNameWithoutExtension(fileName);
				string className = GetKey(baseName);
				if (className == null) return;
				Console.WriteLine($"{Environment.NewLine}Processing file '{className}'...{Environment.NewLine}");

				if (!root.TryGetValue(className, out Tree<string, string> node))
				{
					node = new Tree<string, string>(root.Comparer)
					{
						Key = className
					};
					root[className] = node;
				}

				StreamReader reader = null;
				CsvReader csvReader = null;

				try
				{
					reader = new StreamReader(fileName ?? throw new ArgumentNullException(nameof(fileName)), Encoding.UTF8);
					csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

					while (csvReader.Read())
					{
						string entry = csvReader.GetField(0);
						string fullKey = GetKey(entry);
						if (fullKey == null) continue;
						string value = csvReader.GetField(1).ToNullIfEmpty() ?? $"{baseName}/{entry}";

						if (!fullKey.Contains('.'))
						{
							if (!node.TryGetValue(fullKey, out Tree<string, string> item))
							{
								item = new Tree<string, string>(node.Comparer)
								{
									Key = fullKey,
									Value = value
								};

								node.Add(item);
							}
						}
						else
						{
							string[] keys = fullKey.Split(StringSplitOptions.RemoveEmptyEntries, '.');
							Tree<string, string> current = node;

							for (int i = 0; i < keys.Length; i++)
							{
								string key = keys[i];

								if (!current.TryGetValue(key, out Tree<string, string> item))
								{
									item = new Tree<string, string>(current.Comparer)
									{
										Key = key
									};

									current.Add(item);
								}

								if (i == keys.Length - 1) item.Value = value;
								current = item;
							}
						}

						Console.WriteLine($"{entry} => {fullKey} = '{value}'");
					}
				}
				finally
				{
					ObjectHelper.Dispose(ref csvReader);
					ObjectHelper.Dispose(ref reader);
				}
			}

			static string GetKey(string name)
			{
				name = name.ToNullIfEmpty();
				if (name == null || name.ContainsAny(true, EXCLUDE)) return null;

				StringBuilder sb = new StringBuilder(name.Length);

				for (int i = 0; i < name.Length; i++)
				{
					char c = name[i];
					bool atSentence = sb.Length == 0 || sb[sb.Length - 1] == '.';
					bool atWord = atSentence || char.IsWhiteSpace(sb[sb.Length - 1]);

					if ((c == '-' || c == '_') && !atWord && char.IsDigit(sb[sb.Length - 1]) && i < name.Length - 1 && char.IsDigit(name[i + 1]))
					{
						sb.Append('_');
						continue;
					}

					if (char.IsDigit(c))
					{
						if (atSentence)
						{
							int x, n;
							bool xe = false;

							for (n = i + 1, x = n; n < name.Length && char.IsLetterOrDigit(name[n]); n++)
							{
								if (xe) continue;
								if (char.IsDigit(name[n])) x++;
								else xe = true;
							}

							n -= i; x -= i;

							if (n == 1)
							{
								sb.Append(NUMBERS[c]);
							}
							else
							{
								if (x == 1) sb.Append(NUMBERS[c]);
								else if (long.TryParse(name.Substring(i, x), out long num)) sb.Append(num.ToWords().ToPascalCase());
								else x = 0;

								if (n > x)
								{
									sb.Append(char.ToUpperInvariant(name[i + x++]));
									if (n > x) sb.Append(name.Substring(i + x, n - x));
								}
							}

							i += n;
							if (sb.Length > 0 && !char.IsWhiteSpace(sb[sb.Length - 1])) sb.Append(' ');
							continue;
						}

						sb.Append(c);
						continue;
					}

					if (c != '.' && !char.IsLetter(c))
					{
						if (sb.Length > 0 && !char.IsWhiteSpace(sb[sb.Length - 1])) sb.Append(' ');
						continue;
					}

					sb.Append(atWord
								? char.ToUpperInvariant(c)
								: c);
				}

				sb.Replace(" ", string.Empty);
				return sb.ToString();
			}

			static IEnumerable<Tree<string, string>> EnumerateNode(Tree<string, string> root)
			{
				return root.Values.OrderBy(e => e.Count > 0
													? 1
													: 0)
							.ThenBy(e => e.Key);
			}

			static void WriteEntry(TextWriter writer, Tree<string, string> node, int level)
			{
				const string CS_B_DEF = @"
{0}public static class {1}
{0}{{";
				const string CS_E_DEF = "{0}}}";
				const string VAR_DEC = "{0}public const string {1} = \"{2}\";";

				int tabs = level + 2;
				string indent = new string('\t', tabs);
				bool isValue = node.Count == 0;

				if (isValue)
				{
					if (string.IsNullOrEmpty(node.Value)) return;
					writer.WriteLine(VAR_DEC, indent, node.Key, node.Value);
					return;
				}

				writer.WriteLine(CS_B_DEF, indent, node.Key);
				int childLevel = level + 1;

				foreach (Tree<string, string> child in EnumerateNode(node))
				{
					WriteEntry(writer, child, childLevel);
				}

				writer.WriteLine(CS_E_DEF, indent);
			}
		}
	}
}
