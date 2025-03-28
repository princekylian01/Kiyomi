namespace Kiyomi.Updater
{
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
