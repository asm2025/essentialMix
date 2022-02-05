using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Collections;

[Serializable]
public class VersionChangedException : InvalidOperationException
{
	public VersionChangedException()
		: base(Resources.VersionChanged)
	{
	}

	public VersionChangedException(string message) : base(message) { }

	public VersionChangedException(string message, Exception innerException) : base(message, innerException) { }

	protected VersionChangedException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
}