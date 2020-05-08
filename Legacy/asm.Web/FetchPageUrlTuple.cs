using System.ComponentModel;

namespace asm.Web
{
	public delegate string FetchPageUrlHandler(int page);

	public class FetchPageUrlTuple
	{
		private FetchPageUrlHandler _previousHandler;
		private FetchPageUrlHandler _nextHandler;

		public FetchPageUrlTuple(FetchPageUrlHandler previousHandler, FetchPageUrlHandler nextHandler)
		{
			_previousHandler = previousHandler;
			_nextHandler = nextHandler;
		}

		[Description("The delegate that generates the previous URL. You must set both PreviousHandler and NextHandler. If only one of them is set but not the other, Null is always returned from either of them until the other is set.")]
		public FetchPageUrlHandler PreviousHandler
		{
			get => _previousHandler != null && _nextHandler != null ? _previousHandler : null;
			set => _previousHandler = value;
		}

		[Description("The delegate that generates the next URL. You must set both PreviousHandler and NextHandler. If only one of them is set but not the other, Null is always returned from either of them until the other is set.")]
		public FetchPageUrlHandler NextHandler
		{
			get => _previousHandler != null && _nextHandler != null ? _nextHandler : null;
			set => _nextHandler = value;
		}
	}
}