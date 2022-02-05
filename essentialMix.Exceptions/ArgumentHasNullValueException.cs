using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions;

[Serializable]
public class ArgumentHasNullValueException : ArgumentException
{
	/// <inheritdoc />
	public ArgumentHasNullValueException()
		: base(Resources.ArgumentHasNullValue)
	{
	}

	/// <inheritdoc />
	public ArgumentHasNullValueException(string paramName)
		: base( string.Format(Resources.ArgumentNameHasNullValue, paramName))
	{
	}

	/// <inheritdoc />
	public ArgumentHasNullValueException(string paramName, Exception innerException)
		: base( string.Format(Resources.ArgumentNameHasNullValue, paramName), innerException)
	{
	}

	protected ArgumentHasNullValueException([NotNull] SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}