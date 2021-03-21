using System;
using System.Web;
using System.Web.Mvc;
using JetBrains.Annotations;

namespace essentialMix.Web.Mvc
{
	public class HtmlText : IHtmlElement, IEquatable<HtmlText>
	{
		private string _text;

		public HtmlText()
			: this(null)
		{
		}

		public HtmlText(string text) { Text = text; }

		public override string ToString() { return Text; }

		public override bool Equals(object obj)
		{
			if (obj == null) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj.GetType() == GetType() && Equals((HtmlText)obj);
		}

		public override int GetHashCode() { return Text.GetHashCode(); }

		public virtual HtmlTag Parent { get; set; }

		public string Text
		{
			get => _text;
			set => _text = value ?? string.Empty;
		}

		public virtual IHtmlString ToHtml(TagRenderMode? tagRenderMode = null) { return MvcHtmlString.Create(Text); }

		public bool Equals(HtmlText other) { return other != null && string.Equals(Text, other.Text); }

		public static explicit operator string(HtmlText value) { return value?.Text; }

		[NotNull]
		public static explicit operator HtmlText(string value) { return new HtmlText(value); }
	}
}