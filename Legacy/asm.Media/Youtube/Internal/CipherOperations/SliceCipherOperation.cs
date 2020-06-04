using JetBrains.Annotations;

namespace asm.Media.Youtube.Internal.CipherOperations
{
	internal class SliceCipherOperation : ICipherOperation
	{
		private readonly int _index;

		public SliceCipherOperation(int index)
		{
			_index = index;
		}

		[NotNull]
		public string Decipher([NotNull] string input) { return input.Substring(_index); }
	}
}