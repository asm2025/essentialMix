namespace asm.Windows.Html
{
	/// <summary>
	/// Represent the whole line of text in HTML.
	/// </summary>
	/// <remarks>
	/// The line has a width and height. The lines are calculated on fly from list
	/// of Elements so each line is a group of elements from some element i to
	/// element j in a list of elements. Because HTMLLabel has a iterator, one line
	/// object needs only the end index (j in our case), the beginning index
	/// (i in our case) is the last index of previous line!
	/// </remarks>
	public class TextLine
	{
		public TextLine(float width, float height, int lastElement)
		{
			Width = width;
			Height = height;
			LastElement = lastElement;
		}

		public override string ToString() { return $"TextLine: w={Width};h={Height};le={LastElement}"; }

		public int LastElement { get; set; }

		public float Height { get; set; }

		public float Width { get; set; }
	}
}