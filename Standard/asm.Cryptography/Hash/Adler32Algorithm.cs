using System;
using System.IO;
using System.Security.Cryptography;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Cryptography.Hash
{
	/// <inheritdoc />
	/// <summary>
	/// Implementation of the Adler32 checksum routine.
	/// Adler-32 was never intended to be and is not a hash function. 
	/// It's purpose is error detection after decompression. 
	/// It serves that purpose well since it is fast and since errors in the compressed data are amplified by the decompressor.
	/// </summary>
	public class Adler32Algorithm : HashAlgorithm
	{
		/// <summary>
		/// Base for modulo arithmetic
		/// </summary>
		protected const int BASE = 65521;

		/// <summary>
		/// Number of iterations we can safely do before applying the modulo.
		/// </summary>
		protected const int N_MAX = 5552;

		private MemoryStream _stream;

		public Adler32Algorithm()
		{
		}

		/// <summary>
		/// Gets or sets the initial value or previous result. Use 1 for the first transformation.
		/// </summary>
		/// <value>
		/// The initial value.
		/// </value>
		protected int Initial { get; set; }

		public void Reset()
		{
			Initialize();
			Initial = 1;
		}

		public override void Initialize()
		{
			if (Initial < 1) Initial = 1;
			ObjectHelper.Dispose(ref _stream);
			_stream = new MemoryStream();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) ObjectHelper.Dispose(ref _stream);
			base.Dispose(disposing);
		}

		protected override void HashCore(byte[] array, int ibStart, int cbSize)
		{
			if (_stream == null) Initialize();
			_stream.Write(array, ibStart, cbSize);
		}

		[NotNull]
		protected override byte[] HashFinal()
		{
			_stream.Flush();

			byte[] bytes = _stream.ToArray();
			uint s1 = (uint)(Initial & 0xffff);
			uint s2 = (uint)((Initial >> 16) & 0xffff);

			int index = 0;
			int len = bytes.Length;

			while (len > 0)
			{
				int k = len < N_MAX ? len : N_MAX;
				len -= k;

				for (int i = 0; i < k; i++)
				{
					s1 += bytes[index++];
					s2 += s1;
				}
				s1 %= BASE;
				s2 %= BASE;
			}
			
			Initial = (int)((s2 << 16) | s1);
			return BitConverter.GetBytes(Initial);
		}
	}
}