using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.ComponentModel.DataAnnotations
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
	public sealed class PersonNameAttribute : DataTypeAttribute
	{
		private static readonly Regex __regex = CreateRegEx();

		public PersonNameAttribute() 
			: base(nameof(UserNameAttribute))
		{
			ErrorMessage = "The {0} field is not a valid person name.";
		}

		/// <summary>Determines whether the specified value matches the pattern of a valid person name.</summary>
		/// <param name="value">The value to validate.</param>
		/// <returns>true if the specified value is valid or null; otherwise, false.</returns>
		public override bool IsValid(object value) { return value == null || value is string input && __regex.IsMatch(input); }

		[NotNull]
		private static Regex CreateRegEx()
		{
			const string REGEX_PERSON = @"^(([a-z\-\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+((\s([a-z\-\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+\.?)*)('([a-z\-\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)$";

			try
			{
				if (AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") == null)
					return new Regex(REGEX_PERSON, RegexHelper.OPTIONS_I | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(2.0));
			}
			catch
			{
				// ignored
			}

			return new Regex(REGEX_PERSON, RegexHelper.OPTIONS_I | RegexOptions.ExplicitCapture);
		}
	}
}
