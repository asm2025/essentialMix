using System.Drawing;

namespace essentialMix.Drawing;

/// <summary>
///     RGB components.
/// </summary>
/// <remarks>
///     <para>The class encapsulates <b>RGB</b> color components.</para>
///     <para>
///         <note>
///             <see cref="System.Drawing.Imaging.PixelFormat">PixelFormat.Format24bppRgb</see>
///             actually means BGR format.
///         </note>
///     </para>
/// </remarks>
public class Rgb
{
	/// <summary>
	///     Index of red component.
	/// </summary>
	public const short R = 2;

	/// <summary>
	///     Index of green component.
	/// </summary>
	public const short G = 1;

	/// <summary>
	///     Index of blue component.
	/// </summary>
	public const short B = 0;

	/// <summary>
	///     Index of alpha component for ARGB images.
	/// </summary>
	public const short A = 3;

	/// <summary>
	///     Red component.
	/// </summary>
	public byte Red;

	/// <summary>
	///     Green component.
	/// </summary>
	public byte Green;

	/// <summary>
	///     Blue component.
	/// </summary>
	public byte Blue;

	/// <summary>
	///     Alpha component.
	/// </summary>
	public byte Alpha;

	/// <summary>
	///     Initializes a new instance of the <see cref="Rgb" /> class.
	/// </summary>
	public Rgb()
	{
		Red = 0;
		Green = 0;
		Blue = 0;
		Alpha = 255;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="Rgb" /> class.
	/// </summary>
	/// <param name="red">Red component.</param>
	/// <param name="green">Green component.</param>
	/// <param name="blue">Blue component.</param>
	public Rgb(byte red, byte green, byte blue)
	{
		Red = red;
		Green = green;
		Blue = blue;
		Alpha = 255;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="Rgb" /> class.
	/// </summary>
	/// <param name="red">Red component.</param>
	/// <param name="green">Green component.</param>
	/// <param name="blue">Blue component.</param>
	/// <param name="alpha">Alpha component.</param>
	public Rgb(byte red, byte green, byte blue, byte alpha)
	{
		Red = red;
		Green = green;
		Blue = blue;
		Alpha = alpha;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="Rgb" /> class.
	/// </summary>
	/// <param name="color">Initialize from specified <see cref="System.Drawing.Color">color.</see></param>
	public Rgb(Color color)
	{
		Red = color.R;
		Green = color.G;
		Blue = color.B;
		Alpha = color.A;
	}

	/// <summary>
	///     <see cref="System.Drawing.Color">Color</see> value of the class.
	/// </summary>
	public Color Color
	{
		get => Color.FromArgb(Alpha, Red, Green, Blue);
		set
		{
			Red = value.R;
			Green = value.G;
			Blue = value.B;
			Alpha = value.A;
		}
	}
}