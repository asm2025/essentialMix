using System;
using System.Text;
using JetBrains.Annotations;

namespace asm.IO
{
	/// <inheritdoc />
	/// <summary>
	/// A simple class derived from System.IO.StringWriter, but which allows
	/// the user to select which Encoding is used. This is most
	/// likely to be used with XmlTextWriter, which uses the Encoding
	/// property to determine which encoding to specify in the XML.
	/// </summary>
	public class StringWriter : System.IO.StringWriter
	{
		public StringWriter()
		{
		}

		public StringWriter(IFormatProvider formatProvider)
			: base(formatProvider)
		{
		}

		public StringWriter([NotNull] StringBuilder sb)
			: base(sb)
		{
		}

		public StringWriter([NotNull] StringBuilder sb, IFormatProvider formatProvider)
			: base(sb, formatProvider)
		{
		}

		public StringWriter([NotNull] Encoding encoding)
		{
			Encoding = encoding;
		}

		public StringWriter(IFormatProvider formatProvider, [NotNull] Encoding encoding)
			: base(formatProvider)
		{
			Encoding = encoding;
		}

		public StringWriter([NotNull] StringBuilder sb, [NotNull] Encoding encoding)
			: base(sb)
		{
			Encoding = encoding;
		}

		public StringWriter([NotNull] StringBuilder sb, IFormatProvider formatProvider, [NotNull] Encoding encoding)
			: base(sb, formatProvider)
		{
			Encoding = encoding;
		}

		public override Encoding Encoding { get; }
	}
}