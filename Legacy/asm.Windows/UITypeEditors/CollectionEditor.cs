using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace asm.Windows.UITypeEditors
{
	/// <inheritdoc />
	public class CollectionEditor : System.ComponentModel.Design.CollectionEditor
	{
		/// <inheritdoc />
		public CollectionEditor(Type type) 
			: base(type)
		{
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) { return UITypeEditorEditStyle.Modal; }

		public static EventHandler FormCreated;
		public static EventHandler Apply;
		public static EventHandler Cancel;

		protected CollectionForm Form { get; private set; }

		protected override CollectionForm CreateCollectionForm()
		{
			Form = base.CreateCollectionForm();
			if (Form.AcceptButton is Button button) button.Click += (sender, args) => OnOKClick(EventArgs.Empty);
			button = Form.CancelButton as Button;
			if (button != null) button.Click += (sender, args) => OnCancelClick(EventArgs.Empty);
			OnFormCreated(EventArgs.Empty);
			return Form;
		}

		protected virtual void OnFormCreated(EventArgs args)
		{
			FormCreated?.Invoke(this, args);
		}

		protected virtual void OnOKClick(EventArgs args)
		{
			Apply?.Invoke(this, args);
		}

		protected virtual void OnCancelClick(EventArgs args)
		{
			Cancel?.Invoke(this, args);
		}
	}

	/// <inheritdoc />
	public class CollectionEditor<TItem> : CollectionEditor
	{
		/// <inheritdoc />
		public CollectionEditor(Type type) 
			: base(type)
		{
		}

		[NotNull] protected override Type CreateCollectionItemType() { return typeof(TItem); }
	}

	public class CollectionEditor<TCollection, TItem> : CollectionEditor<TItem>
	{
		/// <inheritdoc />
		public CollectionEditor() 
			: base(typeof(TCollection))
		{
		}
	}
}