namespace asm.Data.Patterns.Parameters
{
	public interface IFilterSettings
	{
		/// <summary>
		/// https://github.com/StefH/System.Linq.Dynamic.Core/wiki/Dynamic-Expressions
		/// </summary>
		DynamicFilter Filter { get; set; }
	}
}