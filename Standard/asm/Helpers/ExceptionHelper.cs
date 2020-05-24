using System;
using System.Text;
using asm.Extensions;

namespace asm.Helpers
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
					testAndCollect?.Invoke(value);

					if (value.InnerException != null)
						CollectMessages(value.InnerException, sb);
					else
						sb.AppendWithLine(value.Message);

					break;
			}

			if (!DebugHelper.DebugMode) return;
			sb.AppendWithLine(value.GetStackTrace());
		}
	}
}