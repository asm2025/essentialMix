using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace asm.Helpers
{
	public static class ValueTaskHelper
	{
		public static ValueTask CompletedTask() { return new ValueTask(); }
		public static ValueTask<TResult> CompletedTask<TResult>() { return new ValueTask<TResult>(); }
		public static ValueTask<TResult> FromResult<TResult>(TResult result) { return new ValueTask<TResult>(result);}
		public static ValueTask FromCanceled(CancellationToken token) { return new ValueTask(Task.FromCanceled(token));}
		public static ValueTask<TResult> FromCanceled<TResult>(CancellationToken token) { return new ValueTask<TResult>(Task.FromCanceled<TResult>(token));}
		public static ValueTask FromException([NotNull] Exception exception) { return new ValueTask(Task.FromException(exception));}
		public static ValueTask<TResult> FromException<TResult>([NotNull] Exception exception) { return new ValueTask<TResult>(Task.FromException<TResult>(exception));}
	}
}