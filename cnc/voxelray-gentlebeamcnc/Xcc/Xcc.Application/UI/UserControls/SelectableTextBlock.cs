using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Xcc.Application.UI.UserControls
{
    public class TextEditorWrapper
    {
        private static readonly Type? TextEditorType = Type.GetType("System.Windows.Documents.TextEditor, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        private static readonly PropertyInfo? IsReadOnlyProperty = TextEditorType?.GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly PropertyInfo? TextViewProperty = TextEditorType?.GetProperty("TextView", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo? RegisterMethod = TextEditorType?.GetMethod(
            "RegisterCommandHandlers",
            BindingFlags.Static | BindingFlags.NonPublic, 
            null, 
            new[] { typeof(Type), typeof(bool), typeof(bool), typeof(bool) }, 
            null);

        private static readonly Type? TextContainerType = Type.GetType("System.Windows.Documents.ITextContainer, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        private static readonly PropertyInfo? TextContainerTextViewProperty = TextContainerType?.GetProperty("TextView");
        private static readonly PropertyInfo? TextContainerProperty = typeof(TextBlock).GetProperty("TextContainer", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void RegisterCommandHandlers(Type controlType, bool acceptsRichContent, bool readOnly, bool registerEventListeners)
        {
            RegisterMethod?.Invoke(null, new object[] { controlType, acceptsRichContent, readOnly, registerEventListeners });
        }

        private object? TextEditor { get; }

        private TextEditorWrapper(object textContainer, FrameworkElement uiScope, bool isUndoEnabled)
        {
            if(TextEditorType is null)
                throw new NullReferenceException("Failed to create TextEditorWrapper. TextEditorType value is null.");

            TextEditor = Activator.CreateInstance(
                TextEditorType, 
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                null, 
                new[] { textContainer, uiScope, isUndoEnabled }, 
                null);
        }

        public static TextEditorWrapper CreateFor(TextBlock textBlock)
        {
            var textContainer = (TextContainerProperty?.GetValue(textBlock)) 
                ?? throw new NullReferenceException($"Failed to create TextEditorWrapper. TextBlock.TextContainer property value is null.");
            
            var textContainerTextView = (TextContainerTextViewProperty?.GetValue(textContainer)) 
                ?? throw new NullReferenceException($"Failed to create TextEditorWrapper. TextBlock.TextContainer.TextContainerTextView property value is null.");
            
            var editor = new TextEditorWrapper(textContainer, textBlock, false);
            IsReadOnlyProperty?.SetValue(editor.TextEditor, true);
            TextViewProperty?.SetValue(editor.TextEditor, textContainerTextView);

            return editor;
        }
    }


    public class SelectableTextBlock : TextBlock
    {
        static SelectableTextBlock()
        {
            FocusableProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata(true));
            TextEditorWrapper.RegisterCommandHandlers(typeof(SelectableTextBlock), true, true, true);

            // remove the focus rectangle around the control
            FocusVisualStyleProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata(null));
        }

        private TextEditorWrapper TextEditor { get; }

        public SelectableTextBlock()
        {
            TextEditor = TextEditorWrapper.CreateFor(this);
        }
    }
}
