using System;
using System.Web.UI.WebControls;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Web
{
	[Serializable]
	public class PagerSettings<TTag>
	{
		private int _itemsPerPage;
		private int _adjacentPageCount;
		private int _currentPage;
		private int _itemCount;

		public PagerSettings([NotNull] FetchPageUrlHandler pageUrlHandler)
			: this(10, 3, pageUrlHandler)
		{
		}

		public PagerSettings(int itemsPerPage, [NotNull] FetchPageUrlHandler pageUrlHandler)
			: this(itemsPerPage, 3, pageUrlHandler)
		{
		}

		public PagerSettings(int itemsPerPage, int adjacentPageCount, [NotNull] FetchPageUrlHandler pageUrlHandler)
		{
			ItemsPerPage = itemsPerPage;
			AdjacentPageCount = adjacentPageCount;
			PageUrlHandler = pageUrlHandler;
			CurrentPage = 1;
		}

		public int ItemsPerPage
		{
			get => _itemsPerPage;
			set => _itemsPerPage = value.NotBelow(1);
		}

		public int AdjacentPageCount
		{
			get => _adjacentPageCount;
			set => _adjacentPageCount = value.Within(0, 5);
		}

		public int CurrentPage
		{
			get
			{
				if (PageCount == 0) return 1;
				if (_currentPage > PageCount) _currentPage = PageCount;
				return _currentPage;
			}
			set => _currentPage = value.NotBelow(1);
		}

		public int PageCount => (int)Math.Ceiling((decimal)ItemCount / ItemsPerPage);

		public int ItemCount
		{
			get => _itemCount;
			set => _itemCount = value.NotBelow(0);
		}

		[NotNull]
		public FetchPageUrlHandler PageUrlHandler { get; set; }

		public bool UsePreviousAndNext { get; set; } = true;
		public bool UseFirstAndLast { get; set; }
		public Action<TTag> OnRootCreated { get; set; }
		public Action<TTag, TTag, bool> OnPageCreated { get; set; }
	}

	[Serializable]
	public class PagerSettings : PagerSettings<WebControl>
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