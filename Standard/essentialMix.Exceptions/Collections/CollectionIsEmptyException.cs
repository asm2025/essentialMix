using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Collections;

[Serializable]
public class CollectionIsEmptyException : InvalidOperationException
{
	public CollectionIsEmptyException()
		: base(Resources.CollectionIsEmpty)
	{
	}

	public CollectionIsEmptyException(string message) : base(message) { }

	public CollectionIsEmptyException(string message, Exception innerException) : base(message, innerException) { }

	protected CollectionIsEmptyException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
}