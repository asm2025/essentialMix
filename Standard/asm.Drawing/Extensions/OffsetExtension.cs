namespace asm.Drawing.Extensions
{
	public static class OffsetExtension
	{
		public static Padding AsPadding(this Offset thisValue) { return (Padding)thisValue; }

		public static Margin AsMargin(this Offset thisValue) { return (Margin)thisValue; }
	}
}