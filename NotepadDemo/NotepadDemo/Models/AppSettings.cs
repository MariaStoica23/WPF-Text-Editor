namespace NotepadDemo.Models
{
    public class AppSettings
    {
        public ViewMode ViewMode { get; set; } = ViewMode.Standard;
        public List<SavedTab> OpenTabs { get; set; } = new();
        public int SelectedTabIndex { get; set; } = 0;
    }
}