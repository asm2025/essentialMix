using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Collections;

[Serializable]
public class LBoundLargerThanZeroException : Exception
{
	public LBoundLargerThanZeroException()
		: base(Resources.LBoundLargerThanZero)
	{
	}

	public LBoundLargerThanZeroException(string message) : base(message) { }

	public LBoundLargerThanZeroException(string message, Exception innerException) : base(message, innerException) { }

	protected LBoundLargerThanZeroException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
}