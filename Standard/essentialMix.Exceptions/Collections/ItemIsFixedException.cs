using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Collections;

[Serializable]
public class ItemIsFixedException : Exception
{
	public ItemIsFixedException()
		: base(Resources.ItemFixed)
	{
	}

	public ItemIsFixedException(string message) : base(message) { }

	public ItemIsFixedException(string message, Exception innerException) : base(message, innerException) { }

	protected ItemIsFixedException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
}