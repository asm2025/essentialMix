using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Threading;

[Serializable]
public class NotAwaitableException : Exception
{
	public NotAwaitableException()
	{
	}

	public NotAwaitableException(string message)
		: base(message)
	{
	}

	public NotAwaitableException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NotAwaitableException([NotNull] SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}