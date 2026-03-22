using NotepadDemo.Models;
using NotepadDemo.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace NotepadDemo.ViewModels
{
    public class FileExplorerViewModel : BaseViewModel
    {
        private readonly FileService _fileService;
        private readonly DialogService _dialogService;
        private readonly TabsViewModel _tabsViewModel;

        private FileSystemItem _selectedItem;
        private string _copiedFolderPath;

        
        public ObservableCollection<FileSystemItem> Roots { get; }

        public FileSystemItem SelectedItem
        {
            get => _selectedItem;
            set 
            { 
                _selectedItem = value; 
                NotifyPropertyChanged(); 
            }
        }

        
        #region Commands

        public ICommand ContextMenuNewFileCommand { get; private set; }
        public ICommand ContextMenuCopyPathCommand { get; private set; }
        public ICommand ContextMenuCopyFolderCommand { get; private set; }
        public ICommand ContextMenuPasteFolderCommand { get; private set; }

        #endregion

        #region Constructor

        public FileExplorerViewModel(FileService fileService, DialogService dialogService, TabsViewModel tabsViewModel)
        {
            _fileService = fileService;
            _dialogService = dialogService;
            _tabsViewModel = tabsViewModel;
            
            Roots = new ObservableCollection<FileSystemItem>();

            InitializeCommands();
            InitializeRoots();
        }

        #endregion

        #region Initialization

        private void InitializeRoots()
        {
            foreach (var drive in DriveInfo.GetDrives())
                Roots.Add(new FileSystemItem(drive.Name));
        }

        private void InitializeCommands()
        {
            ContextMenuNewFileCommand = new RelayCommand(
                _ => ContextMenuNewFile(),
                _ => SelectedItem?.IsDirectory == true);

            ContextMenuCopyPathCommand = new RelayCommand(
                _ => ContextMenuCopyPath(),
                _ => SelectedItem != null);

            ContextMenuCopyFolderCommand = new RelayCommand(
                _ => ContextMenuCopyFolder(),
                _ => SelectedItem?.IsDirectory == true);

            ContextMenuPasteFolderCommand = new RelayCommand(
                _ => ContextMenuPasteFolder(),
                _ => _copiedFolderPath != null && SelectedItem?.IsDirectory == true);
        }

        #endregion

        #region Public Methods

        public void OnItemDoubleClick(FileSystemItem item)
        {
            if (item == null || item.IsDirectory) 
                return;

            _tabsViewModel.OpenFileAtPath(item.FullPath);
        }

        public void RefreshFolderContaining(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) 
                return;

            var folderPath = Path.GetDirectoryName(filePath);
            if (folderPath == null) 
                return;

            var folderItem = FindItemByPath(Roots, folderPath);
            folderItem?.Refresh();
        }

        #endregion

        #region Private Methods

        private void ContextMenuNewFile()
        {
            if (SelectedItem == null || !SelectedItem.IsDirectory) 
                return;

            var fileName = _dialogService.ShowInputDialog(title: "New File", prompt: "Enter file name:");

            if (fileName == null) 
                return;

            try
            {
                var filePath = _fileService.CreateFileInFolder(SelectedItem.FullPath, fileName);

                if (filePath == null)
                {
                    MessageBox.Show($"File '{fileName}' already exists.", "Warning", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SelectedItem.Refresh();
                _tabsViewModel.OpenFileAtPath(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not create file:\n{ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ContextMenuCopyPath()
        {
            if (SelectedItem == null) 
                return;

            Clipboard.SetText(SelectedItem.FullPath);
        }

        private void ContextMenuCopyFolder()
        {
            if (SelectedItem == null || !SelectedItem.IsDirectory) 
                return;

            _copiedFolderPath = SelectedItem.FullPath;
        }

        private void ContextMenuPasteFolder()
        {
            if (_copiedFolderPath == null || SelectedItem == null) 
                return;

            var folderName = Path.GetFileName(_copiedFolderPath);
            var destinationPath = Path.Combine(SelectedItem.FullPath, folderName);

            if (Directory.Exists(destinationPath))
            {
                var confirm = MessageBox.Show($"Folder '{folderName}' already exists.\nOverwrite?", "Confirm Overwrite", 
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (confirm == MessageBoxResult.No) 
                    return;
            }

            try
            {
                _fileService.CopyFolder(_copiedFolderPath, destinationPath);
                SelectedItem.Refresh();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Invalid Operation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not paste folder:\n{ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FileSystemItem FindItemByPath(IEnumerable<FileSystemItem> items, string path)
        {
            foreach (var item in items)
            {
                if (item == null) continue;

                if (string.Equals(item.FullPath, path, StringComparison.OrdinalIgnoreCase))
                    return item;

                if (item.IsExpanded)
                {
                    var found = FindItemByPath(item.Items, path);
                    if (found != null) 
                        return found;
                }
            }
            return null;
        }

        #endregion
    }
}