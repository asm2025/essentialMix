using System;
using System.Drawing;
using JetBrains.Annotations;

namespace essentialMix.Windows.Html
{
	/// <summary>
	/// Represent the value of C# Font object.
	/// </summary>
	/// <remarks>
	/// This is because if I create an object +of Font class then C# really creates a
	/// font (which is time expensive), but with the use of this class I can represent
	/// the font without really creating it.
	/// </remarks>
	public class StrFont : IComparable
	{
		public StrFont()
		{
			Name = "Tahoma";
			Size = 10;
			Style = FontStyle.Regular;
		}

		public StrFont([NotNull] StrFont oldFont)
		{
			Name = oldFont.Name;
			Size = oldFont.Size;
			Style = oldFont.Style;
		}

		public StrFont([NotNull] Font fnt)
		{
			Name = fnt.Name;
			Size = (int)fnt.Size;
			Style = fnt.Style;
		}

		public override string ToString() { return $"Font: {Name}/{Size} - {Style}"; }

		public FontStyle Style { get; set; }

		public int Size { get; set; }

		public string Name { get; set; }

		[NotNull]
		public Font GetRealFont() { return new Font(Name, Size, Style); }

		public int CompareTo(object obj)
		{
			return obj is StrFont old && old.Name == Name && old.Size == Size && old.Style == Style
						? 0
						: -1;
		}
	}
}