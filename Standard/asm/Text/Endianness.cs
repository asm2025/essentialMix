namespace asm.Text
{
	/// <summary>
	/// Endianness of a converter
	/// </summary>
	public enum Endianness
	{
		/// <summary>
		/// Take the machine configuration
		/// </summary>
		Default,
		/// <summary>
		/// Little endian - least significant byte first
		/// </summary>
		LittleEndian,
		/// <summary>
		/// Big endian - most significant byte first
		/// </summary>
		BigEndian
	}
}
