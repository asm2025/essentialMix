using System;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Future
{
	/// <inheritdoc />
	/// <summary>
	/// Implementation of IFuture which retrieves it value from a delegate.
	/// This is primarily used for FromFuture, which will transform another
	/// Future's value on demand.
	/// </summary>
	public class FutureProxy<T> : IFuture<T>
	{
		/// <summary>
		/// Creates a new FutureProxy using the given method
		/// to obtain the value when needed
		/// </summary>
		public FutureProxy([NotNull] Func<T> projection, [NotNull] Func<bool> hasValueProjection)
		{
			Projection = projection;
			HasValueProjection = hasValueProjection;
		}

		/// <inheritdoc />
		/// <summary>
		/// Returns the value of the Future
		/// </summary>
		public T Value => Projection();

		public bool HasValue => HasValueProjection();

		[NotNull]
		protected Func<T> Projection { get; }

		[NotNull]
		protected Func<bool> HasValueProjection { get; }

		/// <summary>
		/// Creates a new FutureProxy from an existing future using
		/// the supplied transformation to obtain the value as needed
		/// </summary>
		[NotNull]
		public static FutureProxy<T> FromFuture<TSource>([NotNull] IFuture<TSource> future, [NotNull] Func<TSource, T> projection)
		{
			return new FutureProxy<T>(() => projection(future.Value), () => future.HasValue);
		}
	}
}