using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using TelegramLauncher.Models;

namespace TelegramLauncher.Services;

public sealed class ProxySourceService
{
    private static readonly SourceDefinition[] Sources =
    {
        new("kort-ru", "https://raw.githubusercontent.com/kort0881/telegram-proxy-collector/main/proxy_ru.txt", 1, 7),
        new("kort-eu", "https://raw.githubusercontent.com/kort0881/telegram-proxy-collector/main/proxy_eu.txt", 1, 7),
        new("kort-all", "https://raw.githubusercontent.com/kort0881/telegram-proxy-collector/main/proxy_all.txt", 1, 7),
        new("solispirit", "https://raw.githubusercontent.com/SoliSpirit/mtproto/master/all_proxies.txt", 1, 8),
        new("mtpro-api", "https://mtpro.xyz/api/?type=mtproto", 1, 8),
        new("mtpro-api-ru", "https://mtpro.xyz/api/?type=mtproto-ru", 1, 8),
        new("proxygenerator-telegram", "https://raw.githubusercontent.com/proxygenerator1/ProxyGenerator/main/telegramProxys.txt", 1, 8),
        new("proxysu-mtproto", "https://raw.githubusercontent.com/hookzof/socks5_list/master/tg/mtproto.txt", 2, 9),
        new("freedom-guard", "https://raw.githubusercontent.com/Freedom-Guard/Proxy/main/proxies/mtproto.txt", 2, 9),
        new("securemanager", "https://raw.githubusercontent.com/securemanager/MTPROTO/main/proxies.txt", 2, 9),
        new("surfboard", "https://raw.githubusercontent.com/Surfboardv2ray/TGProto/refs/heads/main/proxies.txt", 2, 9),
        new("mtp4tg", "https://raw.githubusercontent.com/klondike0x/mtp4tg-proxies/refs/heads/main/all_proxies.txt", 2, 9),
        new("v2ray-no1", "https://raw.githubusercontent.com/V2RAYCONFIGSPOOL/TELEGRAM_PROXY_SUB/refs/heads/main/telegram_proxy_no1.txt", 3, 10),
        new("v2ray-no2", "https://raw.githubusercontent.com/V2RAYCONFIGSPOOL/TELEGRAM_PROXY_SUB/refs/heads/main/telegram_proxy_no2.txt", 3, 10),
        new("v2ray-no3", "https://raw.githubusercontent.com/V2RAYCONFIGSPOOL/TELEGRAM_PROXY_SUB/refs/heads/main/telegram_proxy_no3.txt", 3, 10),
        new("v2ray-no4", "https://raw.githubusercontent.com/V2RAYCONFIGSPOOL/TELEGRAM_PROXY_SUB/refs/heads/main/telegram_proxy_no4.txt", 3, 10),
        new("v2ray-no5", "https://raw.githubusercontent.com/V2RAYCONFIGSPOOL/TELEGRAM_PROXY_SUB/refs/heads/main/telegram_proxy_no5.txt", 3, 10),
        new("v2ray-no6", "https://raw.githubusercontent.com/V2RAYCONFIGSPOOL/TELEGRAM_PROXY_SUB/refs/heads/main/telegram_proxy_no6.txt", 3, 10),
        new("v2ray-no7", "https://raw.githubusercontent.com/V2RAYCONFIGSPOOL/TELEGRAM_PROXY_SUB/refs/heads/main/telegram_proxy_no7.txt", 3, 10),
        new("v2ray-no8", "https://raw.githubusercontent.com/V2RAYCONFIGSPOOL/TELEGRAM_PROXY_SUB/refs/heads/main/telegram_proxy_no8.txt", 3, 10),
        new("v2ray-no9", "https://raw.githubusercontent.com/V2RAYCONFIGSPOOL/TELEGRAM_PROXY_SUB/refs/heads/main/telegram_proxy_no9.txt", 3, 10),
        new("v2ray-no10", "https://raw.githubusercontent.com/V2RAYCONFIGSPOOL/TELEGRAM_PROXY_SUB/refs/heads/main/telegram_proxy_no10.txt", 3, 10),
        new("therealwh-all", "https://raw.githubusercontent.com/Therealwh/MTPproxyLIST/refs/heads/main/verified/proxy_all_verified.txt", 3, 10),
        new("therealwh-tme", "https://raw.githubusercontent.com/Therealwh/MTPproxyLIST/refs/heads/main/verified/proxy_all_tme_verified.txt", 3, 10)
    };

    private static readonly Regex TgProxyRegex = new(
        @"(?:tg://proxy\?|https://t\.me/proxy\?)server=([^&\s]+)&port=(\d+)&secret=([A-Za-z0-9_\-=+%]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex HostPortSecretRegex = new(
        @"([A-Za-z0-9\.-]+):(\d+):([A-Fa-f0-9]{16,256})",
        RegexOptions.Compiled
    );

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = Timeout.InfiniteTimeSpan
    };

    static ProxySourceService()
    {
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TelegramLauncher/1.0");
    }

    public async Task<List<ProxyCandidate>> LoadCandidatesAsync(Action<string>? progress, CancellationToken cancellationToken)
    {
        var unique = new Dictionary<string, ProxyCandidate>(StringComparer.OrdinalIgnoreCase);
        var orderedSources = Sources
            .OrderBy(s => s.Priority)
            .ThenBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        for (var i = 0; i < orderedSources.Length; i++)
        {
            var source = orderedSources[i];
            progress?.Invoke($"Источник {i + 1}/{orderedSources.Length}");
            var body = await LoadSourceBodyOrEmptyAsync(source, cancellationToken);
            if (string.IsNullOrWhiteSpace(body))
            {
                continue;
            }

            AddParsedCandidates(body, unique);
        }

        return unique.Values.ToList();
    }

    private static void AddParsedCandidates(string body, IDictionary<string, ProxyCandidate> unique)
    {
        foreach (Match match in TgProxyRegex.Matches(body))
        {
            if (!match.Success)
            {
                continue;
            }

            AddCandidate(unique, match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
        }

        foreach (Match match in HostPortSecretRegex.Matches(body))
        {
            if (!match.Success)
            {
                continue;
            }

            AddCandidate(unique, match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
        }
    }

    private static void AddCandidate(IDictionary<string, ProxyCandidate> unique, string serverRaw, string portRaw, string secretRaw)
    {
        var server = Uri.UnescapeDataString(serverRaw).Trim();
        var secret = Uri.UnescapeDataString(secretRaw).Trim();

        if (!IsValidCandidate(server, portRaw, secret))
        {
            return;
        }

        var port = int.Parse(portRaw);
        var key = $"{server}:{port}:{secret}";
        if (unique.ContainsKey(key))
        {
            return;
        }

        unique[key] = new ProxyCandidate
        {
            Server = server,
            Port = port,
            Secret = secret
        };
    }

    private static async Task<string> LoadSourceBodyOrEmptyAsync(SourceDefinition source, CancellationToken cancellationToken)
    {
        foreach (var endpoint in ExpandEndpoints(source.Url))
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(source.TimeoutSeconds));

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                using var response = await HttpClient.SendAsync(request, timeoutCts.Token);
                if (!response.IsSuccessStatusCode)
                {
                    continue;
                }

                var body = await response.Content.ReadAsStringAsync(timeoutCts.Token);
                if (!string.IsNullOrWhiteSpace(body))
                {
                    return body;
                }
            }
            catch
            {
                // noop: fallback to next endpoint mirror
            }
        }

        return string.Empty;
    }

    private static bool IsValidCandidate(string server, string portRaw, string secret)
    {
        if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(secret))
        {
            return false;
        }

        if (server.Length > 255 || secret.Length < 16 || secret.Length > 256)
        {
            return false;
        }

        if (server.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (IPAddress.TryParse(server, out var ipAddress) && !IsPublicAddress(ipAddress))
        {
            return false;
        }

        if (!int.TryParse(portRaw, out var port))
        {
            return false;
        }

        return port is >= 1 and <= 65535;
    }

    private static bool IsPublicAddress(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
        {
            return false;
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6 && address.IsIPv6LinkLocal)
        {
            return false;
        }

        var bytes = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            return bytes[0] switch
            {
                0 => false,
                10 => false,
                127 => false,
                169 when bytes[1] == 254 => false,
                172 when bytes[1] is >= 16 and <= 31 => false,
                192 when bytes[1] == 168 => false,
                _ => true
            };
        }

        return address.AddressFamily == AddressFamily.InterNetworkV6
            && !address.IsIPv6Multicast
            && !address.IsIPv6SiteLocal
            && !address.IsIPv6UniqueLocal
            && !address.IsIPv6LinkLocal
            && !address.IsIPv6Teredo;
    }

    private sealed record SourceDefinition(string Name, string Url, int Priority, int TimeoutSeconds);

    private static IEnumerable<string> ExpandEndpoints(string primaryUrl)
    {
        yield return primaryUrl;

        const string rawHost = "https://raw.githubusercontent.com/";
        if (!primaryUrl.StartsWith(rawHost, StringComparison.OrdinalIgnoreCase))
        {
            yield break;
        }

        var rest = primaryUrl[rawHost.Length..];
        var slashMain = rest.IndexOf("/main/", StringComparison.OrdinalIgnoreCase);
        var slashRefs = rest.IndexOf("/refs/heads/", StringComparison.OrdinalIgnoreCase);

        if (slashMain > 0)
        {
            var repo = rest[..slashMain];
            var file = rest[(slashMain + "/main/".Length)..];
            yield return $"https://cdn.jsdelivr.net/gh/{repo}@main/{file}";
            yield break;
        }

        if (slashRefs > 0)
        {
            var repo = rest[..slashRefs];
            var branchAndFile = rest[(slashRefs + "/refs/heads/".Length)..];
            var slash = branchAndFile.IndexOf('/');
            if (slash <= 0)
            {
                yield break;
            }

            var branch = branchAndFile[..slash];
            var file = branchAndFile[(slash + 1)..];
            yield return $"https://cdn.jsdelivr.net/gh/{repo}@{branch}/{file}";
        }
    }
}
