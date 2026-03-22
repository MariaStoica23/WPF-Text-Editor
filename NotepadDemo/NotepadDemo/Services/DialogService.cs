using Microsoft.Win32;
using System.Windows;
using NotepadDemo.Views;

namespace NotepadDemo.Services
{
    public class DialogService
    {
        public string ShowOpenFileDialog(string initialDirectory = null)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = ".txt",
                InitialDirectory = initialDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string ShowSaveFileDialog(string defaultFileName = null)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = ".txt",
                FileName = defaultFileName ?? string.Empty,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public bool? ShowSaveConfirmDialog(string fileName)
        {
            var result = MessageBox.Show(
                $"'{fileName}' has unsaved changes.\nDo you want to save before closing?", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

            return result switch
            {
                MessageBoxResult.Yes => true,
                MessageBoxResult.No => false,
                MessageBoxResult.Cancel => null,
                _ => null
            };
        }

        public string ShowInputDialog(string title, string prompt)
        {
            var dialog = new InputDialog(title, prompt);
            return dialog.ShowDialog() == true ? dialog.InputText : null;
        }
    }
}