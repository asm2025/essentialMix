using JetBrains.Annotations;

namespace essentialMix.Web.Routing
{
	public class RouteValues
	{
		public RouteValues([NotNull] object list)
		{
			List = list;
		}

		[NotNull]
		public object List { get; set; }

		public object Create { get; set; }
		public object Read { get; set; }
		public object Update { get; set; }
		public object Delete { get; set; }
		public object DeleteSelected { get; set; }
		public object CustomAction { get; set; }
		public object CustomDataAction { get; set; }
		public bool ConfirmDelete { get; set; }
	}
}