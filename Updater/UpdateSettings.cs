namespace Kiyomi.Updater
{
    /// <summary>
    /// Настройки обновлений.
    /// Чтобы не хардкодить URL и Owner/Repo, храним здесь.
    /// При желании можно брать из конфиг-файла, env-переменных и т.д.
    /// </summary>
    public static class UpdateSettings
    {
        public static string GitHubRepoOwner = "princekylian01";
        public static string GitHubRepoName = "Kiyomi";

        public static string LatestReleaseApiUrl =>
            $"https://api.github.com/repos/{GitHubRepoOwner}/{GitHubRepoName}/releases/latest";

        public static string DownloadUrl =>
            $"https://github.com/{GitHubRepoOwner}/{GitHubRepoName}/releases/latest/download/update.zip";
    }
}
