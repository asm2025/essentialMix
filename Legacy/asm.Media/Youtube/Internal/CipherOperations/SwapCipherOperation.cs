using System.Text;
using JetBrains.Annotations;

namespace asm.Media.Youtube.Internal.CipherOperations
{
	internal class SwapCipherOperation : ICipherOperation
	{
		private readonly int _index;

		public SwapCipherOperation(int index)
		{
			_index = index;
		}

		[NotNull]
		public string Decipher([NotNull] string input)
		{
			StringBuilder sb = new StringBuilder(input)
			{
				[0] = input[_index],
				[_index] = input[0]
			};
			return sb.ToString();
		}
	}
}