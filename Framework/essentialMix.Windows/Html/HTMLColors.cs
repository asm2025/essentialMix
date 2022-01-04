using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Globalization;
using JetBrains.Annotations;

namespace essentialMix.Windows.Html;

/// <summary>
/// Class for changing HTML color tags (names or #xxyyzz) into c# Color class.
/// </summary>
internal static class HtmlColors
{
	private static readonly ConcurrentDictionary<string, string> __colors = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public static Color GetColor([NotNull] string hexValue)
	{
		if (hexValue.StartsWith("#") && hexValue.Length == 7)
		{
			int rgb = int.Parse("FF" + hexValue.Substring(1, 6), NumberStyles.HexNumber);
			return Color.FromArgb(rgb);
		}

		return Color.Black;
	}

	public static Color GetColorByName(string colorName)
	{
		colorName = colorName.Replace(" ", string.Empty).ToLower();
		if (__colors.Count == 0) SetColors();
		return __colors.TryGetValue(colorName, out string value)
					? GetColor(value)
					: Color.Black;
	}

	private static void SetColors()
	{
		__colors.Clear();

		__colors.TryAdd("aliceblue", "#F0F8FF");
		__colors.TryAdd("antiquewhite", "#FAEBD7");
		__colors.TryAdd("aqua", "#00FFFF");
		__colors.TryAdd("aquamarine", "#7FFFD4");
		__colors.TryAdd("azure", "#F0FFFF");
		__colors.TryAdd("beige", "#F5F5DC");
		__colors.TryAdd("bisque", "#FFE4C4");
		__colors.TryAdd("black", "#000000");
		__colors.TryAdd("blanchedalmond", "#FFEBCD");
		__colors.TryAdd("blue", "#0000FF");
		__colors.TryAdd("blueviolet", "#8A2BE2");
		__colors.TryAdd("brown", "#A52A2A");
		__colors.TryAdd("burlywood", "#DEB887");
		__colors.TryAdd("cadetblue", "#5F9EA0");
		__colors.TryAdd("chartreuse", "#7FFF00");
		__colors.TryAdd("chocolate", "#D2691E");
		__colors.TryAdd("coral", "#FF7F50");
		__colors.TryAdd("cornflowerblue", "#6495ED");
		__colors.TryAdd("cornsilk", "#FFF8DC");
		__colors.TryAdd("crimson", "#DC143C");
		__colors.TryAdd("cyan", "#00FFFF");
		__colors.TryAdd("darkblue", "#00008B");
		__colors.TryAdd("darkcyan", "#008B8B");
		__colors.TryAdd("darkgoldenrod", "#B8860B");
		__colors.TryAdd("darkgray", "#A9A9A9");
		__colors.TryAdd("darkgreen", "#006400");
		__colors.TryAdd("darkkhaki", "#BDB76B");
		__colors.TryAdd("darkmagenta", "#8B008B");
		__colors.TryAdd("darkolivegreen", "#556B2F");
		__colors.TryAdd("darkorange", "#FF8C00");
		__colors.TryAdd("darkorchid", "#9932CC");
		__colors.TryAdd("darkred", "#8B0000");
		__colors.TryAdd("darksalmon", "#E9967A");
		__colors.TryAdd("darkseagreen", "#8FBC8F");
		__colors.TryAdd("darkslateblue", "#483D8B");
		__colors.TryAdd("darkslategray", "#2F4F4F");
		__colors.TryAdd("darkturquoise", "#00CED1");
		__colors.TryAdd("darkviolet", "#9400D3");
		__colors.TryAdd("deeppink", "#FF1493");
		__colors.TryAdd("deepskyblue", "#00BFFF");
		__colors.TryAdd("dimgray", "#696969");
		__colors.TryAdd("dodgerblue", "#1E90FF");
		__colors.TryAdd("firebrick", "#B22222");
		__colors.TryAdd("floralwhite", "#FFFAF0");
		__colors.TryAdd("forestgreen", "#228B22");
		__colors.TryAdd("fuchsia", "#FF00FF");
		__colors.TryAdd("gainsboro", "#DCDCDC");
		__colors.TryAdd("ghostwhite", "#F8F8FF");
		__colors.TryAdd("gold", "#FFD700");
		__colors.TryAdd("goldenrod", "#DAA520");
		__colors.TryAdd("gray", "#808080");
		__colors.TryAdd("green", "#008000");
		__colors.TryAdd("greenyellow", "#ADFF2F");
		__colors.TryAdd("honeydew", "#F0FFF0");
		__colors.TryAdd("hotpink", "#FF69B4");
		__colors.TryAdd("indianred", "#CD5C5C");
		__colors.TryAdd("indigo", "#4B0082");
		__colors.TryAdd("ivory", "#FFFFF0");
		__colors.TryAdd("khaki", "#F0E68C");
		__colors.TryAdd("lavender", "#E6E6FA");
		__colors.TryAdd("lavenderblush", "#FFF0F5");
		__colors.TryAdd("lawngreen", "#7CFC00");
		__colors.TryAdd("lemonchiffon", "#FFFACD");
		__colors.TryAdd("lightblue", "#ADD8E6");
		__colors.TryAdd("lightcoral", "#F08080");
		__colors.TryAdd("lightcyan", "#E0FFFF");
		__colors.TryAdd("lightgoldenrodyellow", "#FAFAD2");
		__colors.TryAdd("lightgrey", "#D3D3D3");
		__colors.TryAdd("lightgreen", "#90EE90");
		__colors.TryAdd("lightpink", "#FFB6C1");
		__colors.TryAdd("lightsalmon", "#FFA07A");
		__colors.TryAdd("lightseagreen", "#20B2AA");
		__colors.TryAdd("lightskyblue", "#87CEFA");
		__colors.TryAdd("lightslategray", "#778899");
		__colors.TryAdd("lightsteelblue", "#B0C4DE");
		__colors.TryAdd("lightyellow", "#FFFFE0");
		__colors.TryAdd("lime", "#00FF00");
		__colors.TryAdd("limegreen", "#32CD32");
		__colors.TryAdd("linen", "#FAF0E6");
		__colors.TryAdd("magenta", "#FF00FF");
		__colors.TryAdd("maroon", "#800000");
		__colors.TryAdd("mediumaquamarine", "#66CDAA");
		__colors.TryAdd("mediumblue", "#0000CD");
		__colors.TryAdd("mediumorchid", "#BA55D3");
		__colors.TryAdd("mediumpurple", "#9370D8");
		__colors.TryAdd("mediumseagreen", "#3CB371");
		__colors.TryAdd("mediumslateblue", "#7B68EE");
		__colors.TryAdd("mediumspringgreen", "#00FA9A");
		__colors.TryAdd("mediumturquoise", "#48D1CC");
		__colors.TryAdd("mediumvioletred", "#C71585");
		__colors.TryAdd("midnightblue", "#191970");
		__colors.TryAdd("mintcream", "#F5FFFA");
		__colors.TryAdd("mistyrose", "#FFE4E1");
		__colors.TryAdd("moccasin", "#FFE4B5");
		__colors.TryAdd("navajowhite", "#FFDEAD");
		__colors.TryAdd("navy", "#000080");
		__colors.TryAdd("oldlace", "#FDF5E6");
		__colors.TryAdd("olive", "#808000");
		__colors.TryAdd("olivedrab", "#6B8E23");
		__colors.TryAdd("orange", "#FFA500");
		__colors.TryAdd("orangered", "#FF4500");
		__colors.TryAdd("orchid", "#DA70D6");
		__colors.TryAdd("palegoldenrod", "#EEE8AA");
		__colors.TryAdd("palegreen", "#98FB98");
		__colors.TryAdd("paleturquoise", "#AFEEEE");
		__colors.TryAdd("palevioletred", "#D87093");
		__colors.TryAdd("papayawhip", "#FFEFD5");
		__colors.TryAdd("peachpuff", "#FFDAB9");
		__colors.TryAdd("peru", "#CD853F");
		__colors.TryAdd("pink", "#FFC0CB");
		__colors.TryAdd("plum", "#DDA0DD");
		__colors.TryAdd("powderblue", "#B0E0E6");
		__colors.TryAdd("purple", "#800080");
		__colors.TryAdd("red", "#FF0000");
		__colors.TryAdd("rosybrown", "#BC8F8F");
		__colors.TryAdd("royalblue", "#4169E1");
		__colors.TryAdd("saddlebrown", "#8B4513");
		__colors.TryAdd("salmon", "#FA8072");
		__colors.TryAdd("sandybrown", "#F4A460");
		__colors.TryAdd("seagreen", "#2E8B57");
		__colors.TryAdd("seashell", "#FFF5EE");
		__colors.TryAdd("sienna", "#A0522D");
		__colors.TryAdd("silver", "#C0C0C0");
		__colors.TryAdd("skyblue", "#87CEEB");
		__colors.TryAdd("slateblue", "#6A5ACD");
		__colors.TryAdd("slategray", "#708090");
		__colors.TryAdd("snow", "#FFFAFA");
		__colors.TryAdd("springgreen", "#00FF7F");
		__colors.TryAdd("steelblue", "#4682B4");
		__colors.TryAdd("tan", "#D2B48C");
		__colors.TryAdd("teal", "#008080");
		__colors.TryAdd("thistle", "#D8BFD8");
		__colors.TryAdd("tomato", "#FF6347");
		__colors.TryAdd("turquoise", "#40E0D0");
		__colors.TryAdd("violet", "#EE82EE");
		__colors.TryAdd("wheat", "#F5DEB3");
		__colors.TryAdd("white", "#FFFFFF");
		__colors.TryAdd("whitesmoke", "#F5F5F5");
		__colors.TryAdd("yellow", "#FFFF00");
		__colors.TryAdd("yellowgreen", "#9ACD32");
	}
}