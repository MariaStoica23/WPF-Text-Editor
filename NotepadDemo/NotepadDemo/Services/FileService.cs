using System.IO;

namespace NotepadDemo.Services
{
    public class FileService
    {
        #region File Operations
        public string ReadFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be empty.", nameof(filePath));

            return File.ReadAllText(filePath);
        }

        public void WriteFile(string filePath, string content)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be empty.", nameof(filePath));

            File.WriteAllText(filePath, content ?? string.Empty);
        }

        public string CreateFileInFolder(string folderPath, string fileName)
        {
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("Folder path cannot be empty.", nameof(folderPath));
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be empty.", nameof(fileName));

            if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                fileName += ".txt";

            var filePath = Path.Combine(folderPath, fileName);

            if (File.Exists(filePath))
                return null;

            File.WriteAllText(filePath, string.Empty);
            return filePath;
        }

        #endregion

        #region Folder Operations

        public void CopyFolder(string sourcePath, string destinationPath)
        {
            if (string.IsNullOrEmpty(sourcePath))
                throw new ArgumentException("Source path cannot be empty.", nameof(sourcePath));
            if (string.IsNullOrEmpty(destinationPath))
                throw new ArgumentException("Destination path cannot be empty.", nameof(destinationPath));
            if (!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");

            if (sourcePath.Equals(destinationPath, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Cannot copy a folder into itself.");

            if (destinationPath.StartsWith(
                    sourcePath + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    "Cannot copy a folder into one of its subfolders.");

            CopyFolderRecursive(sourcePath, destinationPath);
        }

        private void CopyFolderRecursive(string sourcePath, string destinationPath)
        {
            Directory.CreateDirectory(destinationPath);

            foreach (var file in Directory.GetFiles(sourcePath))
            {
                var destFile = Path.Combine(destinationPath, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
            }

            foreach (var subDir in Directory.GetDirectories(sourcePath))
            {
                var destSubDir = Path.Combine(destinationPath, Path.GetFileName(subDir));
                CopyFolderRecursive(subDir, destSubDir);
            }
        }

        #endregion
    }
}