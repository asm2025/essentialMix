using System;
using System.Text;
using asm.Numeric;

namespace asm.Cryptography.Settings
{
	[Serializable]
	public class NumericSettings : Settings
	{
		private BitVectorMode _mode;

		/// <inheritdoc />
		public NumericSettings()
			: this(BitVectorMode.Hexadecimal, null)
		{
		}

		/// <inheritdoc />
		public NumericSettings(BitVectorMode mode)
			: this(mode, null)
		{
		}

		/// <inheritdoc />
		public NumericSettings(BitVectorMode mode, Encoding encoding)
			: base(encoding)
		{
			_mode = mode;
		}

		public virtual BitVectorMode Mode
		{
			get => _mode; 
			set => _mode = value;
		}
	}
}