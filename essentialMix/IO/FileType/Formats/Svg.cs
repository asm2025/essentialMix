using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using essentialMix.Helpers;
using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Svg : FileFormat
{
	private const int BUFFER_SIZE = 512;
	private const int BUFFER_MIN = 100;

	private static readonly Regex __regex = new Regex(@"<!DOCTYPE\s+svg\s+PUBLIC\s+[""']-//W3C//DTD\s+SVG.+?/DTD/svg\d*\.dtd.+?<svg.+?(?:version=[""']\d+(?:\.\d+)?[""'])?", RegexHelper.OPTIONS_I | RegexOptions.Multiline);
	public Svg()
		: base("svg", MediaTypeNames.Image.SvgXml, null)
	{
	}

	/// <inheritdoc />
	public override bool IsMatch(Stream stream)
	{
		if (!stream.CanSeek) throw new NotSupportedException("Stream type is not supported.");
		if (stream.Length < BUFFER_MIN) return false;

		using (StreamReader reader = new StreamReader(stream, Encoding.Default, true, BUFFER_SIZE, true))
		{
			char[] buffer = new char[BUFFER_SIZE];
			if (reader.Read(buffer, 0, BUFFER_SIZE) < BUFFER_MIN) return false;
			string block = new string(buffer);
			return __regex.IsMatch(block);
		}
	}

	/// <inheritdoc />
	protected sealed override bool IsMatch(byte[] buffer) { throw new NotSupportedException(); }
}