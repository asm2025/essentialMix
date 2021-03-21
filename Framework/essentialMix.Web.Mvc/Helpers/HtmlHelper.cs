using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using JetBrains.Annotations;

namespace essentialMix.Web.Mvc.Helpers
{
	public static class HtmlHelper
	{
		public static TagBuilder PageLinks([NotNull] PagerSettings pagerSettings)
		{
			if (pagerSettings.PageCount < 2) return null;

			bool invokeRootCreated = pagerSettings.OnRootCreated != null;
			bool invokePageCreated = pagerSettings.OnPageCreated != null;
			TagBuilder rootTag = new TagBuilder(HtmlTextWriterTag.Ul.ToString());
			rootTag.AddCssClass("pagination");
			if (invokeRootCreated) pagerSettings.OnRootCreated(rootTag);

			string liTagName = HtmlTextWriterTag.Li.ToString();
			string linkTagName = HtmlTextWriterTag.A.ToString();
			string spanTagName = HtmlTextWriterTag.Span.ToString();
			List<TagBuilder> tags = new List<TagBuilder>(pagerSettings.PageCount);
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
				TagBuilder li = new TagBuilder(liTagName);
				TagBuilder child;

				if (pagerSettings.CurrentPage > firstPage)
				{
					child = new TagBuilder(linkTagName);
					child.MergeAttribute("href", pagerSettings.PageUrlHandler(firstPage));
				}
				else
				{
					li.AddCssClass("disabled");
					child = new TagBuilder(spanTagName);
				}

				if (invokePageCreated) pagerSettings.OnPageCreated(li, child, false);

				TagBuilder innerSpan = new TagBuilder(spanTagName) {InnerHtml = "&laquo;"};
				innerSpan.MergeAttribute("aria-hidden", "true");
				child.InnerHtml = innerSpan.ToString();
				li.InnerHtml = child.ToString();
				tags.Add(li);
			}

			if (pagerSettings.UsePreviousAndNext)
			{
				TagBuilder li = new TagBuilder(liTagName);
				TagBuilder child;

				if (pagerSettings.CurrentPage > firstPage)
				{
					child = new TagBuilder(linkTagName);
					child.MergeAttribute("href", pagerSettings.PageUrlHandler(pagerSettings.CurrentPage - 1));
				}
				else
				{
					li.AddCssClass("disabled");
					child = new TagBuilder(spanTagName);
				}

				if (invokePageCreated) pagerSettings.OnPageCreated(li, child, false);

				TagBuilder innerSpan = new TagBuilder(spanTagName) {InnerHtml = "&lt;"};
				innerSpan.MergeAttribute("aria-hidden", "true");
				child.InnerHtml = innerSpan.ToString();
				li.InnerHtml = child.ToString();
				tags.Add(li);
			}

			if (pagerSettings.AdjacentPageCount > 0 && firstAdjacentPage > firstPage)
			{
				TagBuilder li = new TagBuilder(liTagName);
				TagBuilder child = new TagBuilder(linkTagName);
				child.MergeAttribute("href", pagerSettings.PageUrlHandler(firstPage));
				if (invokePageCreated) pagerSettings.OnPageCreated(li, child, false);
				child.InnerHtml = firstPage.ToString();
				li.InnerHtml = child.ToString();
				tags.Add(li);

				if (firstAdjacentPage > firstPage + 1)
				{
					TagBuilder liEllipse = new TagBuilder(liTagName);
					TagBuilder childEllipse = new TagBuilder(linkTagName) {InnerHtml = "&hellip;"};
					childEllipse.MergeAttribute("href", pagerSettings.PageUrlHandler(firstAdjacentPage - 1));
					if (invokePageCreated) pagerSettings.OnPageCreated(liEllipse, childEllipse, false);
					liEllipse.InnerHtml = childEllipse.ToString();
					tags.Add(liEllipse);
				}
			}

			for (int i = firstAdjacentPage; i <= lastAdjacentPage; i++)
			{
				bool isCurrent = pagerSettings.CurrentPage == i;
				TagBuilder li = new TagBuilder(liTagName);
				TagBuilder child;

				if (isCurrent)
				{
					li.AddCssClass("active");
					child = new TagBuilder(spanTagName);
				}
				else
				{
					child = new TagBuilder(linkTagName);
					child.MergeAttribute("href", pagerSettings.PageUrlHandler(i));
				}

				if (invokePageCreated) pagerSettings.OnPageCreated(li, child, isCurrent);
				child.InnerHtml = i.ToString();
				li.InnerHtml = child.ToString();
				tags.Add(li);
			}

			if (pagerSettings.AdjacentPageCount > 0 && lastAdjacentPage < lastPage)
			{
				if (lastAdjacentPage < lastPage - 1)
				{
					TagBuilder liEllip = new TagBuilder(liTagName);
					TagBuilder childEllip = new TagBuilder(linkTagName) { InnerHtml = "&hellip;" };
					childEllip.MergeAttribute("href", pagerSettings.PageUrlHandler(lastAdjacentPage + 1));
					if (invokePageCreated) pagerSettings.OnPageCreated(liEllip, childEllip, false);
					liEllip.InnerHtml = childEllip.ToString();
					tags.Add(liEllip);
				}

				TagBuilder li = new TagBuilder(liTagName);
				TagBuilder child = new TagBuilder(linkTagName);
				child.MergeAttribute("href", pagerSettings.PageUrlHandler(lastPage));
				if (invokePageCreated) pagerSettings.OnPageCreated(li, child, false);
				child.InnerHtml = lastPage.ToString();
				li.InnerHtml = child.ToString();
				tags.Add(li);
			}

			if (pagerSettings.UsePreviousAndNext)
			{
				TagBuilder li = new TagBuilder(liTagName);
				TagBuilder child;

				if (pagerSettings.CurrentPage < lastPage)
				{
					child = new TagBuilder(linkTagName);
					child.MergeAttribute("href", pagerSettings.PageUrlHandler(pagerSettings.CurrentPage + 1));
				}
				else
				{
					li.AddCssClass("disabled");
					child = new TagBuilder(spanTagName);
				}

				if (invokePageCreated) pagerSettings.OnPageCreated(li, child, false);

				TagBuilder innerSpan = new TagBuilder(spanTagName) { InnerHtml = "&gt;" };
				innerSpan.MergeAttribute("aria-hidden", "true");
				child.InnerHtml = innerSpan.ToString();
				li.InnerHtml = child.ToString();
				tags.Add(li);
			}

			if (pagerSettings.UseFirstAndLast)
			{
				TagBuilder li = new TagBuilder(liTagName);
				TagBuilder child;

				if (pagerSettings.CurrentPage < lastPage)
				{
					child = new TagBuilder(linkTagName);
					child.MergeAttribute("href", pagerSettings.PageUrlHandler(lastPage));
				}
				else
				{
					li.AddCssClass("disabled");
					child = new TagBuilder(spanTagName);
				}

				if (invokePageCreated) pagerSettings.OnPageCreated(li, child, false);

				TagBuilder innerSpan = new TagBuilder(spanTagName) { InnerHtml = "&raquo;" };
				innerSpan.MergeAttribute("aria-hidden", "true");
				child.InnerHtml = innerSpan.ToString();
				li.InnerHtml = child.ToString();
				tags.Add(li);
			}

			rootTag.InnerHtml = string.Concat(tags);
			return rootTag;
		}

		public static MvcHtmlString PageLinksAction([NotNull] PagerSettings pagerSettings)
		{
			TagBuilder tag = PageLinks(pagerSettings);
			return tag == null ? MvcHtmlString.Empty : new MvcHtmlString(tag.ToString());
		}

		[NotNull]
		public static SelectList TriStateSelectList(bool? value)
		{
			return new SelectList(new[]
			{
				new SelectListItem
				{
					Text = "Not Set",
					Value = string.Empty,
					Selected = !value.HasValue
				},
				new SelectListItem
				{
					Text = "True",
					Value = "true",
					Selected = value.HasValue && value.Value
				},
				new SelectListItem
				{
					Text = "False",
					Value = "false",
					Selected = value.HasValue && !value.Value
				}
			}, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
		}

		[NotNull]
		public static RouteValueDictionary ToHtmlAttributes(object value)
		{
			RouteValueDictionary pairs = null;
			if (value != null) pairs = System.Web.Mvc.HtmlHelper.AnonymousObjectToHtmlAttributes(value);
			return pairs ?? new RouteValueDictionary();
		}
	}
}
