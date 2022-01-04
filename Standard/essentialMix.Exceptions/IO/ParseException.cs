using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.IO;

[Serializable]
public class ParseException : Exception
{
	/// <summary>
	/// Creates a new <see cref="ParseException" /> with default values.
	/// </summary>
	public ParseException()
	{
	}

	/// <inheritdoc />
	public ParseException(string message)
		: base(message)
	{
	}

	/// <inheritdoc />
	public ParseException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>
	/// Creates a new <see cref="ParseException" /> containing a message and the
	/// file line number that the error occured.
	/// </summary>
	/// <param name="message">
	/// The message indicating the root cause of the error.
	/// </param>
	/// <param name="fileRowNumber">The file line number the error occured on.</param>
	/// <param name="columnNumber">The column number the error occured on.</param>
	public ParseException(string message, int fileRowNumber, int columnNumber)
		: base(message)
	{
		FileRowNumber = fileRowNumber;
		ColumnNumber = columnNumber;
	}

	/// <summary>
	/// Creates a new <see cref="ParseException" /> with serialized data.
	/// </summary>
	/// <param name="info">
	/// The <see cref="SerializationInfo" /> that contains information
	/// about the exception.
	/// </param>
	/// <param name="context">
	/// The <see cref="StreamingContext" /> that contains information
	/// about the source/destination of the exception.
	/// </param>
	protected ParseException([NotNull] SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		FileRowNumber = info.GetInt32(nameof(FileRowNumber));
		ColumnNumber = info.GetInt32(nameof(ColumnNumber));
	}

	/// <summary>
	/// When overridden in a derived class, sets the <see cref="SerializationInfo" />
	/// with information about the exception.
	/// </summary>
	/// <param name="info">
	/// The <see cref="SerializationInfo" /> that holds the serialized object data
	/// about the exception being thrown.
	/// </param>
	/// <param name="context">
	/// The <see cref="StreamingContext" /> that contains contextual information about the source
	/// or destination.
	/// </param>
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue(nameof(FileRowNumber), FileRowNumber);
		info.AddValue(nameof(ColumnNumber), ColumnNumber);
	}

	/// <summary>
	/// The line number in the file that the exception was thrown at.
	/// </summary>
	public int FileRowNumber { get; }

	/// <summary>
	/// The column number in the file that the exception was thrown at.
	/// </summary>
	public int ColumnNumber { get; }
}