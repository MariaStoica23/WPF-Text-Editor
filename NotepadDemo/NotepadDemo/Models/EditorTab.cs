using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace NotepadDemo.Models
{
    public class EditorTab : INotifyPropertyChanged
    {
        private string _title;
        private string _content;
        private string _filePath;
        private bool _isModified;

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath == value) 
                    return;

                _filePath = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Title));
                NotifyPropertyChanged(nameof(CleanTitle));
                NotifyPropertyChanged(nameof(IsNew));
            }
        }

        public string Content
        {
            get => _content;
            set
            {
                if (_content == value)
                    return;

                _content = value;
                IsModified = true;
                NotifyPropertyChanged();
            }
        }

        public bool IsModified
        {
            get => _isModified;
            set
            {
                if (_isModified == value) 
                    return;

                _isModified = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Title));
            }
        }

        public bool IsNew => FilePath == null;

        public string Title => IsModified ? $"{CleanTitle} ●" : CleanTitle;

        public string CleanTitle => IsNew ? _title : Path.GetFileName(FilePath);


        private EditorTab() { }


        #region Factory Methods

        public static EditorTab NewEmpty(int fileNumber) => new()
        {
            _title = $"File {fileNumber}",
            _content = string.Empty,
            _isModified = false
        };

        public static EditorTab FromFile(string filePath, string content) => new()
        {
            _filePath = filePath,
            _content = content,
            _isModified = false
        };

        public static EditorTab FromTitle(string title, string content) => new()
        {
            _title = title,
            _content = content,
            _isModified = !string.IsNullOrEmpty(content)
        };

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}