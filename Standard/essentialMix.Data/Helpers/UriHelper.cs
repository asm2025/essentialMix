using System;
using System.IO;

namespace essentialMix.Data.Helpers
{
	public static class UriHelper
	{
		public static string ResolveUrl(Uri url, string relativeUri)
		{
			Uri uri;

			if (string.IsNullOrEmpty(relativeUri)) uri = url;
			else if (url == null) uri = new Uri(relativeUri);
			else uri = XmlUrlResolverHelper.CreateResolver().ResolveUri(url, relativeUri);

			return uri?.ToString();
		}

		public static Stream GetEntity(Uri url)
		{
			if (url == null) return null;

			Stream stm;

			try
			{
				stm = (Stream)XmlUrlResolverHelper.CreateResolver().GetEntity(url, null, typeof(Stream));
			}
			catch
			{
				stm = null;
			}

			return stm;
		}
	}
}