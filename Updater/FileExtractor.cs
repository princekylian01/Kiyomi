using System.IO;
using System.IO.Compression;

namespace Kiyomi.Updater
{
    public static class FileExtractor
    {
        public static void ExtractZipOverwrite(string zipPath, string destinationFolder)
        {
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    // Папка, в которую извлекаем
                    string filePath = Path.Combine(destinationFolder, entry.FullName);

                    // Создаём подпапки при необходимости
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);

                    // Если это «пустая» директория (Name == ""), пропускаем
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    if (File.Exists(filePath))
                        File.Delete(filePath);

                    entry.ExtractToFile(filePath);
                }
            }
        }
    }
}
