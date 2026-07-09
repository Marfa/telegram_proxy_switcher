namespace TelegramLauncher;

public static class AppMetadata
{
    public const string DisplayName = "TG Proxy";
    public const string LicenseName = "CC BY-NC-SA 4.0";

    // Set repository coordinates before publishing updates.
    public const string GitHubOwner = "Marfa";
    public const string GitHubRepository = "telegram_proxy_switcher";

    public static string ReleasesUrl =>
        string.IsNullOrWhiteSpace(GitHubOwner) || string.IsNullOrWhiteSpace(GitHubRepository)
            ? string.Empty
            : $"https://github.com/{GitHubOwner}/{GitHubRepository}/releases";
}
