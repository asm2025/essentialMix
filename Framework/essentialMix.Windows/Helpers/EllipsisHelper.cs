using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using essentialMix.Extensions;

namespace essentialMix.Windows.Helpers
{
	/// <summary>
	/// Specifies ellipsis format and alignment.
	/// </summary>
	[Flags]
	public enum EllipsisFormat
	{
		/// <summary>
		/// Text is not modified.
		/// </summary>
		None = 0,
		/// <summary>
		/// Text is trimmed at the end of the string. An ellipsis (...) is drawn in place of remaining text.
		/// </summary>
		End = 1,
		/// <summary>
		/// Text is trimmed at the beginning of the string. An ellipsis (...) is drawn in place of remaining text. 
		/// </summary>
		Start = 2,
		/// <summary>
		/// Text is trimmed in the middle of the string. An ellipsis (...) is drawn in place of remaining text.
		/// </summary>
		Middle = 4,
		/// <summary>
		/// Preserve as much as possible of the drive and filename information. Must be combined with alignment information.
		/// </summary>
		Path = 8,
		/// <summary>
		/// Text is trimmed at a word boundary. Must be combined with alignment information.
		/// </summary>
		Word = 16
	}


	public static class EllipsisHelper
	{
		/// <summary>
		/// String used as a place holder for trimmed text.
		/// </summary>
		private const string ELLIPSIS_CHARS = "...";

		private static readonly Regex __prevWord = new Regex(@"\W*\w*$", RegexOptions.Compiled | RegexOptions.Multiline);
		private static readonly Regex __nextWord = new Regex(@"\w*\W*", RegexOptions.Compiled | RegexOptions.Multiline);

		/// <summary>
		/// Truncates a text string to fit within a given control width by replacing trimmed text with ellipses. 
		/// </summary>
		/// <param name="text">String to be trimmed.</param>
		/// <param name="control">text must fit within control width.
		///	The control's Font is used to measure the text string.</param>
		/// <param name="options">Format and alignment of ellipsis.</param>
		/// <returns>This function returns text trimmed to the specified width.</returns>
		public static string Compact(string text, Control control, EllipsisFormat options)
		{
			if (string.IsNullOrEmpty(text)) return text;

			// no alignment information
			if (!options.FastHasFlag(EllipsisFormat.Start) && !options.FastHasFlag(EllipsisFormat.Middle) && !options.FastHasFlag(EllipsisFormat.End)) return text;

			if (control == null) throw new ArgumentNullException(nameof(control));

			using (Graphics dc = control.CreateGraphics())
			{
				Size s = TextRenderer.MeasureText(dc, text, control.Font);

				// control is large enough to display the whole text
				if (s.Width <= control.Width) return text;

				string pre = string.Empty;
				string mid = text;
				string post = string.Empty;

				bool isWord = options.FastHasFlag(EllipsisFormat.Word);
				bool isPath = options.FastHasFlag(EllipsisFormat.Path);
				bool isStart = options.FastHasFlag(EllipsisFormat.Start);
				bool isMiddle = options.FastHasFlag(EllipsisFormat.Middle);
				bool isEnd = options.FastHasFlag(EllipsisFormat.End);

				// split path string into <drive><directory><filename>
				if (isPath)
				{
					pre = Path.GetPathRoot(text);
					mid = Path.GetDirectoryName(text)?.Substring(pre.Length) ?? string.Empty;
					post = Path.GetFileName(text);
				}

				int len = 0;
				int seg = mid.Length;
				string fit = string.Empty;

				// find the longest string that fits into 
				// the control boundaries using bisection method
				while (seg > 1)
				{
					seg -= seg / 2;

					int left = len + seg;
					int right = mid.Length;

					if (left > right) continue;

					if (isMiddle)
					{
						right -= left / 2;
						left -= left / 2;
					}
					else if (isStart)
					{
						right -= left;
						left = 0;
					}

					// trim at a word boundary using regular expressions
					if (isWord)
					{
						if (isEnd) left -= __prevWord.Match(mid, 0, left).Length;
						if (isStart) right += __nextWord.Match(mid, right).Length;
					}

					// build and measure a candidate string with ellipsis
					string tst = mid.Substring(0, left) + ELLIPSIS_CHARS + mid.Substring(right);
					
					// restore path with <drive> and <filename>
					if (isPath) tst = Path.Combine(Path.Combine(pre, tst), post);
					s = TextRenderer.MeasureText(dc, tst, control.Font);

					if (s.Width > control.Width) continue;

					// candidate string fits into control boundaries, try a longer string
					// stop when seg <= 1
					len += seg;
					fit = tst;
				}

				if (len != 0) return fit;

				// string can't fit into control

				// "path" mode is off, just return ellipsis characters
				if (!isPath) return ELLIPSIS_CHARS;

				// <drive> and <directory> are empty, return <filename>
				if (pre.Length == 0 && mid.Length == 0) return post;

				// measure "C:\...\filename.ext"
				fit = Path.Combine(Path.Combine(pre, ELLIPSIS_CHARS), post);
				s = TextRenderer.MeasureText(dc, fit, control.Font);

				// if still not fit then return "...\filename.ext"
				if (s.Width > control.Width) fit = Path.Combine(ELLIPSIS_CHARS, post);
				return fit;
			}
		}
	}
}
