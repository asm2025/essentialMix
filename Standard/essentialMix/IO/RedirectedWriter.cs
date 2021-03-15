using System;
using System.Text;
using JetBrains.Annotations;
using essentialMix.Helpers;

namespace essentialMix.IO
{
	public class RedirectedWriter : BufferedWriter
	{
		/// <inheritdoc />
		public RedirectedWriter([NotNull] Action<string> writing)
			: this(writing, null, EncodingHelper.Default)
		{
		}

		/// <inheritdoc />
		public RedirectedWriter([NotNull] Action<string> writing, IFormatProvider formatProvider)
			: this(writing, formatProvider, EncodingHelper.Default)
		{
		}

		/// <inheritdoc />
		public RedirectedWriter([NotNull] Action<string> writing, [NotNull] Encoding encoding)
			: this(writing, null, encoding)
		{
		}

		/// <inheritdoc />
		public RedirectedWriter([NotNull] Action<string> writing, IFormatProvider formatProvider, [NotNull] Encoding encoding)
			: base(writing, formatProvider, encoding)
		{
		}

		/// <inheritdoc />
		public override void Write(string value) { Writing(value); }
	}
}