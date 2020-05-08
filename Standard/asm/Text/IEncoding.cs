using System.Text;
using JetBrains.Annotations;

namespace asm.Text
{
	public interface IEncoding
	{
		[NotNull]
		Encoding Encoding { get; set; }
	}
}