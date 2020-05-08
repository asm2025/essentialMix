using System.Collections.Generic;
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
		private static readonly Dictionary<string, string> COLORS = new Dictionary<string, string>();

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
			return COLORS.ContainsKey(colorName) ? GetColor(COLORS[colorName]) : Color.Black;
		}

		private static void SetColors()
		{
			COLORS.Clear();

			COLORS.Add("aliceblue", "#F0F8FF");
			COLORS.Add("antiquewhite", "#FAEBD7");
			COLORS.Add("aqua", "#00FFFF");
			COLORS.Add("aquamarine", "#7FFFD4");
			COLORS.Add("azure", "#F0FFFF");
			COLORS.Add("beige", "#F5F5DC");
			COLORS.Add("bisque", "#FFE4C4");
			COLORS.Add("black", "#000000");
			COLORS.Add("blanchedalmond", "#FFEBCD");
			COLORS.Add("blue", "#0000FF");
			COLORS.Add("blueviolet", "#8A2BE2");
			COLORS.Add("brown", "#A52A2A");
			COLORS.Add("burlywood", "#DEB887");
			COLORS.Add("cadetblue", "#5F9EA0");
			COLORS.Add("chartreuse", "#7FFF00");
			COLORS.Add("chocolate", "#D2691E");
			COLORS.Add("coral", "#FF7F50");
			COLORS.Add("cornflowerblue", "#6495ED");
			COLORS.Add("cornsilk", "#FFF8DC");
			COLORS.Add("crimson", "#DC143C");
			COLORS.Add("cyan", "#00FFFF");
			COLORS.Add("darkblue", "#00008B");
			COLORS.Add("darkcyan", "#008B8B");
			COLORS.Add("darkgoldenrod", "#B8860B");
			COLORS.Add("darkgray", "#A9A9A9");
			COLORS.Add("darkgreen", "#006400");
			COLORS.Add("darkkhaki", "#BDB76B");
			COLORS.Add("darkmagenta", "#8B008B");
			COLORS.Add("darkolivegreen", "#556B2F");
			COLORS.Add("darkorange", "#FF8C00");
			COLORS.Add("darkorchid", "#9932CC");
			COLORS.Add("darkred", "#8B0000");
			COLORS.Add("darksalmon", "#E9967A");
			COLORS.Add("darkseagreen", "#8FBC8F");
			COLORS.Add("darkslateblue", "#483D8B");
			COLORS.Add("darkslategray", "#2F4F4F");
			COLORS.Add("darkturquoise", "#00CED1");
			COLORS.Add("darkviolet", "#9400D3");
			COLORS.Add("deeppink", "#FF1493");
			COLORS.Add("deepskyblue", "#00BFFF");
			COLORS.Add("dimgray", "#696969");
			COLORS.Add("dodgerblue", "#1E90FF");
			COLORS.Add("firebrick", "#B22222");
			COLORS.Add("floralwhite", "#FFFAF0");
			COLORS.Add("forestgreen", "#228B22");
			COLORS.Add("fuchsia", "#FF00FF");
			COLORS.Add("gainsboro", "#DCDCDC");
			COLORS.Add("ghostwhite", "#F8F8FF");
			COLORS.Add("gold", "#FFD700");
			COLORS.Add("goldenrod", "#DAA520");
			COLORS.Add("gray", "#808080");
			COLORS.Add("green", "#008000");
			COLORS.Add("greenyellow", "#ADFF2F");
			COLORS.Add("honeydew", "#F0FFF0");
			COLORS.Add("hotpink", "#FF69B4");
			COLORS.Add("indianred", "#CD5C5C");
			COLORS.Add("indigo", "#4B0082");
			COLORS.Add("ivory", "#FFFFF0");
			COLORS.Add("khaki", "#F0E68C");
			COLORS.Add("lavender", "#E6E6FA");
			COLORS.Add("lavenderblush", "#FFF0F5");
			COLORS.Add("lawngreen", "#7CFC00");
			COLORS.Add("lemonchiffon", "#FFFACD");
			COLORS.Add("lightblue", "#ADD8E6");
			COLORS.Add("lightcoral", "#F08080");
			COLORS.Add("lightcyan", "#E0FFFF");
			COLORS.Add("lightgoldenrodyellow", "#FAFAD2");
			COLORS.Add("lightgrey", "#D3D3D3");
			COLORS.Add("lightgreen", "#90EE90");
			COLORS.Add("lightpink", "#FFB6C1");
			COLORS.Add("lightsalmon", "#FFA07A");
			COLORS.Add("lightseagreen", "#20B2AA");
			COLORS.Add("lightskyblue", "#87CEFA");
			COLORS.Add("lightslategray", "#778899");
			COLORS.Add("lightsteelblue", "#B0C4DE");
			COLORS.Add("lightyellow", "#FFFFE0");
			COLORS.Add("lime", "#00FF00");
			COLORS.Add("limegreen", "#32CD32");
			COLORS.Add("linen", "#FAF0E6");
			COLORS.Add("magenta", "#FF00FF");
			COLORS.Add("maroon", "#800000");
			COLORS.Add("mediumaquamarine", "#66CDAA");
			COLORS.Add("mediumblue", "#0000CD");
			COLORS.Add("mediumorchid", "#BA55D3");
			COLORS.Add("mediumpurple", "#9370D8");
			COLORS.Add("mediumseagreen", "#3CB371");
			COLORS.Add("mediumslateblue", "#7B68EE");
			COLORS.Add("mediumspringgreen", "#00FA9A");
			COLORS.Add("mediumturquoise", "#48D1CC");
			COLORS.Add("mediumvioletred", "#C71585");
			COLORS.Add("midnightblue", "#191970");
			COLORS.Add("mintcream", "#F5FFFA");
			COLORS.Add("mistyrose", "#FFE4E1");
			COLORS.Add("moccasin", "#FFE4B5");
			COLORS.Add("navajowhite", "#FFDEAD");
			COLORS.Add("navy", "#000080");
			COLORS.Add("oldlace", "#FDF5E6");
			COLORS.Add("olive", "#808000");
			COLORS.Add("olivedrab", "#6B8E23");
			COLORS.Add("orange", "#FFA500");
			COLORS.Add("orangered", "#FF4500");
			COLORS.Add("orchid", "#DA70D6");
			COLORS.Add("palegoldenrod", "#EEE8AA");
			COLORS.Add("palegreen", "#98FB98");
			COLORS.Add("paleturquoise", "#AFEEEE");
			COLORS.Add("palevioletred", "#D87093");
			COLORS.Add("papayawhip", "#FFEFD5");
			COLORS.Add("peachpuff", "#FFDAB9");
			COLORS.Add("peru", "#CD853F");
			COLORS.Add("pink", "#FFC0CB");
			COLORS.Add("plum", "#DDA0DD");
			COLORS.Add("powderblue", "#B0E0E6");
			COLORS.Add("purple", "#800080");
			COLORS.Add("red", "#FF0000");
			COLORS.Add("rosybrown", "#BC8F8F");
			COLORS.Add("royalblue", "#4169E1");
			COLORS.Add("saddlebrown", "#8B4513");
			COLORS.Add("salmon", "#FA8072");
			COLORS.Add("sandybrown", "#F4A460");
			COLORS.Add("seagreen", "#2E8B57");
			COLORS.Add("seashell", "#FFF5EE");
			COLORS.Add("sienna", "#A0522D");
			COLORS.Add("silver", "#C0C0C0");
			COLORS.Add("skyblue", "#87CEEB");
			COLORS.Add("slateblue", "#6A5ACD");
			COLORS.Add("slategray", "#708090");
			COLORS.Add("snow", "#FFFAFA");
			COLORS.Add("springgreen", "#00FF7F");
			COLORS.Add("steelblue", "#4682B4");
			COLORS.Add("tan", "#D2B48C");
			COLORS.Add("teal", "#008080");
			COLORS.Add("thistle", "#D8BFD8");
			COLORS.Add("tomato", "#FF6347");
			COLORS.Add("turquoise", "#40E0D0");
			COLORS.Add("violet", "#EE82EE");
			COLORS.Add("wheat", "#F5DEB3");
			COLORS.Add("white", "#FFFFFF");
			COLORS.Add("whitesmoke", "#F5F5F5");
			COLORS.Add("yellow", "#FFFF00");
			COLORS.Add("yellowgreen", "#9ACD32");
		}
	}
}