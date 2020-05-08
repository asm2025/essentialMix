using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using asm.Web.Extensions;
using JetBrains.Annotations;
using asm.Helpers;

namespace asm.Web.Helpers
{
	public static class HtmlHelper
	{
		public static string EncodeHtmlAttribute(string value)
		{
			return string.IsNullOrEmpty(value)
						? value
						: HttpUtility.HtmlAttributeEncode(value);
		}

		public static string EncodeJavaScript(string value) { return EncodeJavaScript(value, false); }

		public static string EncodeJavaScript(string value, bool addQuotes)
		{
			return string.IsNullOrEmpty(value)
						? value
						: HttpUtility.JavaScriptStringEncode(value, addQuotes);
		}

		public static string UrlDecode(string value, Encoding encoding = null)
		{
			return string.IsNullOrEmpty(value)
						? value
						: HttpUtility.UrlDecode(value, encoding ?? EncodingHelper.Default);
		}

		public static string UrlEncode(string value, Encoding encoding = null)
		{
			return string.IsNullOrEmpty(value)
						? value
						: HttpUtility.UrlEncode(HttpUtility.UrlPathEncode(value), encoding ?? EncodingHelper.Default);
		}

		public static string HtmlDecode(string value)
		{
			return string.IsNullOrEmpty(value)
						? value
						: HttpUtility.HtmlDecode(value);
		}

		public static string HtmlEncode(string value)
		{
			return string.IsNullOrEmpty(value)
						? value
						: HttpUtility.HtmlEncode(value);
		}

		public static NameValueCollection ParseQueryString(string value, Encoding encoding = null)
		{
			return string.IsNullOrEmpty(value)
						? null
						: HttpUtility.ParseQueryString(value, encoding ?? EncodingHelper.Default);
		}

		public static HtmlControl PageLinks([NotNull] PagerSettings<HtmlControl> pagerSettings)
		{
			if (pagerSettings.PageCount < 2) return null;

			bool invokeRootCreated = pagerSettings.OnRootCreated != null;
			bool invokePageCreated = pagerSettings.OnPageCreated != null;
			HtmlControl rootTag = new HtmlGenericControl(HtmlTextWriterTag.Ul.ToString());
			rootTag.AddCssClass("pagination");
			if (invokeRootCreated) pagerSettings.OnRootCreated(rootTag);

			string liTagName = HtmlTextWriterTag.Li.ToString();
			string spanTagName = HtmlTextWriterTag.Span.ToString();
			int firstPage = 1, lastPage = pagerSettings.PageCount;
			int firstAdjacentPage, lastAdjacentPage;

			if (pagerSettings.AdjacentPageCount > 0)
			{
				if (pagerSettings.CurrentPage <= pagerSettings.AdjacentPageCount * 2)
				{
					firstAdjacentPage = firstPage;
					lastAdjacentPage = Math.Min(firstPage + pagerSettings.AdjacentPageCount * 2, lastPage);
				}
				else if (pagerSettings.CurrentPage > lastPage - pagerSettings.AdjacentPageCount * 2)
				{
					firstAdjacentPage = lastPage - pagerSettings.AdjacentPageCount * 2;
					lastAdjacentPage = lastPage;
				}
				else
				{
					firstAdjacentPage = pagerSettings.CurrentPage - pagerSettings.AdjacentPageCount;
					lastAdjacentPage = pagerSettings.CurrentPage + pagerSettings.AdjacentPageCount;
				}
			}
			else
			{
				firstAdjacentPage = firstPage;
				lastAdjacentPage = lastPage;
			}

			if (pagerSettings.UseFirstAndLast)
			{
				HtmlControl li = new HtmlGenericControl(liTagName);
				HtmlControl child;

				if (pagerSettings.CurrentPage > firstPage)
				{
					child = new HtmlAnchor { HRef = pagerSettings.PageUrlHandler(firstPage) };
				}
				else
				{
					li.AddCssClass("disabled");
					child = new HtmlGenericControl(spanTagName);
				}

				if (invokePageCreated) pagerSettings.OnPageCreated(li, child, false);

				HtmlControl innerSpan = new HtmlGenericControl(spanTagName) { InnerHtml = "&laquo;" };
				innerSpan.Attribute("aria-hidden", "true");
				child.Controls.Add(innerSpan);
				li.Controls.Add(child);
				rootTag.Controls.Add(li);
			}

			if (pagerSettings.UsePreviousAndNext)
			{
				HtmlControl li = new HtmlGenericControl(liTagName);
				HtmlControl child;

				if (pagerSettings.CurrentPage > firstPage)
				{
					child = new HtmlAnchor { HRef = pagerSettings.PageUrlHandler(pagerSettings.CurrentPage - 1) };
				}
				else
				{
					li.AddCssClass("disabled");
					child = new HtmlGenericControl(spanTagName);
				}

				if (invokePageCreated) pagerSettings.OnPageCreated(li, child, false);

				HtmlControl innerSpan = new HtmlGenericControl(spanTagName) { InnerHtml = "&lt;" };
				innerSpan.Attribute("aria-hidden", "true");
				child.Controls.Add(innerSpan);
				li.Controls.Add(child);
				rootTag.Controls.Add(li);
			}

			if (pagerSettings.AdjacentPageCount > 0 && firstAdjacentPage > firstPage)
			{
				HtmlControl li = new HtmlGenericControl(liTagName);
				HtmlControl child = new HtmlAnchor { HRef = pagerSettings.PageUrlHandler(firstPage), InnerHtml = firstPage.ToString() };
				if (invokePageCreated) pagerSettings.OnPageCreated(li, child, false);
				li.Controls.Add(child);
				rootTag.Controls.Add(li);

				if (firstAdjacentPage > firstPage + 1)
				{
					HtmlControl liEllipse = new HtmlGenericControl(liTagName);
					HtmlControl childEllipse = new HtmlAnchor { HRef = pagerSettings.PageUrlHandler(firstAdjacentPage - 1), InnerHtml = "&hellip;" };
					if (invokePageCreated) pagerSettings.OnPageCreated(liEllipse, childEllipse, false);
					liEllipse.Controls.Add(childEllipse);
					rootTag.Controls.Add(liEllipse);
				}
			}

			for (int i = firstAdjacentPage; i <= lastAdjacentPage; i++)
			{
				bool isCurrent = pagerSettings.CurrentPage == i;
				HtmlControl li = new HtmlGenericControl(liTagName);
				HtmlControl child;

				if (isCurrent)
				{
					li.AddCssClass("active");
					child = new HtmlGenericControl(spanTagName) { InnerHtml = i.ToString() };
				}
				else
				{
					child = new HtmlAnchor {HRef = pagerSettings.PageUrlHandler(i), InnerHtml = i.ToString() };
				}

				if (invokePageCreated) pagerSettings.OnPageCreated(li, child, isCurrent);
				li.Controls.Add(child);
				rootTag.Controls.Add(li);
			}

			if (pagerSettings.AdjacentPageCount > 0 && lastAdjacentPage < lastPage)
			{
				if (lastAdjacentPage < lastPage - 1)
				{
					HtmlControl liEllipse = new HtmlGenericControl(liTagName);
					HtmlControl childEllipse = new HtmlAnchor { HRef = pagerSettings.PageUrlHandler(lastAdjacentPage + 1), InnerHtml = "&hellip;" };
					if (invokePageCreated) pagerSettings.OnPageCreated(liEllipse, childEllipse, false);
					liEllipse.Controls.Add(childEllipse);
					rootTag.Controls.Add(liEllipse);
				}

				HtmlControl li = new HtmlGenericControl(liTagName);
				HtmlControl child = new HtmlAnchor {HRef = pagerSettings.PageUrlHandler(lastPage), InnerHtml = lastPage.ToString()};
				if (invokePageCreated) pagerSettings.OnPageCreated(li, child, false);
				li.Controls.Add(child);
				rootTag.Controls.Add(li);
			}

			if (pagerSettings.UsePreviousAndNext)
			{
				HtmlControl li = new HtmlGenericControl(liTagName);
				HtmlControl child;

				if (pagerSettings.CurrentPage < lastPage)
				{
					child = new HtmlAnchor {HRef = pagerSettings.PageUrlHandler(pagerSettings.CurrentPage + 1)};
				}
				else
				{
					li.AddCssClass("disabled");
					child = new HtmlGenericControl(spanTagName);
				}

				if (invokePageCreated) pagerSettings.OnPageCreated(li, child, false);

				HtmlControl innerSpan = new HtmlGenericControl(spanTagName) { InnerHtml = "&gt;" };
				innerSpan.Attribute("aria-hidden", "true");
				child.Controls.Add(innerSpan);
				li.Controls.Add(child);
				rootTag.Controls.Add(li);
			}

			if (pagerSettings.UseFirstAndLast)
			{
				HtmlControl li = new HtmlGenericControl(liTagName);
				HtmlControl child;

				if (pagerSettings.CurrentPage < lastPage)
				{
					child = new HtmlAnchor {HRef = pagerSettings.PageUrlHandler(lastPage)};
				}
				else
				{
					li.AddCssClass("disabled");
					child = new HtmlGenericControl(spanTagName);
				}

				if (invokePageCreated) pagerSettings.OnPageCreated(li, child, false);

				HtmlControl innerSpan = new HtmlGenericControl(spanTagName) { InnerHtml = "&raquo;" };
				innerSpan.Attribute("aria-hidden", "true");
				child.Controls.Add(innerSpan);
				li.Controls.Add(child);
				rootTag.Controls.Add(li);
			}

			return rootTag;
		}
	}
}