namespace asm.Drawing.Extensions
{
	public static class FontWeightExtension
	{
		public static bool IsBold(this Win32.FontWeight value) { return value >= Win32.FontWeight.FW_MEDIUM; }
	}
}