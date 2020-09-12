using System.Threading.Tasks;

namespace asm.Threading.Extensions
{
	public static class ValueTaskExtension
	{
		public static ValueTask AsValueTask<T>(this ValueTask<T> thisValue)
		{
			if (!thisValue.IsCompletedSuccessfully) return new ValueTask(thisValue.AsTask());
			thisValue.GetAwaiter().GetResult();
			return default(ValueTask);
		}
	}
}