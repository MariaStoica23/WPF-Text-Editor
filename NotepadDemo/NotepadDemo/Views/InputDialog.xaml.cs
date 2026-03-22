using System.Windows;

namespace NotepadDemo.Views
{
    public partial class InputDialog : Window
    {
        public string InputText => inputTxt.Text;

        public InputDialog(string title, string prompt)
        {
            InitializeComponent();
            DataContext = new { Title = title, Prompt = prompt };
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(inputTxt.Text))
            {
                MessageBox.Show("Please enter a name.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
            => DialogResult = false;
    }
}