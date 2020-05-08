using System.Collections.Generic;
using System.Linq;
using asm.Media.Youtube.Internal.CipherOperations;
using JetBrains.Annotations;

namespace asm.Media.Youtube.Internal
{
	internal class PlayerSource
	{
		public string Version { get; }

		public IReadOnlyList<ICipherOperation> CipherOperations { get; }

		public PlayerSource(string version, [NotNull] IEnumerable<ICipherOperation> cipherOperations)
		{
			Version = version;
			CipherOperations = cipherOperations.ToArray();
		}

		public string Decipher(string input)
		{
			return CipherOperations.Aggregate(input, (current, operation) => operation.Decipher(current));
		}
	}
}