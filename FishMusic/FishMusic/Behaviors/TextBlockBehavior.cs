using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FishMusic.Behaviors
{
    static class TextBlockBehavior
    {
        public static string GetUpperText(DependencyObject obj) { return (string)obj.GetValue(UpperTextProperty); }
        public static void SetUpperText(DependencyObject obj, string value) { obj.SetValue(UpperTextProperty, value); }

        public static readonly DependencyProperty UpperTextProperty = DependencyProperty.RegisterAttached("UpperText", typeof(string), typeof(TextBlockBehavior), new UIPropertyMetadata(string.Empty, OnUpperTextChanged));

        private static void OnUpperTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var element = obj as TextBlock;
            if (element == null) throw new ArgumentException();
            element.Text = e.NewValue.ToString().ToUpper();
        }

        public static readonly DependencyProperty PlaceHolderTextProperty = DependencyProperty.RegisterAttached(
            "PlaceHolderText", typeof(string), typeof(TextBlockBehavior), new PropertyMetadata(default(string)));

        public static void SetPlaceHolderText(DependencyObject element, string value)
        {
            element.SetValue(PlaceHolderTextProperty, value);
            var txt = element as TextBlock;
            if (txt == null) throw new ArgumentException();
            DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock)).RemoveValueChanged(txt, TextChangedHandler);
            DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock)).AddValueChanged(txt, TextChangedHandler);
            TextChangedHandler(element, EventArgs.Empty);
        }

        private static void TextChangedHandler(object sender, EventArgs eventArgs)
        {
            var txt = (TextBlock)sender;
            if (string.IsNullOrEmpty(txt.Text))
                txt.Text = GetPlaceHolderText(txt);
        }

        public static string GetPlaceHolderText(DependencyObject element)
        {
            return (string)element.GetValue(PlaceHolderTextProperty);
        }

        abstract class FormatRule
        {
            public abstract string RegexPattern { get; }
            public abstract IList<Run> GetRun(Match regexMatch);
        }

        class HeaderFormatRule : FormatRule
        {
            public override IList<Run> GetRun(Match regexMatch)
            {
                var result = new List<Run>();
                var run = new Run(regexMatch.Groups["text"].Value) { FontSize = 16, FontWeight = FontWeights.Bold };
                result.Add(run);
                return result;
            }

            public override string RegexPattern => "^##(?<text>(.*?))$";
        }

        class EnumerationRule : FormatRule
        {
            public override string RegexPattern => "^- (?<text>(.*?))$";

            public override IList<Run> GetRun(Match regexMatch)
            {
                return new List<Run> { new Run("• " + regexMatch.Groups["text"].Value) };
            }
        }

        class ItalicRule : FormatRule
        {
            public override string RegexPattern => @"^\[i\](?<text>(.*?))$";

            public override IList<Run> GetRun(Match regexMatch)
            {
                return new List<Run> { new Run(regexMatch.Groups["text"].Value) { FontStyle = FontStyles.Italic } };
            }
        }

        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached(
            "FormattedText", typeof(string), typeof(TextBlockBehavior), new PropertyMetadata(default(string), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var textBlock = dependencyObject as TextBlock;
            if (textBlock == null) throw new ArgumentException();
            var rules = new List<FormatRule> { new HeaderFormatRule(), new EnumerationRule(), new ItalicRule() };

            var inlines = textBlock.Inlines;
            inlines.Clear();

            foreach (var line in dependencyPropertyChangedEventArgs.NewValue.ToString().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                var tmpLine = line;
                bool success = false;
                foreach (var collection in from rule in rules let regex = new Regex(rule.RegexPattern) let match = regex.Match(tmpLine) where match.Success select rule.GetRun(match))
                {
                    inlines.AddRange(collection);
                    success = true;
                    break;
                }
                if (!success) inlines.Add(new Run(tmpLine));
                inlines.Add(Environment.NewLine);
            }
        }

        public static void SetFormattedText(DependencyObject element, string value)
        {
            element.SetValue(FormattedTextProperty, value);
        }

        public static string GetFormattedText(DependencyObject element)
        {
            return (string)element.GetValue(FormattedTextProperty);
        }
    }
}