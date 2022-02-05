namespace essentialMix.Patterns.Editable;

public abstract class EditableBase : IEditable
{
	protected EditableBase()
	{
	}

	public virtual void BeginEdit() { }

	public virtual void EndEdit() { IsDirty = true; }

	public virtual void CancelEdit() { }

	public bool IsDirty { get; protected set; }
}