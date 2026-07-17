using System.Text.Json;

namespace TelegramLauncher.Services;

public sealed class GitHubUpdateService
{
    private const string GitHubHost = "github.com";
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    static GitHubUpdateService()
    {
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TGProxy/1.1.0");
    }

    public async Task<UpdateCheckResult> CheckAsync(
        string owner,
        string repository,
        Version currentVersion,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repository))
        {
            return UpdateCheckResult.NotConfigured();
        }

        var apiUrl = $"https://api.github.com/repos/{owner}/{repository}/releases/latest";
        try
        {
            using var response = await HttpClient.GetAsync(apiUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return UpdateCheckResult.Failed($"Ошибка проверки обновлений: HTTP {(int)response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            var tag = root.TryGetProperty("tag_name", out var tagProperty)
                ? tagProperty.GetString() ?? string.Empty
                : string.Empty;
            var releaseUrl = root.TryGetProperty("html_url", out var htmlProperty)
                ? htmlProperty.GetString() ?? string.Empty
                : $"https://{GitHubHost}/{owner}/{repository}/releases";
            if (!IsTrustedReleaseUrl(releaseUrl, owner, repository))
            {
                return UpdateCheckResult.Failed("GitHub вернул недоверенную ссылку релиза.");
            }

            var latestVersion = ParseVersion(tag);
            if (latestVersion is null)
            {
                return UpdateCheckResult.Failed("Не удалось распознать версию релиза.");
            }

            if (latestVersion > currentVersion)
            {
                return UpdateCheckResult.NewVersion(tag, releaseUrl);
            }

            return UpdateCheckResult.UpToDate();
        }
        catch (Exception ex)
        {
            return UpdateCheckResult.Failed($"Ошибка проверки обновлений: {ex.Message}");
        }
    }

    private static Version? ParseVersion(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().TrimStart('v', 'V');
        return Version.TryParse(normalized, out var version) ? version : null;
    }

    public static bool IsTrustedReleaseUrl(string releaseUrl, string owner, string repository)
    {
        if (!Uri.TryCreate(releaseUrl, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!uri.Host.Equals(GitHubHost, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var expectedPrefix = $"/{owner}/{repository}/releases";
        return uri.AbsolutePath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase);
    }
}

public sealed record UpdateCheckResult(
    bool IsConfigured,
    bool IsUpToDate,
    bool HasNewVersion,
    string LatestTag,
    string ReleaseUrl,
    string Message)
{
    public static UpdateCheckResult NotConfigured() =>
        new(false, false, false, string.Empty, string.Empty, "Репозиторий обновлений не настроен.");

    public static UpdateCheckResult UpToDate() =>
        new(true, true, false, string.Empty, string.Empty, "Установлена актуальная версия.");

    public static UpdateCheckResult NewVersion(string latestTag, string releaseUrl) =>
        new(true, false, true, latestTag, releaseUrl, $"Доступна новая версия: {latestTag}");

    public static UpdateCheckResult Failed(string message) =>
        new(true, false, false, string.Empty, string.Empty, message);
}
