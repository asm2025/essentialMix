using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Text;

[Serializable]
public class NotWellFormatted : Exception
{
	public NotWellFormatted()
	{
	}

	public NotWellFormatted(string message)
		: base(message)
	{
	}

	public NotWellFormatted(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NotWellFormatted([NotNull] SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}