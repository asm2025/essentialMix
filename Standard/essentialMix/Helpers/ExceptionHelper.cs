using System;
using System.Text;
using essentialMix.Extensions;

namespace essentialMix.Helpers
{
	public static class ExceptionHelper
	{
		public static void CollectMessages(Exception value, StringBuilder sb) { CollectMessages(value, sb, null); }
		public static void CollectMessages(Exception value, StringBuilder sb, Action<Exception> testAndCollect)
		{
			switch (value)
			{
				case null:
					return;
				case AggregateException ae:
					foreach (Exception inn in ae.InnerExceptions)
						CollectMessages(inn, sb);
					break;
				default:
					while (value.InnerException != null)
						value = value.InnerException;

					testAndCollect?.Invoke(value);
					sb.AppendWithLine(value.Message);
					break;
			}

			if (!DebugHelper.DebugMode) return;
			sb.AppendWithLine(value.GetStackTrace());
		}
	}
}