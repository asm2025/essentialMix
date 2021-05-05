using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Web.Mvc.Models
{
	public abstract class PagedViewModel<T>
	{
		private const string PAGE_URL_DEF = "#";

		private string _area;
		private string _controller;
		private string _action;
		[NotNull]
		private PagerSettings _pager;

		protected PagedViewModel([AspMvcController] string controller, [AspMvcAction] string action)
			: this(string.Empty, controller, action, new PagerSettings(_ => PAGE_URL_DEF))
		{
		}

		protected PagedViewModel([AspMvcArea] string area, [AspMvcController] string controller, [AspMvcAction] string action)
			: this(area, controller, action, new PagerSettings(_ => PAGE_URL_DEF))
		{
		}

		protected PagedViewModel([AspMvcController] string controller, [AspMvcAction] string action, [NotNull] PagerSettings pager)
			: this(string.Empty, controller, action, pager)
		{
		}

		protected PagedViewModel([AspMvcArea][CanBeNull]
			string area, [AspMvcController][CanBeNull]
			string controller, [AspMvcAction][CanBeNull]
			string action, [NotNull] PagerSettings pager)
		{
			_area = area ?? string.Empty;
			_controller = controller ?? "Home";
			_action = action ?? "Index";
			_pager = pager;
		}

		public virtual string Area
		{
			get => _area;
			protected set => _area = value;
		}

		public virtual string Controller
		{
			get => _controller;
			protected set => _controller = value;
		}

		public virtual string Action
		{
			get => _action;
			protected set => _action = value;
		}

		public string SortField { get; set; }

		public string Filter { get; set; }

		[NotNull]
		public Type ItemType => typeof(T);

		public abstract T ItemInstance { get; }

		[NotNull]
		public virtual ICollection<T> Items { get; set; } = Array.Empty<T>();

		[NotNull]
		public virtual PagerSettings Pager
		{
			get => _pager;
			set => _pager = value;
		}

		public MvcHtmlString HeaderAction(UrlHelper helper, string fieldName, bool newSortOrder = false)
		{
			return helper == null || string.IsNullOrEmpty(fieldName)
						? MvcHtmlString.Empty
						: BuildAction(helper, fieldName, newSortOrder);
		}

		public MvcHtmlString HeaderActionLink(UrlHelper helper, string fieldName, string text = null, bool newSortOrder = false)
		{
			if (helper == null || string.IsNullOrEmpty(fieldName)) return MvcHtmlString.Empty;

			TagBuilder a = new TagBuilder(HtmlTextWriterTag.A.ToString());
			a.MergeAttribute(HtmlTextWriterAttribute.Href.ToString(), HeaderAction(helper, fieldName, newSortOrder).ToString());
			a.SetInnerText(string.IsNullOrWhiteSpace(text) ? fieldName : text);
			return new MvcHtmlString(a.ToString());
		}

		public MvcHtmlString PagerAction(UrlHelper helper, int page) { return BuildAction(helper, page); }

		public MvcHtmlString PagerAction(UrlHelper helper, int page, string fieldName, bool newSortOrder = false) { return BuildAction(helper, fieldName, newSortOrder, page); }

		public virtual MvcHtmlString PagerActionLink(UrlHelper helper, int page, string text = null) { return PagerActionLink(helper, page, null, text); }

		public MvcHtmlString PagerActionLink(UrlHelper helper, int page, string fieldName, string text, bool newSortOrder = false)
		{
			if (helper == null) return MvcHtmlString.Empty;

			TagBuilder a = new TagBuilder(HtmlTextWriterTag.A.ToString());
			a.MergeAttribute(HtmlTextWriterAttribute.Href.ToString(), PagerAction(helper, page, fieldName, newSortOrder).ToString());
			a.SetInnerText(string.IsNullOrWhiteSpace(text) ? fieldName : text);
			return new MvcHtmlString(a.ToString());
		}

		public virtual string UrlAction([NotNull] UrlHelper helper)
		{
			return helper.Action(Action, Controller, new
			{
				area = Area
			});
		}

		public virtual string GetSortOrder(string fieldName, string currentValue = null, bool newSortOrder = false, bool forPager = true)
		{
			if (string.IsNullOrEmpty(fieldName)) return null;
			if (!newSortOrder && forPager) return SortField;

			currentValue = currentValue?.Trim();
			if (string.IsNullOrEmpty(currentValue)) currentValue = SortField;
			if (string.IsNullOrEmpty(currentValue) || currentValue.IsSame(fieldName + " desc")) return fieldName;
			if (currentValue.IsSame(fieldName)) return fieldName + " desc";
			return null;
		}

		protected MvcHtmlString BuildAction(UrlHelper helper, int? page = null) { return BuildAction(helper, null, false, page); }

		protected virtual MvcHtmlString BuildAction(UrlHelper helper, string fieldName, bool newSortOrder = false, int? page = null, bool forPager = true)
		{
			if (helper == null) return MvcHtmlString.Empty;

			StringBuilder sb = new StringBuilder();

			if (!string.IsNullOrEmpty(fieldName))
			{
				string currentValue = newSortOrder
					? string.IsNullOrEmpty(SortField) || !SortField.StartsWith(fieldName, StringComparison.OrdinalIgnoreCase) ? fieldName + " desc" : SortField
					: SortField;
				string sort = GetSortOrder(fieldName, currentValue, newSortOrder, forPager);
				if (sort != null) sb.Append($"sortOrder={WebUtility.UrlEncode(sort)}");
			}

			if (!string.IsNullOrEmpty(Filter))
			{
				if (sb.Length > 0) sb.Append('&');
				sb.Append($"filter={WebUtility.UrlEncode(Filter)}");
			}

			if (page.HasValue)
			{
				if (sb.Length > 0) sb.Append('&');
				sb.Append($"page={page.Value}");
			}
			else if (Pager.CurrentPage > 1)
			{
				if (sb.Length > 0) sb.Append('&');
				sb.Append($"page={Pager.CurrentPage}");
			}

			if (sb.Length > 0) sb.Insert(0, '?');
			sb.Insert(0, helper.Action(Action, Controller, new {Area}));
			return new MvcHtmlString(sb.ToString());
		}
	}
}