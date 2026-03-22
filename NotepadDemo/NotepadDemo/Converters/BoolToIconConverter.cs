using System.Globalization;
using System.Windows.Data;

namespace NotepadDemo.Converters
{
    public class BoolToIconConverter : IValueConverter
    {
        private const string DirectoryIcon = "📁";
        private const string FileIcon = "📄";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool isDirectory && isDirectory ? DirectoryIcon : FileIcon;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}