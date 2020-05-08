using asm.Extensions;

namespace asm.Media.Youtube.Internal.CipherOperations
{
	internal class ReverseCipherOperation : ICipherOperation
	{
		public string Decipher(string input) { return input.Reverse(); }
	}
}