using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Xcc.Application.UI.Behaviors
{
    public static class HighlightTermBehavior
    {
        public static readonly DependencyProperty TextProperty = 
            DependencyProperty.RegisterAttached(
                "Text",
                typeof(string),
                typeof(HighlightTermBehavior),
                new FrameworkPropertyMetadata("", OnTextChanged));

        public static string GetText(FrameworkElement frameworkElement) => (string)frameworkElement.GetValue(TextProperty);
        public static void SetText(FrameworkElement frameworkElement, string value) => frameworkElement.SetValue(TextProperty, value);


        public static readonly DependencyProperty TermToBeHighlightedProperty = 
            DependencyProperty.RegisterAttached(
                "TermToBeHighlighted",
                typeof(string),
                typeof(HighlightTermBehavior),
                new FrameworkPropertyMetadata(string.Empty, OnTextChanged));




        public static string GetTermToBeHighlighted(FrameworkElement frameworkElement) => (string)frameworkElement.GetValue(TermToBeHighlightedProperty);
        public static void SetTermToBeHighlighted(FrameworkElement frameworkElement, string value) => frameworkElement.SetValue(TermToBeHighlightedProperty, value);

        public static readonly DependencyProperty HighlightStyleProperty =
            DependencyProperty.RegisterAttached(
                "HighlightStyle",
                typeof(Style),
                typeof(HighlightTermBehavior),
                new FrameworkPropertyMetadata(
                    System.Windows.Application.Current.TryFindResource(typeof(Run)) as Style,
                    (d, e) => 
                    {
                        if (d is TextBlock textBlock)
                            SetTextBlockTextAndHighlightTerm(textBlock, GetText(textBlock), GetTermToBeHighlighted(textBlock));
                    }));

        public static Style GetHighlightStyle(DependencyObject obj) => (Style)obj.GetValue(HighlightStyleProperty);
        public static void SetHighlightStyle(DependencyObject obj, Style value) => obj.SetValue(HighlightStyleProperty, value);

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
                SetTextBlockTextAndHighlightTerm(textBlock, GetText(textBlock), GetTermToBeHighlighted(textBlock));
        }

        private static void SetTextBlockTextAndHighlightTerm(TextBlock textBlock, string text, string termToBeHighlighted)
        {
            textBlock.Text = string.Empty;

            if (TextIsEmpty(text))
                return;

            if (TextIsNotContainingTermToBeHighlighted(text, termToBeHighlighted))
            {
                AddPartToTextBlock(textBlock, text);
                return;
            }

            var textParts = SplitTextIntoTermAndNotTermParts(text, termToBeHighlighted);

            foreach (var textPart in textParts)
                AddPartToTextBlockAndHighlightIfNecessary(textBlock, termToBeHighlighted, textPart);
        }

        private static bool TextIsEmpty(string text)
        {
            return string.IsNullOrEmpty(text);
        }

        private static bool TextIsNotContainingTermToBeHighlighted(string text, string termToBeHighlighted)
        {
            return text.Contains(termToBeHighlighted, StringComparison.OrdinalIgnoreCase) == false;
        }

        private static void AddPartToTextBlockAndHighlightIfNecessary(TextBlock textBlock, string termToBeHighlighted, string textPart)
        {
            if (string.Equals(textPart, termToBeHighlighted, StringComparison.OrdinalIgnoreCase))
                AddHighlightedPartToTextBlock(textBlock, textPart);
            else
                AddPartToTextBlock(textBlock, textPart);
        }

        private static void AddPartToTextBlock(TextBlock textBlock, string part)
        {
            textBlock.Inlines.Add(new Run { Text = part });
        }

        private static void AddHighlightedPartToTextBlock(TextBlock textBlock, string part)
        {
            textBlock.Inlines.Add(new Run { Text = part, Style = GetHighlightStyle(textBlock) });
        }

        public static List<string> SplitTextIntoTermAndNotTermParts(string text, string term)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string>() { string.Empty };

            return Regex.Split(text, $@"({Regex.Escape(term)})", RegexOptions.IgnoreCase)
                        .Where(p => p != string.Empty)
                        .ToList();
        }
    }
}
