﻿using System.Linq;
using System.Windows;
using System.Windows.Input;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;

namespace Raven.Studio.Controls.Editors
{
	using ActiproSoftware.Text;
	using ActiproSoftware.Windows.Controls.SyntaxEditor;

	public class QueryEditor : SyntaxEditor
	{
		private static readonly ISyntaxLanguage DefaultLanguage;

		static QueryEditor()
		{
			DefaultLanguage = SyntaxEditorHelper.LoadLanguageDefinitionFromResourceStream("RavenQuery.langdef");
		}

		public QueryEditor()
		{
			Document.Language = DefaultLanguage;
			IsTextDataBindingEnabled = true;
			AreLineModificationMarksVisible = false;
			IsLineNumberMarginVisible = false;

			foreach (var key in InputBindings.Where(x => x.Key == Key.Enter && x.Modifiers == ModifierKeys.Control).ToList())
			{
				InputBindings.Remove(key);
			}
		}


		public ICompletionProvider CompletionProvider
		{
			get { return (ICompletionProvider)GetValue(CompletionProviderProperty); }
			set
			{
				SetValue(CompletionProviderProperty, value);
			}
		}
		public static readonly DependencyProperty CompletionProviderProperty =
			DependencyProperty.Register("CompletionProvider", typeof(ICompletionProvider), typeof(QueryEditor), new PropertyMetadata(null, PropertyChangedCallback));

		private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			var editor = (QueryEditor)dependencyObject;
			editor.Document.Language.RegisterService<ICompletionProvider>((ICompletionProvider)args.NewValue);
		}
	}
}