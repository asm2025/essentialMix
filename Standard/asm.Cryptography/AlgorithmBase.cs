using System.Runtime.InteropServices;
using JetBrains.Annotations;
using asm.Helpers;
using asm.Patterns.Object;

namespace asm.Cryptography
{
	[ComVisible(true)]
	public abstract class AlgorithmBase : Disposable, IAlgorithmBase
	{
		private object _algorithm;

		protected AlgorithmBase([NotNull] object algorithm)
		{
			_algorithm = algorithm;
			AlgorithmName = _algorithm.GetType().Name;
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing) ObjectHelper.Dispose(ref _algorithm);
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