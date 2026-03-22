using NotepadDemo.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace NotepadDemo.Behaviors
{
    public static class TabControlTitleBehavior
    {
        public static readonly DependencyProperty TitlesProperty =
            DependencyProperty.RegisterAttached(
                "Titles",
                typeof(string),
                typeof(TabControlTitleBehavior),
                new PropertyMetadata(null, OnTitlesChanged));

        public static void SetTitles(TabControl element, string value)
            => element.SetValue(TitlesProperty, value);

        public static string GetTitles(TabControl element)
            => (string)element.GetValue(TitlesProperty);

        private static void OnTitlesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TabControl tabControl) 
                return;

            tabControl.SelectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not TabControl tabControl) 
                return;
            if (tabControl.DataContext is not EditorViewModel vm) 
                return;

            vm.Search.FindReplaceTitle = tabControl.SelectedIndex == 0 ? "Find" : "Replace";
        }
    }
}