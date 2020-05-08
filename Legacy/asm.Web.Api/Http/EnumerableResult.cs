using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using asm.Web.Exceptions;

namespace asm.Web.Api.Http
{
	public class EnumerableResult : PushStreamResult
	{
		/// <inheritdoc />
		public EnumerableResult([NotNull] HttpRequestMessage request, [NotNull] IEnumerable enumerable) 
			: this(request, enumerable, null, null)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] HttpRequestMessage request, [NotNull] IEnumerable enumerable, Action<Exception, Stream, HttpContent, TransportContext> onError) 
			: this(request, enumerable, null, onError)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] HttpRequestMessage request, [NotNull] IEnumerable enumerable, Action onCompleted, Action<Exception, Stream, HttpContent, TransportContext> onError)
			: this(request, () => enumerable, onCompleted, onError)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] HttpRequestMessage request, [NotNull] Func<IEnumerable> getResultFunction)
			: this(request, getResultFunction, null, null)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] HttpRequestMessage request, [NotNull] Func<IEnumerable> getResultFunction, Action<Exception, Stream, HttpContent, TransportContext> onError)
			: this(request, getResultFunction, null, onError)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] HttpRequestMessage request, [NotNull] Func<IEnumerable> getResultFunction, Action onCompleted, Action<Exception, Stream, HttpContent, TransportContext> onError) 
			: base(request, onCompleted, onError)
		{
			GetResultFunction = getResultFunction;
		}

		[NotNull]
		protected Func<IEnumerable> GetResultFunction { get; }

		/// <inheritdoc />
		protected override async Task WriteContent(HttpResponseMessage response, Stream stream, HttpContent content, TransportContext context, CancellationToken token)
		{
			if (token.IsCancellationRequested) return;

			IEnumerable enumerable = GetResultFunction();
			if (enumerable == null || token.IsCancellationRequested) return;

			HttpConfiguration configuration = Request.GetConfiguration();
			if (configuration == null) throw new HttpConfigurationMissingException();

			Type type = enumerable.GetType();
			ContentNegotiationResult result = configuration.Services
				.GetContentNegotiator()?
				.Negotiate(type, Request, configuration.Formatters);
			if (result == null || token.IsCancellationRequested) return;
			await result.Formatter.WriteToStreamAsync(type, enumerable, stream, content, context, token);
		}
	}

	public class EnumerableResult<T> : PushStreamResult
	{
		/// <inheritdoc />
		public EnumerableResult([NotNull] HttpRequestMessage request, [NotNull] IEnumerable<T> enumerable)
			: this(request, enumerable, null, null)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] HttpRequestMessage request, [NotNull] IEnumerable<T> enumerable, Action<Exception, Stream, HttpContent, TransportContext> onError)
			: this(request, enumerable, null, onError)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] HttpRequestMessage request, [NotNull] IEnumerable<T> enumerable, Action onCompleted, Action<Exception, Stream, HttpContent, TransportContext> onError)
			: this(request, () => enumerable, onCompleted, onError)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] HttpRequestMessage request, [NotNull] Func<IEnumerable<T>> getResultFunction)
			: this(request, getResultFunction, null, null)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] HttpRequestMessage request, [NotNull] Func<IEnumerable<T>> getResultFunction, Action<Exception, Stream, HttpContent, TransportContext> onError)
			: this(request, getResultFunction, null, onError)
		{
		}

		/// <inheritdoc />
		public EnumerableResult([NotNull] HttpRequestMessage request, [NotNull] Func<IEnumerable<T>> getResultFunction, Action onCompleted, Action<Exception, Stream, HttpContent, TransportContext> onError)
			: base(request, onCompleted, onError)
		{
			GetResultFunction = getResultFunction;
		}

		[NotNull]
		protected Func<IEnumerable<T>> GetResultFunction { get; }

		/// <inheritdoc />
		protected override async Task WriteContent(HttpResponseMessage response, Stream stream, HttpContent content, TransportContext context, CancellationToken token)
		{
			if (token.IsCancellationRequested) return;

			IEnumerable<T> enumerable = GetResultFunction();
			if (enumerable == null || token.IsCancellationRequested) return;

			HttpConfiguration configuration = Request.GetConfiguration();
			if (configuration == null) throw new HttpConfigurationMissingException();

			Type type = enumerable.GetType();
			ContentNegotiationResult result = configuration.Services
				.GetContentNegotiator()?
				.Negotiate(type, Request, configuration.Formatters);
			if (result == null || token.IsCancellationRequested) return;
			await result.Formatter.WriteToStreamAsync(type, enumerable, stream, content, context, token);
		}
	}
}