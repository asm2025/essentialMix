namespace asm.Patterns.Editable
{
	public interface IEditable : System.ComponentModel.IEditableObject
	{
		bool IsDirty { get; }
	}
}