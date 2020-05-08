using System;
using JetBrains.Annotations;

namespace asm.Cryptography
{
	public interface IAlgorithmBase : ICloneable, IDisposable
	{
		[NotNull]
		object Algorithm { get; }
		string AlgorithmName { get; }
	}

	public interface IAlgorithmBase<out T> : IAlgorithmBase
	{
		[NotNull]
		new T Algorithm { get; }
	}
}