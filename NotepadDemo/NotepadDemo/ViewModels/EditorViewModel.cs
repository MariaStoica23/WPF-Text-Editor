using NotepadDemo.Models;
using NotepadDemo.Services;
using NotepadDemo.Views;
using System.Windows.Input;

namespace NotepadDemo.ViewModels
{
    public class EditorViewModel : BaseViewModel
    {
        private readonly FileService _fileService;
        private readonly DialogService _dialogService;
        private readonly SettingsService _settingsService;
        private readonly AppSettings _settings;

        private FindReplaceWindow _findReplaceWindow;
        private ViewMode _currentViewMode;


        public ViewMode CurrentViewMode
        {
            get => _currentViewMode;
            set 
            { 
                _currentViewMode = value; 
                NotifyPropertyChanged(); 
            }
        }

        public TabsViewModel Tabs { get; }
        public FileExplorerViewModel FileExplorer { get; }
        public SearchViewModel Search { get; }

        
        #region Commands

        public ICommand ShowFindCommand { get; private set; }
        public ICommand ShowReplaceCommand { get; private set; }
        public ICommand ShowStandardViewCommand { get; private set; }
        public ICommand ShowExplorerViewCommand { get; private set; }
        public ICommand ShowAboutCommand { get; private set; }

        #endregion

        #region Constructor

        public EditorViewModel()
        {
            _fileService = new FileService();
            _dialogService = new DialogService();
            _settingsService = new SettingsService();

            _settings = _settingsService.Load();
            CurrentViewMode = _settings.ViewMode;

            Tabs = new TabsViewModel(_fileService, _dialogService);
            FileExplorer = new FileExplorerViewModel(_fileService, _dialogService, Tabs);
            Search = new SearchViewModel(Tabs);

            Tabs.OnFileSaved = FileExplorer.RefreshFolderContaining;

            Tabs.RestoreSession(_settings, _fileService);
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            ShowFindCommand = new RelayCommand(_ => ShowFindReplace(openOnFind: true));
            ShowReplaceCommand = new RelayCommand(_ => ShowFindReplace(openOnFind: false));
            ShowStandardViewCommand = new RelayCommand(_ => SetViewMode(ViewMode.Standard));
            ShowExplorerViewCommand = new RelayCommand(_ => SetViewMode(ViewMode.FolderExplorer));
            ShowAboutCommand = new RelayCommand(_ => new AboutWindow().ShowDialog());
        }

        #endregion

        #region Public Methods

        public void SaveSession()
        {
            _settings.OpenTabs = Tabs.Tabs
                .Where(t => !t.IsNew || !string.IsNullOrEmpty(t.Content))
                .Select(t => t.IsNew
                ? new SavedTab { Title = t.CleanTitle, Content = t.Content }
                : new SavedTab { FilePath = t.FilePath, Content = t.IsModified ? t.Content : null }
            ).ToList();

            _settings.SelectedTabIndex = Tabs.Tabs.IndexOf(Tabs.SelectedTab);

            try 
            { 
                _settingsService.Save(_settings); 
            }
            catch { }
        }

        #endregion

        #region Private Methods

        private void ShowFindReplace(bool openOnFind)
        {
            if (_findReplaceWindow == null)
                _findReplaceWindow = new FindReplaceWindow(this);

            if (openOnFind)
                _findReplaceWindow.OpenOnFind();
            else
                _findReplaceWindow.OpenOnReplace();

            _findReplaceWindow.Show();
            _findReplaceWindow.Activate();
        }

        private void SetViewMode(ViewMode mode)
        {
            CurrentViewMode = mode;
            _settings.ViewMode = mode;

            try { _settingsService.Save(_settings); }
            catch { }
        }

        #endregion
    }
}