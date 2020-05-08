namespace asm.Web.Mvc.Models
{
	public abstract class ViewModelEdit
	{
		protected ViewModelEdit()
		{
		}

		public ViewModelEditTypeEnum EditType { get; set; }
		public int? CurrentPage { get; set; }
		public string SortOrder { get; set; }
		public string Filter { get; set; }
	}
}