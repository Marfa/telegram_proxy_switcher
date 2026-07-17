namespace TelegramLauncher.Models;

public sealed class ProxyCandidate
{
    public required string Server { get; init; }

    public required int Port { get; init; }

    public required string Secret { get; init; }

    public int LatencyMs { get; set; }

    public string TgLink =>
        $"tg://proxy?server={Uri.EscapeDataString(Server)}&port={Port}&secret={Uri.EscapeDataString(Secret)}";
}
