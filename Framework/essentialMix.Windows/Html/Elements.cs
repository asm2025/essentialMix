using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Windows.Html
{
	/// <summary>
	/// List of elements (HTML elements or STYLE elements)
	/// </summary>
	/// <remarks>
	/// Class has a parse function which
	/// 1. Parses the HTML text into HTML elements ([NotNull] this is done by HTMLParser object)
	/// 2. Traverse through HTML elements, examine them and inserts corresponding Style
	/// elements in between HTML elements whenever style is changed
	/// (style is changed by HTML tags such as �b�, �p�...)
	/// </remarks>
	public class Elements
	{
		private readonly List<Element> _elements;

		public Elements() { _elements = new List<Element>(); }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(1000);
			foreach (Element elem in _elements)
			{
				sb.Append(elem);
				sb.Append("\n");
			}
			return sb.ToString();
		}

		public IList<Element> Value => _elements;

		[NotNull]
		public Status Parse(string lineOfText, [NotNull] Status status)
		{
			_elements.Clear();
			lineOfText = lineOfText.Replace("\n", string.Empty);
			lineOfText = lineOfText.Replace("\r", string.Empty);

			Stack<StrBrush> brushes = new Stack<StrBrush>();
			Stack<StrFont> fonts = new Stack<StrFont>();

			brushes.Push(new StrBrush(status.Brush));
			fonts.Push(new StrFont(status.Font));

			_elements.Add(new Element(status));
			List<Part> parts = Html.Parse.ParseAll(lineOfText);

			foreach (Part part in parts)
			{
				switch (part)
				{
					case Text text:
						ProcessText(text, _elements);
						break;
					case Tag tag:
						ProcessTag(tag, _elements, status, brushes, fonts);
						break;
				}
			}

			/*
			global::System.IO.StreamWriter sw = new global::System.IO.StreamWriter(@"\temp\parse.txt", true);
			sw.WriteLine("-------------------");
			sw.WriteLine("   PARSE   ");
			sw.WriteLine("-------------------");
			sw.WriteLine(lineOfText);
			sw.WriteLine("-------------------");
			foreach (Part part in parts)
			sw.WriteLine(part.ToString());
			sw.WriteLine("-------------------");
			foreach(Element elem in _elements)
			sw.WriteLine(elem.ToString());
			sw.Close();
			*/

			return new Status(status);

			static void ProcessText(Text text, ICollection<Element> elements)
			{
				string[] words = text.Value.Trim().Split(' ');

				foreach (string word in words)
					elements.Add(new Element(new Text(word)));
			}

			static void ProcessTag(Tag tag, ICollection<Element> elements, Status status, Stack<StrBrush> brushes, Stack<StrFont> fonts)
			{
				Status oldStatus = new Status(status);

				if (tag.Name.IsSame("label"))
				{
					Attribute attr = tag.AttrList.Find("ID");
					string id = attr != null ? attr.Value : string.Empty;
					attr = tag.AttrList.Find("value");
					string value = attr != null ? attr.Value : string.Empty;
					attr = tag.AttrList.Find("width");
					float width = attr?.Get(-1) ?? -1;
					attr = tag.AttrList.Find("height");
					float height = attr?.Get(-1) ?? -1;
					elements.Add(new Element(new Label(id, value, width, height)));
				}

				if (tag.Name.IsSame("br")) status.NewLine = true;
				if (tag.Name.IsSame("pre")) status.WordWrap = tag.End;

				if (tag.End)
				{
					if (tag.Name.IsSame("b")) status.Font.Style &= ~FontStyle.Bold;
					if (tag.Name.IsSame("i")) status.Font.Style &= ~FontStyle.Italic;
					if (tag.Name.IsSame("u")) status.Font.Style &= ~FontStyle.Underline;

					if (tag.Name.IsSame("p"))
					{
						status.NewLine = true;
						status.Alignment = ContentAlignment.TopLeft;
					}

					if (tag.Name.IsSame("font"))
					{
						FontStyle oldFs = status.Font.Style;
						status.Brush = new StrBrush(brushes.Count > 1 ? brushes.Pop() : brushes.Peek());
						status.Font = new StrFont(fonts.Count > 1 ? fonts.Pop() : fonts.Peek()) { Style = oldFs };
					}
				}
				else
				{
					if (tag.Name.IsSame("b")) status.Font.Style |= FontStyle.Bold;
					if (tag.Name.IsSame("i")) status.Font.Style |= FontStyle.Italic;
					if (tag.Name.IsSame("u")) status.Font.Style |= FontStyle.Underline;

					if (tag.Name.IsSame("p"))
					{
						status.NewLine = true;
						status.Alignment = ContentAlignment.TopLeft;
						Attribute attr = tag.AttrList.Find("align");

						if (attr != null)
						{
							if (attr.Value.IsSame("center")) status.Alignment = ContentAlignment.TopCenter;
							if (attr.Value.IsSame("right")) status.Alignment = ContentAlignment.TopRight;
						}
					}

					if (tag.Name.IsSame("font"))
					{
						brushes.Push(new StrBrush(status.Brush));
						fonts.Push(new StrFont(status.Font));

						Attribute attr = tag.AttrList.Find("color");

						if (attr != null)
						{
							if (attr.Value.Length == 7 && attr.Value[0] == '#') status.Brush.Color = HtmlColors.GetColor(attr.Value);
							else status.Brush.Color = HtmlColors.GetColorByName(attr.Value);
						}

						attr = tag.AttrList.Find("size");
						if (attr != null) status.Font.Size = Convert.ToInt16(attr.Value);

						attr = tag.AttrList.Find("name");
						if (attr != null) status.Font.Name = attr.Value;
					}
				}

				if (oldStatus == status) return;
				elements.Add(new Element(status));
				status.NewLine = false;
			}
		}
	}
}