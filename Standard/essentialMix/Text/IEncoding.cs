using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Text
{
	public interface IEncoding
	{
		[NotNull]
		Encoding Encoding { get; set; }
	}
}