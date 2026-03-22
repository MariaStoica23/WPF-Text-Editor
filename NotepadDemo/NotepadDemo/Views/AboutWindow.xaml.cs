using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace NotepadDemo.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
            => Close();
    }
}
