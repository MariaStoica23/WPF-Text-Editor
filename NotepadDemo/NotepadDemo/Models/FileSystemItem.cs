using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace NotepadDemo.Models
{
    public class FileSystemItem : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private bool _isSelected;
        private bool _childrenLoaded;

        public string Name { get; }
        public string FullPath { get; }
        public bool IsDirectory { get; }
        public ObservableCollection<FileSystemItem> Items { get; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value)
                    return;

                _isExpanded = value;
                NotifyPropertyChanged();

                if (value)
                    LoadChildren();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;

                _isSelected = value;
                NotifyPropertyChanged();
            }
        }


        public FileSystemItem(string path)
        {
            FullPath = path;
            Name = string.IsNullOrEmpty(Path.GetFileName(path))
                ? path
                : Path.GetFileName(path);
            IsDirectory = Directory.Exists(path);
            Items = new ObservableCollection<FileSystemItem>();

            if (IsDirectory)
                Items.Add(null);
        }


        #region Public Methods

        public void Refresh()
        {
            if (!IsDirectory) 
                return;

            Items.Clear();
            _childrenLoaded = false;

            if (IsExpanded)
                LoadChildren();
            else
                Items.Add(null);
        }

        #endregion

        #region Private Methods

        private void LoadChildren()
        {
            if (!IsDirectory) 
                return;

            if (_childrenLoaded) 
                return;

            Items.Clear();
            _childrenLoaded = true;
            LoadChildrenFromDisk();

            if (Items.Count == 0)
                Items.Add(null);
        }

        private void LoadChildrenFromDisk()
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(FullPath).OrderBy(d => d))
                    Items.Add(new FileSystemItem(dir));

                foreach (var file in Directory.GetFiles(FullPath).OrderBy(f => f))
                    Items.Add(new FileSystemItem(file));
            }
            catch (UnauthorizedAccessException) { }
            catch (IOException) { }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}