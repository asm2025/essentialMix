using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Compression
{
	/// <summary>
	/// Exception class for ArchiveExtractCallback.
	/// </summary>
	[Serializable]
	public class ExtractionFailedException : CompressionException
	{
		/// <summary>
		/// Initializes a new instance of the ExtractionFailedException class
		/// </summary>
		public ExtractionFailedException()
			: base(Resources.ExtractionFailed)
		{
		}

		/// <summary>
		/// Initializes a new instance of the ExtractionFailedException class
		/// </summary>
		/// <param name="message">Additional detailed message</param>
		public ExtractionFailedException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the ExtractionFailedException class
		/// </summary>
		/// <param name="message">Additional detailed message</param>
		/// <param name="inner">Inner exception occured</param>
		public ExtractionFailedException(string message, Exception inner) : base(message, inner) { }

		/// <summary>
		/// Initializes a new instance of the ExtractionFailedException class
		/// </summary>
		/// <param name="info">All data needed for serialization or deserialization</param>
		/// <param name="context">Serialized stream descriptor</param>
		protected ExtractionFailedException(
			[NotNull] SerializationInfo info, StreamingContext context)
			: base(info, context) { }
	}
}
