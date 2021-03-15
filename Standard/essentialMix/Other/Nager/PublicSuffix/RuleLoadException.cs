using System;

// ReSharper disable once CheckNamespace
// ReSharper disable once CheckNamespace
namespace Other.Nager.PublicSuffix
{
    /// <summary>
    /// Rule Load Exception
    /// </summary>
    public class RuleLoadException : Exception
    {
        /// <summary>
        /// Error Message
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Rule Load Exception
        /// </summary>
        /// <param name="error"></param>
        public RuleLoadException(string error)
        {
            Error = error;
        }
    }
}
