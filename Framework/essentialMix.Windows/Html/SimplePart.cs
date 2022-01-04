namespace essentialMix.Windows.Html;

/// <summary>
/// Simple type which has only a type and a value
/// </summary>
public class SimplePart : Part
{
	public SimplePart(PartType type, string text)
		: base(type) { Value = text; }

	public string Value { get; set; }
}