using JetBrains.Annotations;

namespace essentialMix.Web.Routing;

public sealed class IgnoreRoute : RouteBase
{
	public IgnoreRoute([NotNull] string url) 
		: base(url)
	{
	}
}