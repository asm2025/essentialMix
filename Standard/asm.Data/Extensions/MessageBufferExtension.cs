using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using JetBrains.Annotations;
using asm.Data.Helpers;

namespace asm.Data.Extensions
{
	public static class MessageBufferExtension
	{
		[NotNull]
		public static string ReadToEnd([NotNull] this MessageBuffer thisValue, XmlWriterSettings settings = null)
		{
			using (Message message = thisValue.CreateMessage())
			{
				if (message.IsEmpty) return string.Empty;

				StringBuilder sb = new StringBuilder();

				using (XmlWriter writer = XmlWriter.Create(sb, settings ?? XmlWriterHelper.CreateSettings()))
				{
					message.WriteMessage(writer);
				}

				return sb.ToString();
			}
		}
	}
}