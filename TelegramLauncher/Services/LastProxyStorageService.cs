using System.Text.Json;
using TelegramLauncher.Models;

namespace TelegramLauncher.Services;

public sealed class LastProxyStorageService
{
    private readonly string _storagePath;

    public LastProxyStorageService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _storagePath = Path.Combine(appData, "TelegramLauncher", "last-proxy.json");
    }

    public async Task SaveAsync(ProxyCandidate proxy, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_storagePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(proxy);
        await File.WriteAllTextAsync(_storagePath, json, cancellationToken);
    }

    public async Task<ProxyCandidate?> TryLoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_storagePath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_storagePath, cancellationToken);
            var model = JsonSerializer.Deserialize<ProxyCandidate>(json);
            if (model is null || string.IsNullOrWhiteSpace(model.Server) || model.Port <= 0 || string.IsNullOrWhiteSpace(model.Secret))
            {
                return null;
            }

            return model;
        }
        catch
        {
            return null;
        }
    }
}
