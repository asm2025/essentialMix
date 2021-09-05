using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace essentialMix.MediaFoundation
{
	[Serializable]
	public class MFException : Exception, ISerializable
	{
		private string m_Message;

		public MFException(int hr)
		{
			HResult = hr;
		}

		// This constructor is used for deserialization.
		private MFException(
			[NotNull] SerializationInfo info, 
			StreamingContext context) :
			base( info, context )
		{
		}
        
		[NotNull]
		public override string Message => m_Message ??= MFError.GetErrorText(HResult);
	}
}