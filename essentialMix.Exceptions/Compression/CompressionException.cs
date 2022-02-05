using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Compression;

/// <summary>
/// Exception class for ArchiveUpdateCallback.
/// </summary>
[Serializable]
public class CompressionException : Exception
{
	/// <summary>
	/// Initializes a new instance of the CompressionException class
	/// </summary>
	public CompressionException() : base(Resources.CompressionError) { }

	/// <summary>
	/// Initializes a new instance of the CompressionException class
	/// </summary>
	/// <param name="message">Additional detailed message</param>
	public CompressionException(string message) : base(message) { }

	/// <summary>
	/// Initializes a new instance of the CompressionException class
	/// </summary>
	/// <param name="message">Additional detailed message</param>
	/// <param name="inner">Inner exception occured</param>
	public CompressionException(string message, Exception inner) : base(message, inner) { }

	/// <summary>
	/// Initializes a new instance of the CompressionException class
	/// </summary>
	/// <param name="info">All data needed for serialization or deserialization</param>
	/// <param name="context">Serialized stream descriptor</param>
	protected CompressionException(
		[NotNull] SerializationInfo info, StreamingContext context)
		: base(info, context) { }

}