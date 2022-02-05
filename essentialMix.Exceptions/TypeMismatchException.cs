using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions;

[Serializable]
public class TypeMismatchException : InvalidCastException
{
	public TypeMismatchException()
		: base(Resources.TypeMismatch)
	{
	}

	public TypeMismatchException([NotNull] Type type)
		: base( string.Format(Resources.TypeNameMismatch, type.FullName))
	{
	}

	public TypeMismatchException(string message, int errorCode) 
		: base(message, errorCode)
	{
	}

	public TypeMismatchException(string message)
		: base(message)
	{
	}

	public TypeMismatchException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected TypeMismatchException([NotNull] SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}