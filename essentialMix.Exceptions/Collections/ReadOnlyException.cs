using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Collections;

[Serializable]
public class ReadOnlyException : Exception
{
	public ReadOnlyException()
	{
	}

	public ReadOnlyException(string message)
		: base(message)
	{
	}

	public ReadOnlyException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected ReadOnlyException([NotNull] SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}