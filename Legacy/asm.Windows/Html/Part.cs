namespace asm.Windows.Html
{
	/// <summary>
	/// Base type for all the different HTML parts
	/// </summary>
	public class Part
	{
		public Part(PartType type) { Type = type; }

		public PartType Type { get; }
	}
}