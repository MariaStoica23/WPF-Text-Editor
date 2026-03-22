using NotepadDemo.ViewModels;
using NotepadDemo.Views;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace NotepadDemo.Behaviors
{
    public static class TextBoxHighlightBehavior
    {
        public static readonly DependencyProperty SearchViewModelProperty =
            DependencyProperty.RegisterAttached(
                "SearchViewModel",
                typeof(SearchViewModel),
                typeof(TextBoxHighlightBehavior),
                new PropertyMetadata(null, OnSearchViewModelChanged));

        public static void SetSearchViewModel(TextBox element, SearchViewModel value)
            => element.SetValue(SearchViewModelProperty, value);

        public static SearchViewModel GetSearchViewModel(TextBox element)
            => (SearchViewModel)element.GetValue(SearchViewModelProperty);

        private static void OnSearchViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox textBox)
                return;

            if (e.OldValue is SearchViewModel oldVm)
                oldVm.PropertyChanged -= MakeHandler(textBox);

            if (e.NewValue is SearchViewModel newVm)
                newVm.PropertyChanged += MakeHandler(textBox);
        }

        private static PropertyChangedEventHandler MakeHandler(TextBox textBox)
        {
            return (sender, args) =>
            {
                if (args.PropertyName != nameof(SearchViewModel.FoundIndex)) return;
                if (sender is not SearchViewModel vm) return;
                if (vm.FoundIndex < 0) return;

                textBox.Dispatcher.BeginInvoke(() =>
                {
                    if (vm.FoundIndex + vm.FoundLength > textBox.Text.Length) 
                        return;

                    var focusedWindow = Application.Current.Windows
                        .OfType<Window>()
                        .FirstOrDefault(w => w.IsActive);

                    textBox.Focus();
                    textBox.Select(vm.FoundIndex, vm.FoundLength);
                    textBox.ScrollToLine(
                        textBox.GetLineIndexFromCharacterIndex(vm.FoundIndex));

                    focusedWindow?.Focus();
                });
            };
        }
    }
}