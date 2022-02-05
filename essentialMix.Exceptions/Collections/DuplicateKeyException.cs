using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Collections;

[Serializable]
public class DuplicateKeyException : Exception
{
	public DuplicateKeyException()
		: base(Resources.DuplicateKey)
	{
	}

	public DuplicateKeyException(string message) : base(message) { }

	public DuplicateKeyException(string message, Exception innerException) : base(message, innerException) { }

	protected DuplicateKeyException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
}