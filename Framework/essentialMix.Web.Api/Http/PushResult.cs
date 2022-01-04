using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using essentialMix.Web.Exceptions;

namespace essentialMix.Web.Api.Http;

public class PushResult<T> : PushResultBase
{
	private readonly Task<T> _getResultTask;

	private bool _valueSet;
	private T _value;

	/// <inheritdoc />
	internal PushResult([NotNull] HttpRequestMessage request, [NotNull] T value, Action onCompleted, Action<Exception, Stream, HttpContent, TransportContext> onError)
		: base(request, onCompleted, onError)
	{
		_value = value;
		_valueSet = true;
	}

	/// <inheritdoc />
	internal PushResult([NotNull] HttpRequestMessage request, [NotNull] Task<T> getResultTask, Action onCompleted, Action<Exception, Stream, HttpContent, TransportContext> onError)
		: base(request, onCompleted, onError)
	{
		_getResultTask = getResultTask;
		_getResultTask.ConfigureAwait(false);
	}

	/// <inheritdoc />
	protected override async Task WriteContent(HttpResponseMessage response, Stream stream, HttpContent content, TransportContext context, CancellationToken token = default(CancellationToken))
	{
		try
		{
			if (token.IsCancellationRequested) return;
			
			if (!_valueSet && _getResultTask != null)
			{
				_value = await _getResultTask;
				if (token.IsCancellationRequested) return;
				_valueSet = true;
			}

			if (!_valueSet) return;

			T value = _value;
			if (value is null || token.IsCancellationRequested) return;

			HttpConfiguration configuration = Request.GetConfiguration();
			if (configuration == null) throw new HttpConfigurationMissingException();

			Type type = value.GetType();
			ContentNegotiationResult result = configuration.Services
															.GetContentNegotiator()?
															.Negotiate(type, Request, configuration.Formatters);
			if (result == null || token.IsCancellationRequested) return;
			await result.Formatter.WriteToStreamAsync(type, value, stream, content, context, token);
		}
		catch (Exception e)
		{
			OnError?.Invoke(e, stream, content, context);
		}
		finally
		{
			OnCompleted?.Invoke();
		}
	}
}

public static class PushResult
{
	[NotNull]
	public static PushResult<T> From<T>([NotNull] HttpRequestMessage request, [NotNull] T value)
	{
		return From(request, value, null, null);
	}

	[NotNull]
	public static PushResult<T> From<T>([NotNull] HttpRequestMessage request, [NotNull] T value, Action<Exception, Stream, HttpContent, TransportContext> onError)
	{
		return From(request, value, null, onError);
	}

	[NotNull]
	public static PushResult<T> From<T>([NotNull] HttpRequestMessage request, [NotNull] T value, Action onCompleted, Action<Exception, Stream, HttpContent, TransportContext> onError)
	{
		return new PushResult<T>(request, value, onCompleted, onError);
	}

	[NotNull]
	public static PushResult<T> From<T>([NotNull] HttpRequestMessage request, [NotNull] Task<T> getResultTask)
	{
		return From(request, getResultTask, null, null);
	}

	[NotNull]
	public static PushResult<T> From<T>([NotNull] HttpRequestMessage request, [NotNull] Task<T> getResultTask, Action<Exception, Stream, HttpContent, TransportContext> onError)
	{
		return From(request, getResultTask, null, onError);
	}

	[NotNull]
	public static PushResult<T> From<T>([NotNull] HttpRequestMessage request, [NotNull] Task<T> getResultTask, Action onCompleted, Action<Exception, Stream, HttpContent, TransportContext> onError)
	{
		return new PushResult<T>(request, getResultTask, onCompleted, onError);
	}
}