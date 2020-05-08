using System;
using System.Runtime.Serialization;
using asm.Exceptions.Properties;
using JetBrains.Annotations;

namespace asm.Exceptions.IO
{
	/// <summary>
    /// Exception class for 7-zip library operations.
    /// </summary>
    [Serializable]
    public class LibraryException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the LibraryException class
        /// </summary>
        public LibraryException() : base(Resources.LibraryNotLoaded) { }

        /// <summary>
        /// Initializes a new instance of the LibraryException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        public LibraryException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the LibraryException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        /// <param name="inner">Inner exception occured</param>
        public LibraryException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new instance of the LibraryException class
        /// </summary>
        /// <param name="info">All data needed for serialization or deserialization</param>
        /// <param name="context">Serialized stream descriptor</param>
        protected LibraryException(
            [NotNull] SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
