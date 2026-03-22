using NotepadDemo.Models;
using NotepadDemo.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NotepadDemo.Views
{
    public partial class NotepadWindow : Window
    {
        private readonly EditorViewModel _viewModel;

        public NotepadWindow()
        {
            InitializeComponent();
            _viewModel = (EditorViewModel)Resources["EditorVM"];
            DataContext = _viewModel;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _viewModel.SaveSession();
        }

        private void fileTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (fileTree.SelectedItem is FileSystemItem item)
                _viewModel.FileExplorer.OnItemDoubleClick(item);
        }

        private void fileTree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = e.OriginalSource as DependencyObject;
            while (item != null && item is not TreeViewItem)
                item = VisualTreeHelper.GetParent(item);

            if (item is TreeViewItem treeViewItem)
            {
                treeViewItem.IsSelected = true;
                treeViewItem.Focus();
            }
        }

        private void fileTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _viewModel.FileExplorer.SelectedItem = e.NewValue as FileSystemItem;
        }
    }
}