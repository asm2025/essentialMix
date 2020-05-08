using JetBrains.Annotations;

namespace asm.Web.Routing
{
	public sealed class IgnoreRoute : RouteBase
	{
		public IgnoreRoute([NotNull] string url) 
			: base(url)
		{
		}
	}
}