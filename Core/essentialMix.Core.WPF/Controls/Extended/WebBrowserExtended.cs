using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Core.WPF.Controls.Extended;

public static class WebBrowserExtended
{
	private const string PROP_URI = "Uri";

	public static readonly DependencyProperty UriProperty = DependencyProperty.RegisterAttached(PROP_URI, typeof(string), typeof(WebBrowserExtended), new UIPropertyMetadata(null, UriChanged));

	public static string GetUri([NotNull] DependencyObject obj)
	{
		return (string)obj.GetValue(UriProperty);
	}

	public static void SetUri([NotNull] DependencyObject sender, string args)
	{
		sender.SetValue(UriProperty, args);
	}

	public static void UriChanged([NotNull] DependencyObject sender, DependencyPropertyChangedEventArgs args)
	{
		const string URL_EMPTY = "about:blank";

		if (sender is not WebBrowser browser) return;

		string url = (args.NewValue switch
						{
							string s => s,
							StringBuilder sb => sb.ToString(),
							Uri uri => uri.ToString(),
							UriBuilder builder => builder.Uri.ToString(),
							_ => null
						}).ToNullIfEmpty() ?? URL_EMPTY;
		browser.Navigate(url);
	}
}