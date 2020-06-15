namespace asm.Other.MarcGravell.Channel
{
	public enum ChannelMessageType : byte
	{
		Activation,
		Deactivation,
		MethodCall,
		ReturnValue,
		ReturnException
	}
}