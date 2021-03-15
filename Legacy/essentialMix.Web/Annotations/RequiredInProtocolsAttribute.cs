using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Web;
using essentialMix.Helpers;

namespace essentialMix.Web.Annotations
{
	/// <inheritdoc />
	/// <summary>
	/// https://dataannotationsextensions.apphb.com/ RegEx is expensive and slow hens this class and more protocols are required
	/// RequiredHttpAttribute class. Identical behavior to the RequiredAttribute class
	/// with the addition that it allows for bypassing validation based on the Http
	/// method of the current Http request.
	/// </summary>
	/// <remarks>
	/// This class will have identical behavior to the RequiredAttribute class in the scenario
	/// where the System.Web.HttpContext.Current property returns null.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class RequiredInProtocolsAttribute : RequiredAttribute
    {
        /// <summary>
        /// Gets or sets the HTTP methods that this attribute will validate on.
        /// </summary>
        /// <remarks>The value is treated as a regular expression pattern, e.g., GET|POST.</remarks>
        public HttpMethod[] HttpMethods { get; set; }

        /// <inheritdoc />
        public override bool IsValid(object value) { return !RequiredInContext(HttpContext.Current) || base.IsValid(value); }

		/// <inheritdoc />
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			return RequiredInContext(HttpContext.Current)
						? base.IsValid(value, validationContext)
						: ValidationResult.Success;
		}

		/// <summary>
		/// Determines if the provided HTTP context permits validation.
		/// </summary>
		/// <param name="context">The HTTP context.</param>
		/// <returns>True if validation is required and false otherwise.</returns>
		protected bool RequiredInContext(HttpContext context)
        {
			if (context == null || HttpMethods == null || HttpMethods.Length <= 0) return true;

			HttpMethod method = UriHelper.HttpMethodFromString(context.Request.HttpMethod);
			return HttpMethods.Contains(method);

		}
    }
}