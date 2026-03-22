using NotepadDemo.Models;
using NotepadDemo.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace NotepadDemo.ViewModels
{
    public class TabsViewModel : BaseViewModel
    {
        private readonly FileService _fileService;
        private readonly DialogService _dialogService;
        private EditorTab _selectedTab;

        public ObservableCollection<EditorTab> Tabs { get; }

        public EditorTab SelectedTab
        {
            get => _selectedTab;
            set 
            { 
                _selectedTab = value; 
                NotifyPropertyChanged(); 
            }
        }

        public Action<string> OnFileSaved { get; set; }


        #region Commands

        public ICommand NewFileCommand { get; private set; }
        public ICommand OpenFileCommand { get; private set; }
        public ICommand SaveFileCommand { get; private set; }
        public ICommand SaveFileAsCommand { get; private set; }
        public ICommand CloseTabCommand { get; private set; }
        public ICommand CloseAllCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        #endregion

        #region Constructor

        public TabsViewModel(FileService fileService, DialogService dialogService)
        {
            _fileService = fileService;
            _dialogService = dialogService;
           
            Tabs = new ObservableCollection<EditorTab>();

            InitializeCommands();
            NewFile();
        }

        #endregion

        #region Initialization

        private void InitializeCommands()
        {
            NewFileCommand = new RelayCommand(_ => NewFile());
            OpenFileCommand = new RelayCommand(_ => OpenFile());
            SaveFileCommand = new RelayCommand(_ => SaveFile(), _ => SelectedTab != null);
            SaveFileAsCommand = new RelayCommand(_ => SaveFileAs(), _ => SelectedTab != null);
            CloseTabCommand = new RelayCommand(t => CloseTab(t as EditorTab), _ => SelectedTab != null);
            CloseAllCommand = new RelayCommand(_ => CloseAll(), _ => Tabs.Count > 0);
            ExitCommand = new RelayCommand(_ => Exit());
        }

        #endregion

        #region Public Methods

        public void NewFile()
        {
            var tab = EditorTab.NewEmpty(GetNextFileNumber());
            Tabs.Add(tab);
            SelectedTab = tab;
        }

        public int GetNextFileNumber()
        {
            int number = 1;
            while (Tabs.Any(t => t.CleanTitle == $"File {number}"))
                number++;
            return number;
        }

        public void OpenFileAtPath(string filePath)
        {
            var existing = Tabs.FirstOrDefault(t => t.FilePath == filePath);
            if (existing != null)
            {
                SelectedTab = existing;
                return;
            }

            try
            {
                var content = _fileService.ReadFile(filePath);
                var tab = EditorTab.FromFile(filePath, content);
                Tabs.Add(tab);
                SelectedTab = tab;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open file:\n{ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CloseAll(bool openNewTab = true)
        {
            var unsaved = Tabs.Where(t => t.IsModified).ToList();
            foreach (var tab in unsaved)
            {
                SelectedTab = tab;
                var result = _dialogService.ShowSaveConfirmDialog(tab.CleanTitle);
                if (result == true)
                {
                    if (tab.IsNew)
                    {
                        SaveFileAs();
                        if (tab.IsNew) 
                            return;
                    }
                    else
                    {
                        SaveTabToFile(tab);
                    }
                }
                else if (result == null)
                    return;
            }

            Tabs.Clear();
            SelectedTab = null;

            if (openNewTab)
                NewFile();
        }

        public void RestoreSession(AppSettings settings, FileService fileService)
        {
            if (settings.OpenTabs == null || settings.OpenTabs.Count == 0)
                return;

            Tabs.Clear();

            foreach (var saved in settings.OpenTabs)
            {
                EditorTab tab;

                if (saved.FilePath != null)
                {
                    if (!File.Exists(saved.FilePath)) continue;

                    try
                    {
                        var content = saved.Content ?? fileService.ReadFile(saved.FilePath);
                        tab = EditorTab.FromFile(saved.FilePath, content);
                        if (saved.Content != null)
                            tab.IsModified = true;
                    }
                    catch { continue; }
                }
                else
                {
                    tab = EditorTab.FromTitle(saved.Title, saved.Content ?? string.Empty);
                }

                Tabs.Add(tab);
            }

            if (Tabs.Count == 0)
            {
                NewFile();
                return;
            }

            var index = Math.Clamp(settings.SelectedTabIndex, 0, Tabs.Count - 1);
            SelectedTab = Tabs[index];
        }

        #endregion

        #region Private Methods

        private void OpenFile()
        {
            var filePath = _dialogService.ShowOpenFileDialog();
            if (filePath == null) 
                return;

            OpenFileAtPath(filePath);
        }

        private void SaveFile()
        {
            if (SelectedTab == null) 
                return;

            if (SelectedTab.IsNew) 
            { 
                SaveFileAs(); 
                return; 
            }
            
            SaveTabToFile(SelectedTab);
        }

        private void SaveFileAs()
        {
            if (SelectedTab == null) 
                return;

            var filePath = _dialogService.ShowSaveFileDialog(SelectedTab.CleanTitle);
            if (filePath == null) 
                return;

            try
            {
                _fileService.WriteFile(filePath, SelectedTab.Content);
                SelectedTab.FilePath = filePath;
                SelectedTab.IsModified = false;
                OnFileSaved?.Invoke(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save file:\n{ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SaveTabToFile(EditorTab tab)
        {
            if (tab == null) 
                return;

            try
            {
                _fileService.WriteFile(tab.FilePath, tab.Content);
                tab.IsModified = false;
                OnFileSaved?.Invoke(tab.FilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save file:\n{ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseTab(EditorTab tab)
        {
            if (tab == null) return;

            if (tab.IsModified)
            {
                var result = _dialogService.ShowSaveConfirmDialog(tab.CleanTitle);
                if (result == true)
                {
                    if (tab.IsNew)
                    {
                        SelectedTab = tab;
                        SaveFileAs();
                        if (tab.IsNew) 
                            return;
                    }
                    else
                    {
                        SaveTabToFile(tab);
                    }
                }
                else if (result == null)
                    return;
            }

            Tabs.Remove(tab);

            if (Tabs.Count == 0)
                NewFile();
            else
                SelectedTab = Tabs.LastOrDefault();
        }
        private void Exit()
            => Application.Current.Shutdown();

        #endregion
    }
}