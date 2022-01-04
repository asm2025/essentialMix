using System.Text.RegularExpressions;

namespace essentialMix.Helpers;

public static class WebResponseHelper
{
	public static Regex TitleCheckExpression { get; } = new Regex(@"<title>\s*(.+?)\s*</title>", RegexHelper.OPTIONS_I | RegexOptions.Multiline);
}