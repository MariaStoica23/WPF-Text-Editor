using NotepadDemo.Models;
using System.Windows;
using System.Windows.Input;

namespace NotepadDemo.ViewModels
{
    public class SearchViewModel : BaseViewModel
    {
        private readonly TabsViewModel _tabsViewModel;

        private string _searchText;
        private string _replaceText;
        private bool _searchInAllTabs;
        private bool _matchCase;
        private bool _pendingReplace;
        private string _findReplaceTitle = "Find";
        private string _statusMessage;
        private string _replaceStatusMessage;
        private int _foundIndex = -1;
        private int _foundLength;

        private List<(EditorTab Tab, int Index)> _cachedMatches = new();
        private int _currentMatchIndex = -1;

      
        public string SearchText
        {
            get => _searchText;
            set 
            { 
                _searchText = value; 
                NotifyPropertyChanged(); 
                InvalidateCache(); 
            }
        }

        public string ReplaceText
        {
            get => _replaceText;
            set 
            { 
                _replaceText = value; 
                NotifyPropertyChanged(); 
            }
        }

        public bool SearchInAllTabs
        {
            get => _searchInAllTabs;
            set 
            { 
                _searchInAllTabs = value; 
                NotifyPropertyChanged(); 
                InvalidateCache(); 
            }
        }

        public bool MatchCase
        {
            get => _matchCase;
            set 
            { 
                _matchCase = value; 
                NotifyPropertyChanged(); 
                InvalidateCache(); 
            }
        }

        public string FindReplaceTitle
        {
            get => _findReplaceTitle;
            set 
            { 
                _findReplaceTitle = value; 
                NotifyPropertyChanged(); 
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set 
            { 
                _statusMessage = value; 
                NotifyPropertyChanged(); 
            }
        }

        public string ReplaceStatusMessage
        {
            get => _replaceStatusMessage;
            private set 
            { 
                _replaceStatusMessage = value; 
                NotifyPropertyChanged(); 
            }
        }

        public int FoundIndex
        {
            get => _foundIndex;
            private set 
            { 
                _foundIndex = value; 
                NotifyPropertyChanged(); 
            }
        }

        public int FoundLength
        {
            get => _foundLength;
            private set 
            { 
                _foundLength = value; 
                NotifyPropertyChanged(); 
            }
        }

        private StringComparison Comparison
            => MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;


        #region Commands

        public ICommand FindCommand { get; private set; }
        public ICommand ReplaceCommand { get; private set; }
        public ICommand ReplaceAllCommand { get; private set; }

        #endregion

        #region Constructor

        public SearchViewModel(TabsViewModel tabsViewModel)
        {
            _tabsViewModel = tabsViewModel;

            _tabsViewModel.Tabs.CollectionChanged += (s, e) => InvalidateCache();

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            FindCommand = new RelayCommand(
                _ => Find(),
                _ => !string.IsNullOrEmpty(SearchText) && HasTabs());

            ReplaceCommand = new RelayCommand(
                _ => Replace(),
                _ => !string.IsNullOrEmpty(SearchText) && HasTabs());

            ReplaceAllCommand = new RelayCommand(
                _ => ReplaceAll(),
                _ => !string.IsNullOrEmpty(SearchText) && HasTabs());
        }

        #endregion

        #region Public Methods

        public void ClearStatus()
        {
            StatusMessage = string.Empty;
            ReplaceStatusMessage = string.Empty;
            InvalidateCache();
        }

        #endregion

        #region Private Methods — Search

        private void Find()
        {
            if (string.IsNullOrEmpty(SearchText)) 
                return;

            if (!_cachedMatches.Any())
            {
                RefreshCache();

                if (_cachedMatches.Any())
                {
                    var startIndex = _cachedMatches.FindIndex(m => m.Tab == _tabsViewModel.SelectedTab);
                    _currentMatchIndex = startIndex >= 0 ? startIndex - 1 : -1;
                }
            }

            if (_cachedMatches.Count == 0)
            {
                FoundIndex = -1;
                StatusMessage = "No results found";
                MessageBox.Show($"'{SearchText}' not found.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _currentMatchIndex = (_currentMatchIndex + 1) % _cachedMatches.Count;

            var wrapped = _currentMatchIndex == 0 && _cachedMatches.Count > 1;
            var (tab, index) = _cachedMatches[_currentMatchIndex];

            SetFoundResult(tab, index);
            StatusMessage = wrapped
                ? $"Match {_currentMatchIndex + 1} of {_cachedMatches.Count} (wrapped)"
                : $"Match {_currentMatchIndex + 1} of {_cachedMatches.Count}";
        }

        private void Replace()
        {
            if (string.IsNullOrEmpty(SearchText)) 
                return;
            if (!HasTabs()) 
                return;

            if (_pendingReplace && FoundIndex >= 0)
            {
                var currentTab = _tabsViewModel.SelectedTab;
                if (currentTab?.Content != null && IsMatchAtCurrentPosition(currentTab))
                {
                    PerformReplace(currentTab);
                    InvalidateCache();
                    RefreshCache();

                    if (_cachedMatches.Count == 0)
                    {
                        FoundIndex = -1;
                        ReplaceStatusMessage = "One occurrence replaced. No other occurrence found.";
                        return;
                    }

                    var tabs = GetTargetTabs().ToList();
                    var currentTabIndex = tabs.IndexOf(_tabsViewModel.SelectedTab);
                    if (currentTabIndex < 0) currentTabIndex = 0;

                    var orderedTabs = tabs
                        .Skip(currentTabIndex)
                        .Concat(tabs.Take(currentTabIndex))
                        .ToList();

                    foreach (var t in orderedTabs)
                    {
                        var matchIndex = _cachedMatches.FindIndex(m => m.Tab == t);
                        if (matchIndex >= 0)
                        {
                            _currentMatchIndex = matchIndex;
                            var (tab, index) = _cachedMatches[_currentMatchIndex];
                            SetFoundResult(tab, index);
                            _pendingReplace = true;
                            ReplaceStatusMessage = "One occurrence replaced. Next occurrence found.";
                            return;
                        }
                    }

                    FoundIndex = -1;
                    ReplaceStatusMessage = "One occurrence replaced. No other occurrence found.";
                    return;
                }
            }

            if (!_cachedMatches.Any())
                RefreshCache();

            if (_cachedMatches.Count == 0)
            {
                FoundIndex = -1;
                _pendingReplace = false;
                ReplaceStatusMessage = "No results found.";
                return;
            }

            var startIndex = _cachedMatches.FindIndex(m => m.Tab == _tabsViewModel.SelectedTab);
            _currentMatchIndex = (startIndex >= 0) ? startIndex : 0;

            var match = _cachedMatches[_currentMatchIndex];
            SetFoundResult(match.Tab, match.Index);
            _pendingReplace = true;
            ReplaceStatusMessage = "Occurrence found - press Replace to confirm.";
        }

        private void ReplaceAll()
        {
            if (string.IsNullOrEmpty(SearchText)) 
                return;

            var tabs = GetTargetTabs().ToList();
            if (tabs.Count == 0) 
                return;

            if (!_cachedMatches.Any())
                RefreshCache();

            var totalReplacements = _cachedMatches.Count;

            if (totalReplacements == 0)
            {
                MessageBox.Show($"'{SearchText}' not found.", "Replace All", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var tab in tabs)
            {
                if (string.IsNullOrEmpty(tab.Content)) 
                    continue;

                tab.Content = tab.Content.Replace(SearchText, ReplaceText ?? string.Empty, Comparison);
            }

            InvalidateCache();

            MessageBox.Show($"Replaced {totalReplacements} occurrence(s).", "Replace All", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void InvalidateCache()
        {
            _cachedMatches.Clear();
            _currentMatchIndex = -1;
            _pendingReplace = false;
            FoundIndex = -1;
            StatusMessage = string.Empty;
            ReplaceStatusMessage = string.Empty;
        }

        private void RefreshCache()
        {
            _cachedMatches = GetAllMatches();
            _currentMatchIndex = -1;
        }

        #endregion

        #region Private Methods — Helpers

        private bool HasTabs() => _tabsViewModel.Tabs.Count > 0;

        private IEnumerable<EditorTab> GetTargetTabs()
        {
            if (SearchInAllTabs)
                return _tabsViewModel.Tabs;

            if (_tabsViewModel.SelectedTab != null)
                return new[] { _tabsViewModel.SelectedTab };

            return Enumerable.Empty<EditorTab>();
        }

        private List<(EditorTab Tab, int Index)> GetAllMatches()
        {
            var results = new List<(EditorTab, int)>();
            if (string.IsNullOrEmpty(SearchText)) 
                return results;

            foreach (var tab in GetTargetTabs())
            {
                if (string.IsNullOrEmpty(tab.Content)) continue;
                int i = 0;
                while ((i = tab.Content.IndexOf(SearchText, i, Comparison)) >= 0)
                {
                    results.Add((tab, i));
                    i += SearchText.Length;
                }
            }
            return results;
        }

        private void SetFoundResult(EditorTab tab, int index)
        {
            _tabsViewModel.SelectedTab = tab;
            FoundIndex = index;
            FoundLength = SearchText.Length;
        }

        private bool IsMatchAtCurrentPosition(EditorTab tab)
        {
            if (FoundIndex + SearchText.Length > tab.Content.Length) 
                return false;

            var textAtIndex = tab.Content.Substring(FoundIndex, SearchText.Length);
            return string.Equals(textAtIndex, SearchText, Comparison);
        }

        private void PerformReplace(EditorTab tab)
        {
            tab.Content = tab.Content
                .Remove(FoundIndex, SearchText.Length)
                .Insert(FoundIndex, ReplaceText ?? string.Empty);
        }

        #endregion
    }
}