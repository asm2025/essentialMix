using System;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;

namespace essentialMix.Cryptography
{
	public abstract class AlgorithmBase : Disposable, IAlgorithmBase
	{
		private readonly object _algorithm;

		protected AlgorithmBase([NotNull] object algorithm)
		{
			_algorithm = algorithm;
			AlgorithmName = _algorithm.GetType().Name;
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing && _algorithm is IDisposable disposable) ObjectHelper.Dispose(ref disposable);
			base.Dispose(disposing);
		}

		public object Algorithm => _algorithm;

		public string AlgorithmName { get; }

		public abstract object Clone();
	}

	public abstract class AlgorithmBase<T> : AlgorithmBase, IAlgorithmBase<T>
	{
		protected AlgorithmBase([NotNull] T algorithm)
			: base(algorithm)
		{
			Algorithm = (T)base.Algorithm;
		}

		public new T Algorithm { get; }
	}
}