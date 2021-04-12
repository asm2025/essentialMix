using JetBrains.Annotations;

namespace TestWPF.ViewModels
{
	public abstract class ViewModelBase
	{
		protected ViewModelBase()
		{
			Title = GetType().Name.Replace("ViewModel", string.Empty);
		}

		[NotNull]
		public string Title { get; }
	}
}