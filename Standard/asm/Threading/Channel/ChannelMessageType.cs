namespace asm.Threading.Channel
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