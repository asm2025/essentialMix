using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.ComponentModel.DataAnnotations
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
	public sealed class UserNameAttribute : DataTypeAttribute
	{
		private static readonly Regex REGEX = CreateRegEx();

		public UserNameAttribute() 
			: base(nameof(UserNameAttribute))
		{
			ErrorMessage = "The {0} field is not a valid user name.";
		}

		/// <summary>Determines whether the specified value matches the pattern of a valid user name.</summary>
		/// <param name="value">The value to validate.</param>
		/// <returns>true if the specified value is valid or null; otherwise, false.</returns>
		public override bool IsValid(object value) { return value == null || value is string input && REGEX.IsMatch(input); }

		[NotNull]
		private static Regex CreateRegEx()
		{
			const string RGX_USER = @"^((([a-z\d_\-\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z\d_\-]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f\x21\x23-\x5b\x5d-\x7e\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))(?:@((([a-z\d\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z\d\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z\d\-\._~\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z\d\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z\d\-\._~\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)?$";

			try
			{
				if (AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") == null)
					return new Regex(RGX_USER, RegexHelper.OPTIONS_I | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(2.0));
			}
			catch
			{
			}

			return new Regex(RGX_USER, RegexHelper.OPTIONS_I | RegexOptions.ExplicitCapture);
		}
	}
}
