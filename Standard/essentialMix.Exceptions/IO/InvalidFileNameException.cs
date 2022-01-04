using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.IO;

/// <summary>
/// Exception class for empty common root if file name array in SevenZipCompressor.
/// </summary>
[Serializable]
public class InvalidFileNameException : Exception
{
	/// <summary>
	/// Initializes a new instance of the InvalidFileNameException class
	/// </summary>
	public InvalidFileNameException() : base(Resources.InvalidFileName) { }

	/// <summary>
	/// Initializes a new instance of the InvalidFileNameException class
	/// </summary>
	/// <param name="message">Additional detailed message</param>
	public InvalidFileNameException(string message) : base(message) { }

	/// <summary>
	/// Initializes a new instance of the InvalidFileNameException class
	/// </summary>
	/// <param name="message">Additional detailed message</param>
	/// <param name="inner">Inner exception occured</param>
	public InvalidFileNameException(string message, Exception inner) : base(message, inner) { }

	/// <summary>
	/// Initializes a new instance of the InvalidFileNameException class
	/// </summary>
	/// <param name="info">All data needed for serialization or deserialization</param>
	/// <param name="context">Serialized stream descriptor</param>
	protected InvalidFileNameException(
		[NotNull] SerializationInfo info, StreamingContext context)
		: base(info, context) { }
}