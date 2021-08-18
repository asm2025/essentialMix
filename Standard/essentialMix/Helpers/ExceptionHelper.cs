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
					{
						Exception ex = inn;

						while (ex.InnerException != null) 
							ex = ex.InnerException;

						testAndCollect?.Invoke(ex);
						sb.AppendWithLine(ex.Message);
					}
					break;
				default:
					while (value.InnerException != null)
						value = value.InnerException;

					testAndCollect?.Invoke(value);
					sb.AppendWithLine(value.Message);
					break;
			}

			sb.AppendWithLine(value.StackTrace);
		}
	}
}