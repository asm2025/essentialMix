using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Collections;

[Serializable]
public class LimitReachedException : Exception
{
	public LimitReachedException()
		: base(Resources.LimitReached)
	{
	}

	public LimitReachedException(string message) : base(message) { }

	public LimitReachedException(string message, Exception innerException) : base(message, innerException) { }

	protected LimitReachedException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
}