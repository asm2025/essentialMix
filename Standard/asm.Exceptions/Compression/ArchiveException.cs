using System;
using System.Runtime.Serialization;
using asm.Exceptions.Properties;
using JetBrains.Annotations;

namespace asm.Exceptions.Compression
{
	/// <summary>
	/// Exception class for 7-zip archive open or read operations.
	/// </summary>
	[Serializable]
	public class ArchiveException : CompressionException
	{
		/// <summary>
		/// Initializes a new instance of the ArchiveException class
		/// </summary>
		public ArchiveException() : base(Resources.InvalidArchive) { }

		/// <summary>
		/// Initializes a new instance of the ArchiveException class
		/// </summary>
		/// <param name="message">Additional detailed message</param>
		public ArchiveException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the ArchiveException class
		/// </summary>
		/// <param name="message">Additional detailed message</param>
		/// <param name="inner">Inner exception occured</param>
		public ArchiveException(string message, Exception inner) : base(message, inner) { }

		/// <summary>
		/// Initializes a new instance of the ArchiveException class
		/// </summary>
		/// <param name="info">All data needed for serialization or deserialization</param>
		/// <param name="context">Serialized stream descriptor</param>
		protected ArchiveException(
			[NotNull] SerializationInfo info, StreamingContext context)
			: base(info, context) { }
	}
}
