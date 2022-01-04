using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.Web;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class ControlExtension
{
	public static Page AsPage([NotNull] this Control thisValue)
	{
		Page page = thisValue.As(thisValue.Page);

		if (page == null)
		{
			HttpContext ctx = HttpContext.Current;
			if (ctx == null) return null;
			page = ctx.CurrentHandler as Page;
		}

		return page;
	}

	public static ClientScriptManager GetScriptManager([NotNull] this Control thisValue)
	{
		ClientScriptManager csm = AsPage(thisValue)?.ClientScript;
		return csm;
	}

	public static void StyleSheet([NotNull] this Control thisValue, [NotNull] params string[] urls)
	{
		if (urls.Length == 0) return;
		StyleSheet(thisValue, urls.Select(e => UriHelper.ToUri(e)).Where(e => e != null));
	}

	public static void StyleSheet([NotNull] this Control thisValue, [NotNull] params Uri[] urls)
	{
		if (urls.Length == 0) return;
		StyleSheet(thisValue, (IEnumerable<Uri>)urls);
	}

	public static void StyleSheet([NotNull] this Control thisValue, [NotNull] IEnumerable<Uri> urls)
	{
		Page page = AsPage(thisValue);
		if (page?.Header == null) return;

		ClientScriptManager csm = page.ClientScript;
		Type type = thisValue.GetType();

		foreach (Uri url in urls)
		{
			string key = UriHelper.GetFileName(url);
			if (string.IsNullOrEmpty(key) || csm.IsClientScriptBlockRegistered(type, key)) continue;

			HtmlLink link = new HtmlLink { Href = $"{url}?t={DateTime.Now.Ticks}" };
			link.Attributes.Add("rel", "stylesheet");
			link.Attributes.Add("type", "text/css");
			page.Header.Controls.Add(link);
			csm.RegisterClientScriptBlock(type, key, string.Empty);
		}
	}

	public static void ClientScriptBlock([NotNull] this Control thisValue, [NotNull] string script) { ClientScriptBlock(thisValue, script, true); }
	public static void ClientScriptBlock([NotNull] this Control thisValue, [NotNull] string script, bool startup) { ClientScriptBlock(thisValue, null, script, startup); }
	public static void ClientScriptBlock([NotNull] this Control thisValue, string key, [NotNull] string script) { ClientScriptBlock(thisValue, key, script, true); }
	public static void ClientScriptBlock([NotNull] this Control thisValue, string key, [NotNull] string script, bool startup) { ClientScriptBlock(thisValue, key, script, startup, true); }
	public static void ClientScriptBlock([NotNull] this Control thisValue, string key, [NotNull] string script, bool startup, bool addScriptTags) { ClientScriptBlock(thisValue, null, key, script, startup, addScriptTags); }
	public static void ClientScriptBlock([NotNull] this Control thisValue, Type type, string key, [NotNull] string script, bool startup, bool addScriptTags)
	{
		if (string.IsNullOrEmpty(script)) throw new ArgumentNullException(nameof(script));
		Page page = AsPage(thisValue);
		Type t = type ?? thisValue.GetType();
		string k = StringHelper.ToKey(key).IfNullOrEmpty(() => StringHelper.ToKey(t.FullName));
			
		if (startup)
		{
			ClientScriptManager cs = page.ClientScript;
			if (!string.IsNullOrEmpty(k) && cs.IsStartupScriptRegistered(t, k)) return;
			cs.RegisterStartupScript(t, k, script, addScriptTags);
			return;
		}

		if (!string.IsNullOrEmpty(k) && page.Header.FindControl(k) != null) return;

		Literal js = addScriptTags
						? new Literal
						{
							ID = k,
							Text =
								string.Format("{3}<{0} {1}=\"text/javascript\">{3}<!--{3}{2}{3}//-->{3}</{0}>",
											HtmlTextWriterTag.Script,
											HtmlTextWriterAttribute.Type,
											script,
											Environment.NewLine)
						}
						: new Literal { ID = k, Text = script };

		page.Header.Controls.Add(js);
	}

	public static void ClientScriptHeaderInclude([NotNull] this Control thisValue, [NotNull] params string[] urls)
	{
		if (urls.Length == 0) return;
		ClientScriptHeaderInclude(thisValue, urls.Select(e => UriHelper.ToUri(e)).Where(e => e != null));
	}

	public static void ClientScriptHeaderInclude([NotNull] this Control thisValue, [NotNull] params Uri[] urls)
	{
		if (urls.Length == 0) return;
		ClientScriptHeaderInclude(thisValue, (IEnumerable<Uri>)urls);
	}

	public static void ClientScriptHeaderInclude([NotNull] this Control thisValue, [NotNull] IEnumerable<Uri> urls)
	{
		Page page = AsPage(thisValue);
		if (page?.Header == null) return;

		ClientScriptManager csm = page.ClientScript;
		Type type = thisValue.GetType();

		foreach (Uri url in urls)
		{
			string key = UriHelper.GetFileName(url);
			if (string.IsNullOrEmpty(key) || csm.IsClientScriptBlockRegistered(type, key)) continue;

			Literal js = new Literal
			{
				ID = key,
				Text = $"{Environment.NewLine}<{HtmlTextWriterTag.Script} {HtmlTextWriterAttribute.Type}=\"text/javascript\" {HtmlTextWriterAttribute.Src}=\"{url}?t={DateTime.Now.Ticks}\"></{HtmlTextWriterTag.Script}>"
			};
			page.Header.Controls.Add(js);
		}
	}

	public static void ClientScriptInclude([NotNull] this Control thisValue, [NotNull] string virtualPath) { ClientScriptInclude(thisValue, virtualPath, null); }
	public static void ClientScriptInclude([NotNull] this Control thisValue, [NotNull] string virtualPath, string key) { ClientScriptInclude(thisValue, null, virtualPath, key); }
	public static void ClientScriptInclude([NotNull] this Control thisValue, Type type, [NotNull] string virtualPath, string key)
	{
		if (string.IsNullOrEmpty(virtualPath)) throw new ArgumentNullException(nameof(virtualPath));

		Page page = AsPage(thisValue);
		Type t = type ?? thisValue.GetType();
		string k = StringHelper.ToKey(key).IfNullOrEmpty(() => StringHelper.ToKey(UriHelper.GetFileName(virtualPath)));
		ClientScriptManager cs = page.ClientScript;
		if (!string.IsNullOrEmpty(k) && cs.IsClientScriptIncludeRegistered(t, k)) return;
		cs.RegisterClientScriptInclude(t, k, $"{page.ResolveUrl(virtualPath)}?t={DateTime.Now.Ticks}");
	}

	public static string GetWebResource([NotNull] this Control thisValue, [NotNull] string resourceName) { return GetWebResource(thisValue, null, resourceName); }

	public static string GetWebResource([NotNull] this Control thisValue, Type type, [NotNull] string resourceName)
	{
		if (string.IsNullOrEmpty(resourceName)) throw new ArgumentNullException(nameof(resourceName));

		Page page = thisValue.AsPage();
		return page.ClientScript.GetWebResourceUrl(type ?? thisValue.GetType(), resourceName);
	}

	public static void ClientScriptResource([NotNull] this Control thisValue, [NotNull] string resourceName) { ClientScriptResource(thisValue, null, resourceName); }
	public static void ClientScriptResource([NotNull] this Control thisValue, Type type, [NotNull] string resourceName)
	{
		if (string.IsNullOrEmpty(resourceName)) throw new ArgumentNullException(nameof(resourceName));

		Page page = AsPage(thisValue);
		page.ClientScript.RegisterClientScriptResource(type ?? thisValue.GetType(), resourceName);
	}

	public static void ClientScriptArray([NotNull] this Control thisValue, [NotNull] string name, params object[] values)
	{
		if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

		Page page = AsPage(thisValue);
		StringBuilder value = new StringBuilder();

		if (!values.IsNullOrEmpty())
		{
			foreach (object o in values)
			{
				value.Separator(',');

				if (o.IsNumeric()) value.Append(o);
				else value.AppendFormat("\"{0}\"", o);
			}
		}

		page.ClientScript.RegisterArrayDeclaration(name, value.ToString());
	}

	public static void CssHeaderBlock([NotNull] this Control thisValue, [NotNull] Style style) { CssHeaderBlock(thisValue, style, null); }
	public static void CssHeaderBlock([NotNull] this Control thisValue, [NotNull] Style style, string selector)
	{
		if (style == null || style.IsEmpty) throw new ArgumentNullException(nameof(style));

		Page page = AsPage(thisValue);
		if (string.IsNullOrEmpty(selector)) page.Header.StyleSheet.RegisterStyle(style, null);
		else page.Header.StyleSheet.CreateStyleRule(style, null, selector);
	}

	public static void CssHeaderInclude([NotNull] this Control thisValue, [NotNull] string virtualPath) { CssHeaderInclude(thisValue, virtualPath, null); }
	public static void CssHeaderInclude([NotNull] this Control thisValue, [NotNull] string virtualPath, string key) { CssHeaderInclude(thisValue, virtualPath, key, null); }
	public static void CssHeaderInclude([NotNull] this Control thisValue, [NotNull] string virtualPath, string key, string media)
	{
		if (string.IsNullOrEmpty(virtualPath)) throw new ArgumentNullException(nameof(virtualPath));

		Page page = AsPage(thisValue);
		string k = StringHelper.ToKey(key).IfNullOrEmpty(() => UriHelper.GetFileName(virtualPath));
		if (!string.IsNullOrEmpty(k) && page.Header.FindControl(k) != null) return;

		string m = string.IsNullOrEmpty(media) ? "all" : media;
		HtmlLink css = new HtmlLink
		{
			ID = k,
			Href = $"{page.ResolveUrl(virtualPath)}?{DateTime.Now.Ticks}"
		};

		css.Attribute(HtmlTextWriterAttribute.Rel, "stylesheet");
		css.Attribute(HtmlTextWriterAttribute.Type, "text/css");
		css.Attribute("media", m);
		page.Header.Controls.Add(css);
	}

	public static T FindControl<T>([NotNull] this Control thisValue) where T : Control { return FindControl<T>(thisValue, false); }

	public static T FindControl<T>([NotNull] this Control thisValue, bool childrenOnly) where T : Control { return FindControl<T>(thisValue, null, childrenOnly); }

	public static T FindControl<T>([NotNull] this Control thisValue, string id) where T : Control { return FindControl<T>(thisValue, id, false); }

	public static T FindControl<T>([NotNull] this Control thisValue, string id, bool childrenOnly) where T : Control
	{
		bool ignoreId = string.IsNullOrEmpty(id);
		T ctl = childrenOnly ? null : thisValue as T;
		if (ctl != null && (ignoreId || ctl.ID.IsSame(id))) return ctl;

		Stack<Control> lookupControls = new Stack<Control>();

		if (childrenOnly) foreach (Control child in thisValue.Controls) lookupControls.Push(child);
		else
		{
			MasterPage master;
			lookupControls.Push(thisValue);

			if (thisValue is Page page)
			{
				master = page.Master;

				while(master != null)
				{
					lookupControls.Push(master);
					master = master.Master;
				}
			}
			else if (thisValue.Page != null)
			{
				lookupControls.Push(thisValue.Page);
				master = thisValue.Page.Master;

				while(master != null)
				{
					lookupControls.Push(master);
					master = master.Master;
				}
			}
		}

		while(lookupControls.Count > 0)
		{
			Control current = lookupControls.Pop();
			ctl = current as T;
			if (ctl != null && (ignoreId || ctl.ID.IsSame(id))) break;

			foreach (Control child in current.Controls) lookupControls.Push(child);
		}

		return ctl;
	}

	public static HtmlForm FindForm([NotNull] this Control thisValue) { return PageForm(thisValue) ?? FindForm(thisValue, null); }

	public static HtmlForm FindForm([NotNull] this Control thisValue, string id) { return FindForm(thisValue, id, false); }

	public static HtmlForm FindForm([NotNull] this Control thisValue, string id, bool childrenOnly) { return FindControl<HtmlForm>(thisValue, id, childrenOnly); }

	[NotNull]
	public static string GetKey([NotNull] this Control thisValue) { return GetKey(thisValue, null); }

	[NotNull]
	public static string GetKey([NotNull] this Control thisValue, string suffix)
	{
		string ctlType = thisValue.GetType().FullName;
		StringBuilder sb = new StringBuilder();

		if (HttpContext.Current != null)
		{
			sb.Concat('_', StringHelper.ToKey(HttpContext.Current.Request.Url.AbsolutePath));
			if (HttpContext.Current.User != null) sb.Concat('_', StringHelper.ToKey(HttpContext.Current.User.Identity.Name));
		}

		sb.Concat('_', StringHelper.ToKey(ctlType));
		sb.Concat('_', suffix);
		return sb.ToString();
	}

	[NotNull]
	public static string GetPostBackReference([NotNull] this Control thisValue) { return GetPostBackReference(thisValue, (string)null); }

	[NotNull]
	public static string GetPostBackReference([NotNull] this Control thisValue, string argOrNullForLaterFormat)
	{
		return GetPostBackReference(thisValue, argOrNullForLaterFormat, false);
	}

	[NotNull]
	public static string GetPostBackReference([NotNull] this Control thisValue, string argOrNullForLaterFormat, bool registerForEventValidation)
	{
		Page page = thisValue.AsPage();
		ClientScriptManager cs = page.ClientScript;
		return cs.GetPostBackEventReference(thisValue, argOrNullForLaterFormat ?? "{0}", registerForEventValidation);
	}

	public static string GetPostBackReference([NotNull] this Control thisValue, PostBackOptions options) { return GetPostBackReference(thisValue, options, false); }

	public static string GetPostBackReference([NotNull] this Control thisValue, PostBackOptions options, bool registerForEventValidation)
	{
		if (options == null) return GetPostBackReference(thisValue, string.Empty, registerForEventValidation);
		Page page = thisValue.AsPage();
		ClientScriptManager cs = page.ClientScript;
		options.Argument ??= "{0}";
		return cs.GetPostBackEventReference(options, registerForEventValidation);
	}

	[NotNull]
	public static string GetCallbackReference([NotNull] this Control thisValue, [NotNull] string onCallbackCompleteName)
	{
		return GetCallbackReference(thisValue, "{0}", onCallbackCompleteName, null!, null, false);
	}

	[NotNull]
	public static string GetCallbackReference([NotNull] this Control thisValue, [NotNull] string onCallbackCompleteName, bool useAsync)
	{
		return GetCallbackReference(thisValue, "{0}", onCallbackCompleteName, null!, null, useAsync);
	}

	[NotNull]
	public static string GetCallbackReference([NotNull] this Control thisValue, [NotNull] string onCallbackCompleteName, [NotNull] string onBeforeCallbackScript, bool useAsync)
	{
		return GetCallbackReference(thisValue, "{0}", onCallbackCompleteName, onBeforeCallbackScript, null, useAsync);
	}

	[NotNull]
	public static string GetCallbackReference([NotNull] this Control thisValue, [NotNull] string onCallbackCompleteName, [NotNull] string onBeforeCallbackScript, string onErrorName, bool useAsync)
	{
		return GetCallbackReference(thisValue, "{0}", onCallbackCompleteName, onBeforeCallbackScript, onErrorName, useAsync);
	}

	[NotNull]
	public static string GetCallbackReference([NotNull] this Control thisValue, string onCallbackCompleteName, [NotNull] string argument, [NotNull] string onBeforeCallbackScript, string onErrorName, bool useAsync)
	{
		Page page = thisValue.AsPage();
		ClientScriptManager cs = page.ClientScript;
		return cs.GetCallbackEventReference(thisValue, argument, onCallbackCompleteName, onBeforeCallbackScript, onErrorName, useAsync);
	}

	[NotNull]
	public static string GetCallbackReference([NotNull] this Control thisValue, CallbackOptions options)
	{
		Page page = thisValue.AsPage();
		ClientScriptManager cs = page.ClientScript;
		return string.IsNullOrEmpty(options.Target)
					? cs.GetCallbackEventReference(thisValue, options.Argument, options.OnCallbackCompleteName, options.OnBeforeCallbackScript, options.OnErrorName,
													options.Asynchronous)
					: cs.GetCallbackEventReference(options.Target, options.Argument, options.OnCallbackCompleteName, options.OnBeforeCallbackScript, options.OnErrorName,
													options.Asynchronous);
	}

	public static T GetQueryStringValue<T>([NotNull] this Control thisValue, string key) { return GetQueryStringValue(thisValue, key, default(T)); }

	public static T GetQueryStringValue<T>([NotNull] this Control thisValue, string key, T defaultValue)
	{
		if (string.IsNullOrEmpty(key)) return defaultValue;
		Page page = AsPage(thisValue);
		return page == null ? defaultValue : page.Request.QueryString[key].To(defaultValue);
	}

	public static void HiddenField([NotNull] this Control thisValue, [NotNull] string name) { HiddenField(thisValue, name, string.Empty); }

	public static void HiddenField([NotNull] this Control thisValue, [NotNull] string name, [NotNull] string value)
	{
		if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

		Page page = AsPage(thisValue);
		page.ClientScript.RegisterHiddenField(name, value);
	}

	public static bool IsCallback([NotNull] this Control thisValue)
	{
		Page page = AsPage(thisValue);
		return page is { IsCallback: true };
	}

	public static bool IsFirstLoad([NotNull] this Control thisValue)
	{
		Page page = AsPage(thisValue);
		return page is { IsPostBack: false, IsCallback: false };
	}

	public static bool IsPostBack([NotNull] this Control thisValue)
	{
		Page page = AsPage(thisValue);
		return page is { IsPostBack: true };
	}

	public static void MetaTag([NotNull] this Control thisValue, [NotNull] string name, string content)
	{
		if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

		Page page = AsPage(thisValue);
		HtmlMeta hm = new HtmlMeta {Name = name, Content = content};
		page.Header.Controls.Add(hm);
	}

	public static HtmlForm PageForm([NotNull] this Control thisValue)
	{
		Page page = AsPage(thisValue);
		return page?.Form;
	}

	public static string RenderToString([NotNull] this Control thisValue)
	{
		StringBuilder sb = new StringBuilder();

		//open a temp stream to save rendered html into  
		using (MemoryStream memStr = new MemoryStream())
		{
			using (StreamWriter stream = new StreamWriter(memStr))
			{
				using (HtmlTextWriter wt = new HtmlTextWriter(stream))
				{
					try
					{
						thisValue.RenderControl(wt);
					}
					finally
					{
						memStr.Close();
						stream.Close();
						wt.Close();
					}

					wt.Flush();

					if (memStr.Length == 0 || !memStr.CanSeek || !memStr.CanRead)
					{
						wt.Close();
						stream.Close();
						memStr.Close();
						return null;
					}

					memStr.Seek(0, SeekOrigin.Begin);

					//read back the html from memory
					using (StreamReader rd = new StreamReader(memStr))
					{
						int read, x = 0, len = (int)memStr.Length;
						char[] bytes = new char[Math.Min(len, Constants.BUFFER_8_KB)];

						sb.EnsureCapacity(len);

						while (x < len && (read = rd.ReadBlock(bytes, x, bytes.Length)) > 0)
						{
							sb.Append(bytes, 0, read);
							x += read;
						}
					}
				}
			}
		}

		return sb.ToString();
	}
}