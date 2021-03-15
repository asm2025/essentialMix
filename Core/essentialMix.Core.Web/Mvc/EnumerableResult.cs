using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using essentialMix.Extensions;

namespace essentialMix.Core.Web.Mvc
{
	public class EnumerableResult : PushStreamResult
	{
		/// <inheritdoc />
		public EnumerableResult([NotNull] IEnumerable enumerable) 
			: this(enumerable, null, null)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] IEnumerable enumerable, Action<Exception, ActionContext> onError) 
			: this(enumerable, null, onError)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] IEnumerable enumerable, Action onCompleted, Action<Exception, ActionContext> onError)
			: this(() => enumerable, onCompleted, onError)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] Func<IEnumerable> getResultFunction)
			: this(getResultFunction, null, null)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] Func<IEnumerable> getResultFunction, Action<Exception, ActionContext> onError)
			: this(getResultFunction, null, onError)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] Func<IEnumerable> getResultFunction, Action onCompleted, Action<Exception, ActionContext> onError) 
			: base(onCompleted, onError)
		{
			GetResultFunction = getResultFunction;
		}

		[NotNull]
		protected Func<IEnumerable> GetResultFunction { get; }

		/// <inheritdoc />
		protected override Task WriteContent(ActionContext context, CancellationToken token = default(CancellationToken))
		{
			if (token.IsCancellationRequested) return Task.FromCanceled(token);

			IEnumerable enumerable = GetResultFunction();
			if (enumerable == null || token.IsCancellationRequested) return Task.FromCanceled(token);

			(IOutputFormatter selectedFormatter, OutputFormatterWriteContext formatterContext) = context.HttpContext.SelectFormatter(enumerable);
			return selectedFormatter.WriteAsync(formatterContext);
		}
	}

	public class EnumerableResult<T> : PushStreamResult
	{
		/// <inheritdoc />
		public EnumerableResult([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null, null)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] IEnumerable<T> enumerable, Action<Exception, ActionContext> onError)
			: this(enumerable, null, onError)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] IEnumerable<T> enumerable, Action onCompleted, Action<Exception, ActionContext> onError)
			: this(() => enumerable, onCompleted, onError)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] Func<IEnumerable<T>> getResultFunction)
			: this(getResultFunction, null, null)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] Func<IEnumerable<T>> getResultFunction, Action<Exception, ActionContext> onError)
			: this(getResultFunction, null, onError)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] Func<IEnumerable<T>> getResultFunction, Action onCompleted, Action<Exception, ActionContext> onError)
			: base(onCompleted, onError)
		{
			GetResultFunction = getResultFunction;
		}

		[NotNull]
		protected Func<IEnumerable<T>> GetResultFunction { get; }

		/// <inheritdoc />
		protected override Task WriteContent(ActionContext context, CancellationToken token = default(CancellationToken))
		{
			if (token.IsCancellationRequested) return Task.FromCanceled(token);

			IEnumerable<T> enumerable = GetResultFunction();
			if (enumerable == null || token.IsCancellationRequested) return Task.FromCanceled(token);

			(IOutputFormatter selectedFormatter, OutputFormatterWriteContext formatterContext) = context.HttpContext.SelectFormatter(enumerable);
			return selectedFormatter.WriteAsync(formatterContext);
		}
	}
}