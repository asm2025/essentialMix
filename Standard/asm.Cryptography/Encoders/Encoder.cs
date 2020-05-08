using JetBrains.Annotations;

namespace asm.Cryptography.Encoders
{
	/*
		Usage
			x.Encode(string);
			x.Decode(encodedText);
	*/
	public abstract class Encoder<T> : AlgorithmEncodeBase<T>, IEncode
		where T : IEncode
	{
		protected Encoder([NotNull] T algorithm)
			: base(algorithm)
		{
		}

		/// <inheritdoc />
		public virtual byte[] EncodeToBytes(string value) { return Algorithm.EncodeToBytes(value); }

		/// <inheritdoc />
		public virtual string Encode(string value) { return Algorithm.Encode(value); }

		/// <inheritdoc />
		public virtual string Encode(byte[] buffer) { return Algorithm.Encode(buffer); }

		/// <inheritdoc />
		public virtual string Encode(byte[] buffer, int startIndex, int count) { return Algorithm.Encode(buffer, startIndex, count); }

		/// <inheritdoc />
		public virtual byte[] DecodeToBytes(string value) { return Algorithm.DecodeToBytes(value); }

		/// <inheritdoc />
		public virtual string Decode(string value) { return Algorithm.Decode(value); }
	}
}