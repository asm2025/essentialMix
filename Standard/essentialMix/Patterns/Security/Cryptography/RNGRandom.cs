using System;
using System.Security.Cryptography;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Security.Cryptography
{
	[Serializable]
	public class RNGRandom : RandomNumberGenerator
	{
		private RandomNumberGenerator _rng = new RNGCryptoServiceProvider();

		public RNGRandom()
		{
		}

		public int Next()
		{
			byte[] data = new byte[Constants.INT_SIZE];
			GetBytes(data);
			return BitConverter.ToInt32(data, 0) & (int.MaxValue - 1);
		}

		public int Next(int maxValue) { return Next(0, maxValue); }

		public int Next(int minValue, int maxValue)
		{
			if (minValue > maxValue) throw new ArgumentOutOfRangeException();
			return (int)Math.Floor(minValue + (maxValue - minValue + 1) * NextDouble());
		}

		public double NextDouble()
		{
			byte[] data = new byte[sizeof(uint)];
			GetBytes(data);
			uint randUInt = BitConverter.ToUInt32(data, 0);
			return randUInt / (uint.MaxValue + 1.0);
		}

		public void NextBytes([NotNull] byte[] buffer) { GetBytes(buffer); }

		protected override void Dispose(bool disposing)
		{
			if (disposing) ObjectHelper.Dispose(ref _rng);
			base.Dispose(disposing);
		}

		public override void GetBytes(byte[] data)
		{
			if (data.Length < 1) return;
			_rng.GetBytes(data);
		}

		public override void GetNonZeroBytes(byte[] data)
		{
			if (data.Length < 1) return;
			_rng.GetNonZeroBytes(data);
		}
	}
}