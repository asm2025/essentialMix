namespace asm.Windows.Html
{
	/// <summary>
	/// Label is a special HTML tag named label used in component named HTMLLabel.
	/// Label has value, id, width and height.
	/// Labels are used so the programmer can change HTML text without the
	/// need for reparsing the text (parsing is an expensive operation).
	/// Labels can also be used for positioning other elements on top of
	/// HTMLLabel. Programmer can, for example, position TextBox on exactly
	/// the right place in text.
	/// </summary>
	public class Label : SimplePart
	{
		public Label(string id, string value, float width, float height)
			: base(PartType.SpecialAnchore, value)
		{
			ID = id;
			Width = width;
			Height = height;
		}

		public override string ToString() { return $"LABEL: ID={ID};value={Value};W={Width};H={Height}"; }

		public float Height { get; set; }

		public float Width { get; set; }

		public string ID { get; set; }
	}
}