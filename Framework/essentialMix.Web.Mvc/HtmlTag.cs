using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using essentialMix.Extensions;
using HtmlAgilityPack;
using JetBrains.Annotations;

namespace essentialMix.Web.Mvc;

public class HtmlTag : IHtmlElement, IDictionary<string, string>
{
	private readonly TagBuilder _tagBuilder;

	private IList<IHtmlElement> _contents = new List<IHtmlElement>();
	private TagRenderMode? _tagRenderMode;

	public HtmlTag(HtmlTextWriterTag tag)
		: this(tag.ToString().ToLower())
	{
	}

	public HtmlTag([NotNull] string tagName)
	{
		if (tagName == null) throw new ArgumentNullException(nameof(tagName));
		_tagBuilder = new TagBuilder(tagName);
	}

	public override string ToString() { return ToHtml().ToHtmlString(); }

	public IList<IHtmlElement> Contents
	{
		get => _contents;
		set => _contents = value.ToList();
	}

	public string TagName => _tagBuilder.TagName;

	public HtmlTag Parent { get; set; }
	[NotNull]
	public IEnumerable<HtmlTag> Children => Contents.OfType<HtmlTag>();

	[NotNull]
	public IEnumerable<HtmlTag> Parents => Parent == null ? Enumerable.Empty<HtmlTag>() : new[] {Parent}.Concat(Parent.Parents);

	[NotNull]
	public IEnumerable<HtmlTag> Siblings
	{
		get
		{
			return Parent?.Children.Where(child => !ReferenceEquals(child, this)) ?? Enumerable.Empty<HtmlTag>();
		}
	}

	[NotNull]
	public HtmlTag ToggleAttribute([NotNull] string attribute, bool value)
	{
		if (attribute == null) throw new ArgumentNullException(nameof(attribute));
		if (value) return Attribute(attribute, attribute);
		Remove(attribute);
		return this;
	}

	[NotNull] 
	public IEnumerable<HtmlTag> Find([NotNull] Func<HtmlTag, bool> filter) { return Children.Where(filter).Concat(Children.SelectMany(c => c.Find(filter))); }

	[NotNull] 
	public HtmlTag Prepend([NotNull] params IHtmlElement[] elements) { return Insert(0, elements); }

	[NotNull] 
	public HtmlTag Prepend([NotNull] IEnumerable<IHtmlElement> elements) { return Insert(0, elements); }

	[NotNull] 
	public HtmlTag Prepend(string text) { return Insert(0, new HtmlText(text)); }

	[NotNull] 
	public HtmlTag Insert(int index, [NotNull] params IHtmlElement[] elements) { return Insert(index, elements.AsEnumerable()); }

	[NotNull]
	public HtmlTag Insert(int index, [NotNull] IEnumerable<IHtmlElement> elements)
	{
		if (index < 0 || index > _contents.Count) throw new IndexOutOfRangeException($"Cannot insert anything at index '{index}', content elements count = {Contents.Count}");

		foreach (IHtmlElement element in elements.Reverse())
		{
			_contents.Insert(index, element);
			element.Parent = this;
		}

		return this;
	}

	[NotNull]
	public HtmlTag Insert(int index, [NotNull] string text)
	{
		if (text == null) throw new ArgumentNullException(nameof(text));
		return Insert(index, new HtmlText(text));
	}

	[NotNull] 
	public HtmlTag Append([NotNull] params IHtmlElement[] elements) { return Append(elements.AsEnumerable()); }

	[NotNull]
	public HtmlTag Append([NotNull] IEnumerable<IHtmlElement> elements)
	{
		if (elements == null) throw new ArgumentNullException(nameof(elements));

		foreach (IHtmlElement element in elements)
		{
			_contents.Add(element);
			element.Parent = this;
		}

		return this;
	}

	[NotNull] 
	public HtmlTag Append(string text) { return Append(new HtmlText(text)); }

	public int Count => _tagBuilder.Attributes.Count;

	public bool IsReadOnly => _tagBuilder.Attributes.IsReadOnly;

	public ICollection<string> Keys => _tagBuilder.Attributes.Keys;

	public ICollection<string> Values => _tagBuilder.Attributes.Values;

	public string this[string attribute]
	{
		get => _tagBuilder.Attributes[attribute];
		set => _tagBuilder.Attributes[attribute] = value;
	}

	public IEnumerator<KeyValuePair<string, string>> GetEnumerator() { return _tagBuilder.Attributes.GetEnumerator(); }

	IEnumerator IEnumerable.GetEnumerator() { return _tagBuilder.Attributes.GetEnumerator(); }

	public void Add(KeyValuePair<string, string> item) { _tagBuilder.Attributes.Add(item); }
	public void Add(string attribute, string value) { _tagBuilder.Attributes.Add(attribute, value); }

	public bool Remove(KeyValuePair<string, string> item) { return _tagBuilder.Attributes.Remove(item); }

	public bool Remove(string attribute) { return _tagBuilder.Attributes.Remove(attribute); }

	public void Clear() { _tagBuilder.Attributes.Clear(); }

	public bool Contains(KeyValuePair<string, string> item) { return _tagBuilder.Attributes.Contains(item); }

	public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) { _tagBuilder.Attributes.CopyTo(array, arrayIndex); }

	public bool ContainsKey(string attribute) { return _tagBuilder.Attributes.ContainsKey(attribute); }

	public bool TryGetValue(string attribute, out string value) { return _tagBuilder.Attributes.TryGetValue(attribute, out value); }

	public bool HasAttribute([NotNull] string attribute) { return ContainsKey(attribute); }

	[NotNull]
	public HtmlTag Attribute([NotNull] string attribute, string value, bool replaceExisting = true)
	{
		if (attribute == null) throw new ArgumentNullException(nameof(attribute));
		_tagBuilder.MergeAttribute(attribute, value, replaceExisting);
		return this;
	}

	[NotNull]
	public HtmlTag Data([NotNull] string attribute, string value, bool replaceExisting = true)
	{
		if (attribute == null) throw new ArgumentNullException(nameof(attribute));
		return Attribute(attribute.StartsWith("data-") ? attribute : "data-" + attribute, value, replaceExisting);
	}

	[NotNull]
	public HtmlTag Data([NotNull] object data, bool replaceExisting = true)
	{
		if (data == null) throw new ArgumentNullException(nameof(data));
		RouteValueDictionary htmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(data);

		foreach (KeyValuePair<string, object> htmlAttribute in htmlAttributes)
		{
			string attribute = htmlAttribute.Key;
			Attribute(attribute.StartsWith("data-") ? attribute : "data-" + attribute,
					Convert.ToString(htmlAttribute.Value),
					replaceExisting);
		}
		return this;
	}

	[NotNull]
	public IReadOnlyDictionary<string, string> Styles
	{
		get
		{
			if (!TryGetValue("style", out string styles)) return new Dictionary<string, string>().AsReadOnly();

			string[] styleRulesSplit = styles.Split(';');
			var styleRuleStep1 =
				styleRulesSplit.Select(styleRule => new {StyleRule = styleRule, SeparatorIndex = styleRule.IndexOf(':')})
								.ToArray();
			var styleRuleStep2 = styleRuleStep1.Select(a =>
															new
															{
																StyleKey = a.StyleRule.Substring(0, a.SeparatorIndex),
																StyleValue = a.StyleRule.Substring(a.SeparatorIndex + 1, a.StyleRule.Length - a.SeparatorIndex - 1)
															}).ToArray();

			return styleRuleStep2.ToDictionary(a => a.StyleKey, a => a.StyleValue);
		}
		set
		{
			if (value.Count == 0)
				Remove("style");
			else
			{
				string newStyle = string.Join(";", value.Select(s => $"{s.Key}:{s.Value}"));
				Attribute("style", newStyle);
			}
		}
	}

	[NotNull]
	public HtmlTag Style([NotNull] string key, [NotNull] string value, bool replaceExisting = true)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		if (value == null) throw new ArgumentNullException(nameof(value));
		if (key.Contains(";")) throw new ArgumentException($"Style key cannot contain ';'! Key was '{key}'");
		if (value.Contains(";")) throw new ArgumentException($"Style value cannot contain ';'! Value was '{key}'");
		Dictionary<string, string> styles = Styles.ToDictionary(s => s.Key, s => s.Value);
		if (!styles.ContainsKey(key) || replaceExisting) styles[key] = value;
		Styles = styles;
		return this;
	}

	[NotNull]
	public HtmlTag RemoveStyle([NotNull] string key)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		Styles = Styles.Where(s => !string.Equals(s.Key, key)).ToDictionary(s => s.Key, s => s.Value);
		return this;
	}

	[NotNull]
	public IEnumerable<string> Classes
	{
		get => TryGetValue("class", out string classes) ? classes.Split(' ') : Enumerable.Empty<string>();
		set
		{
			if (!value.Any()) Remove("class");
			else Attribute("class", string.Join(" ", value));
		}
	}

	public bool HasClass(string @class)
	{
		return Classes.Any(c => string.Equals(c, @class));
	}

	[NotNull]
	public HtmlTag Class(string value)
	{
		if (value == null) return this;
		string[] classesToAdd = value.Split(' ');
		Classes = Classes.Concat(classesToAdd).Distinct();
		return this;
	}

	[NotNull]
	public HtmlTag RemoveClass([NotNull] string value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		string[] classesToRemove = value.Split(' ');
		Classes = Classes.Where(c => !classesToRemove.Contains(c));
		return this;
	}

	[NotNull] 
	public HtmlTag Merge<TKey, TValue>(IDictionary<TKey, TValue> attributes) { return Merge(attributes, false); }

	[NotNull]
	public HtmlTag Merge<TKey, TValue>(IDictionary<TKey, TValue> attributes, bool replaceExisting)
	{
		return attributes == null ? this : Merge(attributes.ToDictionary(a => Convert.ToString(a.Key), a => (object)a.Value), replaceExisting);
	}

	[NotNull] 
	public HtmlTag Merge([NotNull] IDictionary<string, object> attributes) { return Merge(attributes, false); }

	[NotNull]
	public HtmlTag Merge([NotNull] IDictionary<string, object> attributes, bool replaceExisting)
	{
		_tagBuilder.MergeAttributes(attributes, replaceExisting);
		if (attributes.TryGetValue("class", out object values)) Class(Convert.ToString(values));
		return this;
	}

	[NotNull] 
	public HtmlTag Merge(object attributes) { return Merge(HtmlHelper.AnonymousObjectToHtmlAttributes(attributes)); }

	[NotNull] 
	public HtmlTag Merge(object attributes, bool replaceExisting) { return Merge(HtmlHelper.AnonymousObjectToHtmlAttributes(attributes), replaceExisting); }

	[NotNull]
	public HtmlTag Render(TagRenderMode tagRenderMode)
	{
		_tagRenderMode = tagRenderMode;
		return this;
	}

	public IHtmlString ToHtml(TagRenderMode? tagRenderMode = null)
	{
		tagRenderMode ??= _tagRenderMode ?? TagRenderMode.Normal;
		StringBuilder stringBuilder = new StringBuilder();

		switch (tagRenderMode)
		{
			case TagRenderMode.StartTag:
				stringBuilder.Append(_tagBuilder.ToString(TagRenderMode.StartTag));
				break;
			case TagRenderMode.EndTag:
				stringBuilder.Append(_tagBuilder.ToString(TagRenderMode.EndTag));
				break;
			case TagRenderMode.SelfClosing:
				if (Contents.Any())
				{
					throw new InvalidOperationException(
														"Cannot render this tag with the self closing TagRenderMode because this tag has inner contents: " + this);
				}

				stringBuilder.Append(_tagBuilder.ToString(TagRenderMode.SelfClosing));
				break;
			default:
				stringBuilder.Append(_tagBuilder.ToString(TagRenderMode.StartTag));
				foreach (IHtmlElement content in Contents)
				{
					stringBuilder.Append(content.ToHtml());
				}
				stringBuilder.Append(_tagBuilder.ToString(TagRenderMode.EndTag));
				break;
		}
		return MvcHtmlString.Create(stringBuilder.ToString());
	}

	[NotNull]
	public static HtmlTag Parse([NotNull] IHtmlString html, bool validateSyntax = false)
	{
		if (html == null) throw new ArgumentNullException(nameof(html));
		return Parse(html.ToString(), validateSyntax);
	}

	[NotNull]
	public static HtmlTag Parse([NotNull] string html, bool validateSyntax = false)
	{
		if (html == null) throw new ArgumentNullException(nameof(html));
		return Parse(new StringReader(html), validateSyntax);
	}

	[NotNull]
	public static HtmlTag Parse([NotNull] TextReader textReader, bool validateSyntax = false)
	{
		if (textReader == null)
		{
			throw new ArgumentNullException(nameof(textReader));
		}
		HtmlDocument htmlDocument = new HtmlDocument {OptionCheckSyntax = validateSyntax};
		HtmlNode.ElementsFlags.Remove("option");
		htmlDocument.Load(textReader);
		return Parse(htmlDocument, validateSyntax);
	}

	[NotNull]
	public static HtmlTag Parse([NotNull] HtmlDocument htmlDocument, bool validateSyntax = false)
	{
		if (htmlDocument.ParseErrors.Any() && validateSyntax)
		{
			IEnumerable<string> readableErrors = htmlDocument.ParseErrors.Select(e => $"Code = {e.Code}, SourceText = {e.SourceText}, Reason = {e.Reason}");
			throw new InvalidOperationException($"Parse errors found: \n{string.Join(Environment.NewLine, (string[])readableErrors)}");
		}

		htmlDocument.OptionWriteEmptyNodes = true;
		return ParseHtmlTag(htmlDocument.DocumentNode.ChildNodes.Single());
	}

	[NotNull]
	public static IEnumerable<HtmlTag> ParseAll([NotNull] IHtmlString html, bool validateSyntax = false)
	{
		if (html == null) throw new ArgumentNullException(nameof(html));
		return ParseAll(html.ToString(), validateSyntax);
	}

	[NotNull]
	public static IEnumerable<HtmlTag> ParseAll([NotNull] string html, bool validateSyntax = false)
	{
		if (html == null) throw new ArgumentNullException(nameof(html));
		return ParseAll(new StringReader(html), validateSyntax);
	}

	[NotNull]
	public static IEnumerable<HtmlTag> ParseAll([NotNull] TextReader textReader, bool validateSyntax = false)
	{
		if (textReader == null) throw new ArgumentNullException(nameof(textReader));
		HtmlDocument htmlDocument = new HtmlDocument {OptionCheckSyntax = validateSyntax};
		HtmlNode.ElementsFlags.Remove("option");
		htmlDocument.Load(textReader);
		return ParseAll(htmlDocument, validateSyntax);
	}

	[NotNull]
	public static IEnumerable<HtmlTag> ParseAll([NotNull] HtmlDocument htmlDocument, bool validateSyntax = false)
	{
		if (htmlDocument.ParseErrors.Any() && validateSyntax)
		{
			IEnumerable<string> readableErrors =
				htmlDocument.ParseErrors.Select(
												e => $"Code = {e.Code}, SourceText = {e.SourceText}, Reason = {e.Reason}");
			throw new InvalidOperationException($"Parse errors found: \n{string.Join("\n", (string[])readableErrors)}");
		}
		return htmlDocument.DocumentNode.ChildNodes.Select(ParseHtmlTag);
	}

	[NotNull]
	private static HtmlTag ParseHtmlTag([NotNull] HtmlNode htmlNode)
	{
		HtmlTag htmlTag = new HtmlTag(htmlNode.Name);
		if (htmlNode.Closed && !htmlNode.ChildNodes.Any()) htmlTag.Render(TagRenderMode.SelfClosing);

		foreach (HtmlAttribute attribute in htmlNode.Attributes)
		{
			htmlTag.Attribute(attribute.Name, attribute.Value);
		}

		foreach (HtmlNode childNode in htmlNode.ChildNodes)
		{
			IHtmlElement childElement = null;

			switch (childNode.NodeType)
			{
				case HtmlNodeType.Element:
					childElement = ParseHtmlTag(childNode);
					break;
				case HtmlNodeType.Text:
					childElement = ParseHtmlText(childNode);
					break;
			}

			if (childElement != null) htmlTag.Append(childElement);
		}
		return htmlTag;
	}

	[NotNull] 
	private static HtmlText ParseHtmlText([NotNull] HtmlNode htmlNode) { return new HtmlText(htmlNode.InnerText); }

	public static explicit operator string(HtmlTag value) { return value?.ToString(); }
}