namespace asm.Collections
{
	public abstract class EditableObjectBase : IEditableObject
	{
		protected EditableObjectBase()
		{
		}

		public virtual void BeginEdit() { }

		public virtual void EndEdit() { IsDirty = true; }

		public virtual void CancelEdit() { }

		public bool IsDirty { get; protected set; }
	}
}