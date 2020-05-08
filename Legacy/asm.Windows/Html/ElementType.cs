namespace asm.Windows.Html
{
	/// <summary>
	/// HTML is split on elements. But different elements changes the style of text
	/// (alignment, font, color...) and each such change is represented by Status object.
	/// So Status objects are inserted between HTML elements.
	/// The whole HTML now becomes the list of HTML elements and Style elements.
	/// </summary>
	public enum ElementType
	{
		Status,
		Html
	}
}