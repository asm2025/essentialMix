using System.Web.Routing;

namespace asm.Web.Helpers
{
	public class RouteValueDictionaryHelper
	{
		public static RouteValueDictionary FromObject(object value)
		{
			switch (value)
			{
				case null:
					return null;
				case RouteValueDictionary dictionary:
					return dictionary;
				default:
					return new RouteValueDictionary(value);
			}
		}
	}
}