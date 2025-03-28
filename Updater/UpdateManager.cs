using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Kiyomi.Updater
{
    public static class UpdateManager
    {
        private const string UpdateZipFile = "update.zip";

        /// <summary>
        /// Проверяем, есть ли более свежая версия, чем текущая (AssemblyVersion).
        /// </summary>
        public static async Task<bool> IsUpdateAvailableAsync()
        {
            Version localVersion = Assembly.GetExecutingAssembly().GetName().Version;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "KiyomiUpdater");
                string json = await client.GetStringAsync(UpdateSettings.LatestReleaseApiUrl);
                var release = JObject.Parse(json);

                // Предположим, что в release["tag_name"] лежит, например, "v0.2".
                string tagName = release["tag_name"]?.ToString() ?? "";
                if (tagName.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                {
                    tagName = tagName.Substring(1);
                }

                if (Version.TryParse(tagName, out Version gitVersion))
                {
                    return gitVersion > localVersion;
                }
                else
                {
                    // Если формат не распознан — считаем, что нет обновлений.
                    return false;
                }
            }
        }

        /// <summary>
        /// 1) Скачиваем файл update.zip
        /// 2) Бэкапим текущие файлы
        /// 3) Распаковываем поверх
        /// 4) Удаляем update.zip
        /// 5) Перезапускаем приложение
        /// </summary>
        public static async Task DownloadAndApplyUpdateAsync(
            Action<int> onProgressChanged,
            Action<bool, string> onCompleted)
        {
            try
            {
                // 1. Скачать архив
                await DownloadLatestReleaseAsync(onProgressChanged);

                // 2. Бэкап
                BackupManager.BackupDirectory(".");

                // 3. Распаковка
                FileExtractor.ExtractZipOverwrite(UpdateZipFile, ".");

                // 4. Удаляем архив
                if (File.Exists(UpdateZipFile))
                {
                    File.Delete(UpdateZipFile);
                }

                // 5. Перезапуск
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                System.Diagnostics.Process.Start(exePath);

                // Сообщаем о завершении
                onCompleted(true, "Update applied successfully.");

                // Небольшая задержка и завершаем старое приложение
                await Task.Delay(500);
                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Rollback();
                onCompleted(false, $"Ошибка при установке обновления: {ex.Message}");
            }
        }

        private static void Rollback()
        {
            try
            {
                BackupManager.RestoreBackup(".", "backup");
            }
            catch (Exception ex)
            {
                // Если даже откат не удался
                System.Windows.MessageBox.Show($"Не удалось выполнить откат: {ex.Message}");
            }
        }

        /// <summary>
        /// Скачивание файла с прогрессом
        /// </summary>
        private static async Task DownloadLatestReleaseAsync(Action<int> onProgressChanged)
        {
            string downloadUrl = UpdateSettings.DownloadUrl;

            using (HttpClient client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true }))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "KiyomiUpdater");
                using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    long? totalBytes = response.Content.Headers.ContentLength;
                    using (var downloadStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(UpdateZipFile, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        byte[] buffer = new byte[81920];
                        long totalRead = 0;
                        int bytesRead;

                        while ((bytesRead = await downloadStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalRead += bytesRead;

                            if (totalBytes.HasValue && totalBytes > 0)
                            {
                                int progress = (int)((totalRead * 100) / totalBytes.Value);
                                onProgressChanged?.Invoke(progress);
                            }
                            else
                            {
                                // Если Content-Length не задан, даём хотя бы число Kb
                                onProgressChanged?.Invoke((int)(totalRead / 1024));
                            }
                        }
                    }
                }
            }
        }
    }
}
