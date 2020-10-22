using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Globalization;
using JetBrains.Annotations;

namespace asm.Windows.Html
{
	/// <summary>
	/// Class for changing HTML color tags (names or #xxyyzz) into c# Color class.
	/// </summary>
	internal static class HtmlColors
	{
		private static readonly ConcurrentDictionary<string, string> COLORS = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
			if (COLORS.Count == 0) SetColors();
			return COLORS.TryGetValue(colorName, out string value)
						? GetColor(value)
						: Color.Black;
		}

		private static void SetColors()
		{
			COLORS.Clear();

			COLORS.TryAdd("aliceblue", "#F0F8FF");
			COLORS.TryAdd("antiquewhite", "#FAEBD7");
			COLORS.TryAdd("aqua", "#00FFFF");
			COLORS.TryAdd("aquamarine", "#7FFFD4");
			COLORS.TryAdd("azure", "#F0FFFF");
			COLORS.TryAdd("beige", "#F5F5DC");
			COLORS.TryAdd("bisque", "#FFE4C4");
			COLORS.TryAdd("black", "#000000");
			COLORS.TryAdd("blanchedalmond", "#FFEBCD");
			COLORS.TryAdd("blue", "#0000FF");
			COLORS.TryAdd("blueviolet", "#8A2BE2");
			COLORS.TryAdd("brown", "#A52A2A");
			COLORS.TryAdd("burlywood", "#DEB887");
			COLORS.TryAdd("cadetblue", "#5F9EA0");
			COLORS.TryAdd("chartreuse", "#7FFF00");
			COLORS.TryAdd("chocolate", "#D2691E");
			COLORS.TryAdd("coral", "#FF7F50");
			COLORS.TryAdd("cornflowerblue", "#6495ED");
			COLORS.TryAdd("cornsilk", "#FFF8DC");
			COLORS.TryAdd("crimson", "#DC143C");
			COLORS.TryAdd("cyan", "#00FFFF");
			COLORS.TryAdd("darkblue", "#00008B");
			COLORS.TryAdd("darkcyan", "#008B8B");
			COLORS.TryAdd("darkgoldenrod", "#B8860B");
			COLORS.TryAdd("darkgray", "#A9A9A9");
			COLORS.TryAdd("darkgreen", "#006400");
			COLORS.TryAdd("darkkhaki", "#BDB76B");
			COLORS.TryAdd("darkmagenta", "#8B008B");
			COLORS.TryAdd("darkolivegreen", "#556B2F");
			COLORS.TryAdd("darkorange", "#FF8C00");
			COLORS.TryAdd("darkorchid", "#9932CC");
			COLORS.TryAdd("darkred", "#8B0000");
			COLORS.TryAdd("darksalmon", "#E9967A");
			COLORS.TryAdd("darkseagreen", "#8FBC8F");
			COLORS.TryAdd("darkslateblue", "#483D8B");
			COLORS.TryAdd("darkslategray", "#2F4F4F");
			COLORS.TryAdd("darkturquoise", "#00CED1");
			COLORS.TryAdd("darkviolet", "#9400D3");
			COLORS.TryAdd("deeppink", "#FF1493");
			COLORS.TryAdd("deepskyblue", "#00BFFF");
			COLORS.TryAdd("dimgray", "#696969");
			COLORS.TryAdd("dodgerblue", "#1E90FF");
			COLORS.TryAdd("firebrick", "#B22222");
			COLORS.TryAdd("floralwhite", "#FFFAF0");
			COLORS.TryAdd("forestgreen", "#228B22");
			COLORS.TryAdd("fuchsia", "#FF00FF");
			COLORS.TryAdd("gainsboro", "#DCDCDC");
			COLORS.TryAdd("ghostwhite", "#F8F8FF");
			COLORS.TryAdd("gold", "#FFD700");
			COLORS.TryAdd("goldenrod", "#DAA520");
			COLORS.TryAdd("gray", "#808080");
			COLORS.TryAdd("green", "#008000");
			COLORS.TryAdd("greenyellow", "#ADFF2F");
			COLORS.TryAdd("honeydew", "#F0FFF0");
			COLORS.TryAdd("hotpink", "#FF69B4");
			COLORS.TryAdd("indianred", "#CD5C5C");
			COLORS.TryAdd("indigo", "#4B0082");
			COLORS.TryAdd("ivory", "#FFFFF0");
			COLORS.TryAdd("khaki", "#F0E68C");
			COLORS.TryAdd("lavender", "#E6E6FA");
			COLORS.TryAdd("lavenderblush", "#FFF0F5");
			COLORS.TryAdd("lawngreen", "#7CFC00");
			COLORS.TryAdd("lemonchiffon", "#FFFACD");
			COLORS.TryAdd("lightblue", "#ADD8E6");
			COLORS.TryAdd("lightcoral", "#F08080");
			COLORS.TryAdd("lightcyan", "#E0FFFF");
			COLORS.TryAdd("lightgoldenrodyellow", "#FAFAD2");
			COLORS.TryAdd("lightgrey", "#D3D3D3");
			COLORS.TryAdd("lightgreen", "#90EE90");
			COLORS.TryAdd("lightpink", "#FFB6C1");
			COLORS.TryAdd("lightsalmon", "#FFA07A");
			COLORS.TryAdd("lightseagreen", "#20B2AA");
			COLORS.TryAdd("lightskyblue", "#87CEFA");
			COLORS.TryAdd("lightslategray", "#778899");
			COLORS.TryAdd("lightsteelblue", "#B0C4DE");
			COLORS.TryAdd("lightyellow", "#FFFFE0");
			COLORS.TryAdd("lime", "#00FF00");
			COLORS.TryAdd("limegreen", "#32CD32");
			COLORS.TryAdd("linen", "#FAF0E6");
			COLORS.TryAdd("magenta", "#FF00FF");
			COLORS.TryAdd("maroon", "#800000");
			COLORS.TryAdd("mediumaquamarine", "#66CDAA");
			COLORS.TryAdd("mediumblue", "#0000CD");
			COLORS.TryAdd("mediumorchid", "#BA55D3");
			COLORS.TryAdd("mediumpurple", "#9370D8");
			COLORS.TryAdd("mediumseagreen", "#3CB371");
			COLORS.TryAdd("mediumslateblue", "#7B68EE");
			COLORS.TryAdd("mediumspringgreen", "#00FA9A");
			COLORS.TryAdd("mediumturquoise", "#48D1CC");
			COLORS.TryAdd("mediumvioletred", "#C71585");
			COLORS.TryAdd("midnightblue", "#191970");
			COLORS.TryAdd("mintcream", "#F5FFFA");
			COLORS.TryAdd("mistyrose", "#FFE4E1");
			COLORS.TryAdd("moccasin", "#FFE4B5");
			COLORS.TryAdd("navajowhite", "#FFDEAD");
			COLORS.TryAdd("navy", "#000080");
			COLORS.TryAdd("oldlace", "#FDF5E6");
			COLORS.TryAdd("olive", "#808000");
			COLORS.TryAdd("olivedrab", "#6B8E23");
			COLORS.TryAdd("orange", "#FFA500");
			COLORS.TryAdd("orangered", "#FF4500");
			COLORS.TryAdd("orchid", "#DA70D6");
			COLORS.TryAdd("palegoldenrod", "#EEE8AA");
			COLORS.TryAdd("palegreen", "#98FB98");
			COLORS.TryAdd("paleturquoise", "#AFEEEE");
			COLORS.TryAdd("palevioletred", "#D87093");
			COLORS.TryAdd("papayawhip", "#FFEFD5");
			COLORS.TryAdd("peachpuff", "#FFDAB9");
			COLORS.TryAdd("peru", "#CD853F");
			COLORS.TryAdd("pink", "#FFC0CB");
			COLORS.TryAdd("plum", "#DDA0DD");
			COLORS.TryAdd("powderblue", "#B0E0E6");
			COLORS.TryAdd("purple", "#800080");
			COLORS.TryAdd("red", "#FF0000");
			COLORS.TryAdd("rosybrown", "#BC8F8F");
			COLORS.TryAdd("royalblue", "#4169E1");
			COLORS.TryAdd("saddlebrown", "#8B4513");
			COLORS.TryAdd("salmon", "#FA8072");
			COLORS.TryAdd("sandybrown", "#F4A460");
			COLORS.TryAdd("seagreen", "#2E8B57");
			COLORS.TryAdd("seashell", "#FFF5EE");
			COLORS.TryAdd("sienna", "#A0522D");
			COLORS.TryAdd("silver", "#C0C0C0");
			COLORS.TryAdd("skyblue", "#87CEEB");
			COLORS.TryAdd("slateblue", "#6A5ACD");
			COLORS.TryAdd("slategray", "#708090");
			COLORS.TryAdd("snow", "#FFFAFA");
			COLORS.TryAdd("springgreen", "#00FF7F");
			COLORS.TryAdd("steelblue", "#4682B4");
			COLORS.TryAdd("tan", "#D2B48C");
			COLORS.TryAdd("teal", "#008080");
			COLORS.TryAdd("thistle", "#D8BFD8");
			COLORS.TryAdd("tomato", "#FF6347");
			COLORS.TryAdd("turquoise", "#40E0D0");
			COLORS.TryAdd("violet", "#EE82EE");
			COLORS.TryAdd("wheat", "#F5DEB3");
			COLORS.TryAdd("white", "#FFFFFF");
			COLORS.TryAdd("whitesmoke", "#F5F5F5");
			COLORS.TryAdd("yellow", "#FFFF00");
			COLORS.TryAdd("yellowgreen", "#9ACD32");
		}
	}
}