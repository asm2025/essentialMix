namespace asm.Collections
{
	public interface IEditableObject : System.ComponentModel.IEditableObject
	{
		bool IsDirty { get; }
	}
}