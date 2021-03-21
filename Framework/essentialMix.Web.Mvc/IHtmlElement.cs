using System.Web;
using System.Web.Mvc;

namespace essentialMix.Web.Mvc
{
	public interface IHtmlElement
	{
		HtmlTag Parent { get; set; }
		IHtmlString ToHtml(TagRenderMode? tagRenderMode = null);
	}
}