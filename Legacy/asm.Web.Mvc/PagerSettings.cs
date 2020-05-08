using System;
using System.Web.Mvc;
using JetBrains.Annotations;

namespace asm.Web.Mvc
{
	[Serializable]
	public class PagerSettings : PagerSettings<TagBuilder>
	{
		public PagerSettings([NotNull] FetchPageUrlHandler pageUrlHandler) 
			: base(pageUrlHandler)
		{
		}

		public PagerSettings(int itemsPerPage, [NotNull] FetchPageUrlHandler pageUrlHandler) 
			: base(itemsPerPage, pageUrlHandler)
		{
		}

		public PagerSettings(int itemsPerPage, int adjacentPageCount, [NotNull] FetchPageUrlHandler pageUrlHandler) 
			: base(itemsPerPage, adjacentPageCount, pageUrlHandler)
		{
		}
	}
}