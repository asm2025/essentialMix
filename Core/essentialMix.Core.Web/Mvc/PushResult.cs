using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using essentialMix.Extensions;

namespace essentialMix.Core.Web.Mvc;

public class PushResult<T> : PushResultBase
{
	private readonly Task<T> _getResultTask;

	private bool _valueSet;
	private T _value;

	/// <inheritdoc />
	internal PushResult([NotNull] T value, Action onCompleted, Action<Exception, ActionContext> onError)
		: base(onCompleted, onError)
	{
		_value = value;
		_valueSet = true;
	}

	/// <inheritdoc />
	internal PushResult([NotNull] Task<T> getResultTask, Action onCompleted, Action<Exception, ActionContext> onError)
		: base(onCompleted, onError)
	{
		_getResultTask = getResultTask;
		_getResultTask.ConfigureAwait(false);
	}

	/// <inheritdoc />
	protected override async Task WriteContent(ActionContext context, CancellationToken token = default(CancellationToken))
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

			(IOutputFormatter selectedFormatter, OutputFormatterWriteContext formatterContext) = context.HttpContext.SelectFormatter(_value);
			if (selectedFormatter == null || token.IsCancellationRequested) return;
			await selectedFormatter.WriteAsync(formatterContext);
		}
		catch (Exception e)
		{
			OnError?.Invoke(e, context);
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
	public static PushResult<T> From<T>([NotNull] T value)
	{
		return From(value, null, null);
	}

	[NotNull]
	public static PushResult<T> From<T>([NotNull] T value, Action<Exception, ActionContext> onError)
	{
		return From(value, null, onError);
	}

	[NotNull]
	public static PushResult<T> From<T>([NotNull] T value, Action onCompleted, Action<Exception, ActionContext> onError)
	{
		return new PushResult<T>(value, onCompleted, onError);
	}

	[NotNull]
	public static PushResult<T> From<T>([NotNull] Task<T> getResultTask)
	{
		return From(getResultTask, null, null);
	}

	[NotNull]
	public static PushResult<T> From<T>([NotNull] Task<T> getResultTask, Action<Exception, ActionContext> onError)
	{
		return From(getResultTask, null, onError);
	}

	[NotNull]
	public static PushResult<T> From<T>([NotNull] Task<T> getResultTask, Action onCompleted, Action<Exception, ActionContext> onError)
	{
		return new PushResult<T>(getResultTask, onCompleted, onError);
	}
}