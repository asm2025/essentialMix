namespace asm.Windows.Html
{
	/// <summary>
	/// HTML Plain text
	/// </summary>
	public class Text : SimplePart
	{
		public Text(string value)
			: base(PartType.Text, value) { }

		public override string ToString() { return $"TEXT: {Value}"; }
	}
}