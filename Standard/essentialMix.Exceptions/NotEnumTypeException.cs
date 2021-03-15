using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using essentialMix.Exceptions.Properties;

namespace essentialMix.Exceptions
{
	[Serializable]
	public class NotEnumTypeException : TypeMismatchException
	{
		public NotEnumTypeException()
			: base(Resources.ValueIsNotEnumType)
		{
		}

		public NotEnumTypeException([NotNull] Type type)
			: base( string.Format(Resources.ValueTypeIsNotEnumType, type.FullName))
		{
		}

		public NotEnumTypeException(string message, int errorCode) 
			: base(message, errorCode)
		{
		}

		public NotEnumTypeException(string message)
			: base(message)
		{
		}

		public NotEnumTypeException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected NotEnumTypeException([NotNull] SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}