using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Helpers
{
	public static class RegexHelper
	{
		public const RegexOptions OPTIONS = RegexOptions.CultureInvariant | RegexOptions.Compiled;
		public const RegexOptions OPTIONS_I = OPTIONS | RegexOptions.IgnoreCase;

		public const string RGX_WORD = "(\\s|^){0}(\\s|$)";
		public const string RGX_PARTITIONS_P = "[^{0}]+[{0}]?";

		private const string RGX_PATTERN_DBL_DOT = @"(([^\\]?)(\.{2,}))";
		private const string RGX_PATTERN_DBL_DOT_RPL = "${2}.";
		
		private static readonly string[][] __escapeStrings =
		{
			new[] {@"\?", "?"},
			new[] {@"\*", "*"},
			new[] {"?*", "*"},
			new[] {"*?", "*"},
			new[] {"**", "*"},
			new[] {"?", "."},
			new[] {"*", ".*"}
		};

		public static readonly Regex AllAsterisks = new Regex(@"^[*.\\]+$", OPTIONS);

		public static string EscapeFilePattern(string pattern)
		{
			pattern = pattern?.Trim();
			return string.IsNullOrEmpty(pattern)
						? "*"
						: pattern.Escape().Replace(__escapeStrings).Replace(RGX_PATTERN_DBL_DOT, RGX_PATTERN_DBL_DOT_RPL, OPTIONS);
		}

		[NotNull]
		public static string FromWildCards(string pattern)
		{
			pattern = EscapeFilePattern(pattern);
			return string.Concat('^', pattern, '$');
		}

		[NotNull]
		public static string FromFilePattern(string pattern)
		{
			pattern = pattern?.Replace("||", "|").Trim('|', ' ');
			if (string.IsNullOrEmpty(pattern) || !pattern.Contains("|")) return FromWildCards(pattern);
			
			string[] entries = pattern.Split(StringSplitOptions.RemoveEmptyEntries, '|');
			ISet<string> unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (string entry in entries)
			{
				string value = EscapeFilePattern(entry);
				if (AllAsterisks.IsMatch(value)) return string.Concat('^', value, '$');
				unique.Add(string.Concat("(?:", value, ")"));
			}

			return string.Concat('^', string.Join("|", unique), '$');
		}
	}
}