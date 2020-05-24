using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using asm.ComponentModel.DataAnnotations;
using asm.Helpers;

namespace asm.Extensions
{
	public static class ExceptionExtension
	{
		public static string GetStackTrace(this Exception thisValue)
		{
			if (thisValue == null) return null;

			StackFrame[] stackFrames = new StackTrace(thisValue, true).GetFrames();
			if (stackFrames == null) return string.Empty;
			
			IEnumerable<string> frames = stackFrames.Where(f => !f.GetMethod().IsDefined<HideFromStackTraceAttribute>())
				.Select(f => new StackTrace(f).ToString());
			return string.Concat(frames);
		}

		public static string CollectMessages(this Exception thisValue)
		{
			if (thisValue == null) return null;

			StringBuilder sb = new StringBuilder();
			ExceptionHelper.CollectMessages(thisValue, sb);
			return sb.ToString();
		}
	}
}