using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class ValueTaskExtension
	{
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static ValueTask AsValueTask<T>(this ValueTask<T> thisValue)
		{
			if (!thisValue.IsCompletedSuccessfully) return new ValueTask(thisValue.AsTask());
			thisValue.GetAwaiter().GetResult();
			return default(ValueTask);
		}
	}
}