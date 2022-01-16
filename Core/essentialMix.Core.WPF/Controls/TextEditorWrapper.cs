using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;

namespace essentialMix.Core.WPF.Controls;

public class TextEditorWrapper
{
	private static readonly Type __textEditorType = Type.GetType("System.Windows.Documents.TextEditor, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
	private static readonly PropertyInfo __isReadOnlyProp = __textEditorType.GetProperty("IsReadOnly", Constants.BF_NON_PUBLIC_INSTANCE);
	private static readonly PropertyInfo __textViewProp = __textEditorType.GetProperty("TextView", Constants.BF_NON_PUBLIC_INSTANCE);
	private static readonly MethodInfo __registerMethod = __textEditorType.GetMethod("RegisterCommandHandlers", Constants.BF_NON_PUBLIC_STATIC, null, new[] { typeof(Type), typeof(bool), typeof(bool), typeof(bool) }, null);

	private static readonly Type __textContainerType = Type.GetType("System.Windows.Documents.ITextContainer, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
	private static readonly PropertyInfo __textContainerTextViewProp = __textContainerType.GetProperty("TextView");

	private static readonly PropertyInfo __textContainerProp = typeof(TextBlock).GetProperty("TextContainer", Constants.BF_NON_PUBLIC_INSTANCE);

	private readonly object _editor;

	public static void RegisterCommandHandlers(Type controlType, bool acceptsRichContent, bool readOnly, bool registerEventListeners)
	{
		__registerMethod.Invoke(null, new object[] { controlType, acceptsRichContent, readOnly, registerEventListeners });
	}

	[NotNull]
	public static TextEditorWrapper CreateFor(TextBlock tb)
	{
		object textContainer = __textContainerProp.GetValue(tb);
		TextEditorWrapper editor = new TextEditorWrapper(textContainer, tb, false);
		__isReadOnlyProp.SetValue(editor._editor, true);
		__textViewProp.SetValue(editor._editor, __textContainerTextViewProp.GetValue(textContainer));
		return editor;
	}

	public TextEditorWrapper(object textContainer, FrameworkElement uiScope, bool isUndoEnabled)
	{
		_editor = Activator.CreateInstance(__textEditorType, Constants.BF_NON_PUBLIC_INSTANCE | BindingFlags.CreateInstance, null, new[] { textContainer, uiScope, isUndoEnabled }, null);
	}
}