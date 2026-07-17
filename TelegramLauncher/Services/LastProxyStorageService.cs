using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TelegramLauncher.Models;

namespace TelegramLauncher.Services;

public sealed class LastProxyStorageService
{
    private static readonly byte[] AdditionalEntropy = Encoding.UTF8.GetBytes("TelegramLauncher.LastProxy.v1");
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
        var protectedBytes = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(json),
            AdditionalEntropy,
            DataProtectionScope.CurrentUser
        );
        await File.WriteAllTextAsync(_storagePath, Convert.ToBase64String(protectedBytes), cancellationToken);
    }

    public async Task<ProxyCandidate?> TryLoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_storagePath))
        {
            return null;
        }

        try
        {
            var storedValue = await File.ReadAllTextAsync(_storagePath, cancellationToken);
            var json = TryDecrypt(storedValue) ?? storedValue;
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

    private static string? TryDecrypt(string storedValue)
    {
        if (string.IsNullOrWhiteSpace(storedValue))
        {
            return null;
        }

        try
        {
            var protectedBytes = Convert.FromBase64String(storedValue);
            var jsonBytes = ProtectedData.Unprotect(
                protectedBytes,
                AdditionalEntropy,
                DataProtectionScope.CurrentUser
            );
            return Encoding.UTF8.GetString(jsonBytes);
        }
        catch
        {
            return null;
        }
    }
}
