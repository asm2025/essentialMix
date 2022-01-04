using System.Web.Routing;
using JetBrains.Annotations;

namespace essentialMix.Web.Routing;

public sealed class Route : RouteBase
{
	public Route([NotNull] string url) 
		: base(url)
	{
	}

	public string Name
	{
		get => NameInternal;
		set => NameInternal = value;
	}

	public object Defaults
	{
		get => DefaultsInternal;
		set => DefaultsInternal = value;
	}

	public string[] Namespaces
	{
		get => NamespacesInternal;
		set => NamespacesInternal = value;
	}

	public IRouteHandler RouteHandler
	{
		get => RouteHandlerInternal;
		set => RouteHandlerInternal = value;
	}
}