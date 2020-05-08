using System.ServiceModel.Channels;
using JetBrains.Annotations;
using asm.Extensions;
using asm.Helpers;

namespace asm.Data.Helpers
{
	public static class MessageHelper
	{
		public static MessageBuffer CreateBufferedCopy([NotNull] ref Message value, int maxValue = int.MaxValue)
		{
			if (value.IsEmpty) return null;
			MessageBuffer buffer = value.CreateBufferedCopy(maxValue.NotBelow(10));
			ObjectHelper.Dispose(ref value);
			value = buffer.CreateMessage();
			return buffer;
		}
	}
}