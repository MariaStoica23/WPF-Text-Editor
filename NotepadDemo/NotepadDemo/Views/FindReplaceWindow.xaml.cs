using NotepadDemo.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace NotepadDemo.Views
{
    public partial class FindReplaceWindow : Window
    {
        private EditorViewModel ViewModel => DataContext as EditorViewModel;

        public FindReplaceWindow(EditorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public void OpenOnFind()
            => tabControl.SelectedIndex = 0;

        public void OpenOnReplace()
            => tabControl.SelectedIndex = 1;

        private void Close_Click(object sender, RoutedEventArgs e)
            => HideAndClear();

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            HideAndClear();
        }

        private void HideAndClear()
        {
            ViewModel?.Search.ClearStatus();
            Hide();
        }
    }
}