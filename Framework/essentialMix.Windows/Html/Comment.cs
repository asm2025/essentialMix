namespace essentialMix.Windows.Html
{
	/// <summary>
	/// HTML Comment
	/// </summary>
	public class Comment : SimplePart
	{
		public Comment(string value)
			: base(PartType.Comment, value) { }

		public override string ToString() { return $"Comment: {Value}"; }
	}
}