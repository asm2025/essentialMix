using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text;
using essentialMix.ComponentModel.DataAnnotations;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class ExceptionExtension
{
	[NotNull]
	public static string GetStackTrace(this Exception thisValue, bool excludeHidden = false)
	{
		if (thisValue == null) return string.Empty;
		if (!excludeHidden) return thisValue.StackTrace;

		StringBuilder sb = new StringBuilder();
		AppendStackTrace(thisValue, sb, true);
		return sb.ToString();
	}

	public static void AppendStackTrace(this Exception thisValue, [NotNull] StringBuilder sb, bool excludeHidden = false)
	{
		if (thisValue == null) return;

		if (!excludeHidden)
		{
			sb.AppendWithLine(thisValue.StackTrace);
			return;
		}

		StackTrace stackTrace = new StackTrace(thisValue, true);
		if (stackTrace.FrameCount == 0) return;
		if (sb.Length > 0) sb.AppendLine();

		for (int i = 0; i < stackTrace.FrameCount; i++)
		{
			StackFrame frame = stackTrace.GetFrame(i);
			if (frame ==  null || frame.GetMethod().IsDefined<HideFromStackTraceAttribute>()) continue;
			sb.Append(new StackTrace(frame));
		}
	}

	public static string CollectMessages(this Exception thisValue)
	{
		if (thisValue == null) return null;

		StringBuilder sb = new StringBuilder();
		ExceptionHelper.CollectMessages(thisValue, sb);
		return sb.ToString();
	}

	public static string Unwrap(this Exception thisValue)
	{
		switch (thisValue)
		{
			case null:
				return null;
			case AggregateException ae:
			{
				StringBuilder sb = new StringBuilder();

				foreach (Exception inn in ae.InnerExceptions)
				{
					Exception ex = inn;

					while (ex.InnerException != null) 
						ex = ex.InnerException;

					sb.AppendLine(ex.Message);
				}

				return sb.ToString();
			}
			default:
			{
				while (thisValue.InnerException != null) 
					thisValue = thisValue.InnerException;

				return thisValue.Message;
			}
		}
	}

	public static ExceptionDispatchInfo Capture(this Exception thisValue)
	{
		return thisValue == null
					? null
					: ExceptionDispatchInfo.Capture(thisValue);
	}
}