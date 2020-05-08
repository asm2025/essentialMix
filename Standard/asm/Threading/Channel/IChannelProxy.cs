using System;

namespace asm.Threading.Channel
{
	internal interface IChannelProxy : IDisposable
	{
		/// <summary>Nongeneric version of the Instance </summary>
		object InstanceObject { get; }

		Channel Channel { get; }
		int? ObjectID { get; }
		byte DomainAddress { get; }
		Type ObjectType { get; }
		bool IsDisconnected { get; }

		void Disconnect();

		/// <summary>Connects the proxy for channel implementors. Used by Channel.</summary>
		void RegisterLocal(Channel channel, int? objectID, Action onDisconnect);
	}
}