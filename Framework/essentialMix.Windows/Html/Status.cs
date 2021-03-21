using System;
using System.Drawing;
using JetBrains.Annotations;

namespace essentialMix.Windows.Html
{
	/// <summary>
	/// Represent the status of text (alignment, font...) somewhere in the HTML text
	/// </summary>
	public class Status : IComparable
	{
		public Status()
		{
			Font = new StrFont();
			NewLine = false;
			WordWrap = true;
			Alignment = ContentAlignment.TopLeft;
			Brush = new StrBrush();
		}

		public Status([NotNull] Status oldStatus)
		{
			Font = new StrFont(oldStatus.Font);
			NewLine = oldStatus.NewLine;
			WordWrap = oldStatus.WordWrap;
			Alignment = oldStatus.Alignment;
			Brush = new StrBrush(oldStatus.Brush);
		}

		public override string ToString() { return $"Status: nl={NewLine};fnt={Font};ww={WordWrap};al={Alignment};br={Brush};"; }

		public StrBrush Brush { get; set; }

		public StrFont Font { get; set; }

		public bool NewLine { get; set; }

		public bool WordWrap { get; set; }

		public ContentAlignment Alignment { get; set; }

		public int CompareTo(object obj)
		{
			return obj is Status old && old.Alignment == Alignment && old.Brush == Brush && old.Font == Font && old.NewLine == NewLine && old.WordWrap == WordWrap
						? 0
						: -1;
		}
	}
}