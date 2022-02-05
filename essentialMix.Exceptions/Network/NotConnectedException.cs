using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Network;

[Serializable]
public class NotConnectedException : Exception
{
	public NotConnectedException()
		: base(Resources.NotConnected)
	{
	}

	public NotConnectedException(string message) : base(message) { }

	public NotConnectedException(string message, Exception innerException) : base(message, innerException) { }

	protected NotConnectedException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
}