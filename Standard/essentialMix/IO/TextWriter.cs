using System;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.IO
{
	/// <inheritdoc />
	/// <summary>
	/// A simple class derived from System.IO.TextWriter, but which allows
	/// the user to select which Encoding is used. This is most
	/// likely to be used with XmlTextWriter, which uses the Encoding
	/// property to determine which encoding to specify in the XML.
	/// </summary>
	public class TextWriter : System.IO.TextWriter
	{
		public TextWriter()
		{
		}

		public TextWriter(IFormatProvider formatProvider) 
			: base(formatProvider)
		{
		}

		public TextWriter([NotNull] Encoding encoding)
		{
			Encoding = encoding;
		}

		public TextWriter(IFormatProvider formatProvider, [NotNull] Encoding encoding)
			: base(formatProvider)
		{
			Encoding = encoding;
		}

		public override Encoding Encoding { get; }
	}
}