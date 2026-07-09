using System.Diagnostics;
using System.Net.Sockets;
using System.Collections.Concurrent;
using TelegramLauncher.Models;

namespace TelegramLauncher.Services;

public sealed class ProxyProbeService
{
    public async Task<ProxyCandidate?> FindBestWorkingAsync(
        IReadOnlyList<ProxyCandidate> candidates,
        Action<string> progress,
        CancellationToken cancellationToken)
    {
        var maxParallel = Math.Min(20, Math.Max(8, Environment.ProcessorCount * 2));
        using var hardStop = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        using var fullCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, hardStop.Token);
        var queue = new ConcurrentQueue<ProxyCandidate>(candidates);
        var workers = new List<Task<ProxyCandidate?>>(maxParallel);
        var checkedCount = 0;
        const int wantedCandidates = 6;

        for (var i = 0; i < maxParallel; i++)
        {
            workers.Add(Task.Run(async () =>
            {
                while (!fullCts.IsCancellationRequested && queue.TryDequeue(out var candidate))
                {
                    try
                    {
                        var current = Interlocked.Increment(ref checkedCount);
                        progress($"Проверка {current}/{candidates.Count}");
                        var checkedProxy = await ProbeWithHardTimeoutAsync(candidate, fullCts.Token);
                        if (checkedProxy is not null)
                        {
                            return checkedProxy;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                return null;
            }, CancellationToken.None));
        }

        var matches = new List<ProxyCandidate>();
        while (workers.Count > 0)
        {
            var finished = await Task.WhenAny(workers);
            workers.Remove(finished);
            var result = await finished;
            if (result is not null)
            {
                matches.Add(result);
                if (matches.Count >= wantedCandidates)
                {
                    break;
                }
            }
        }

        return matches
            .OrderBy(GetPriorityBucket)
            .ThenBy(p => p.LatencyMs)
            .FirstOrDefault();
    }

    private static async Task<ProxyCandidate?> ProbeWithHardTimeoutAsync(ProxyCandidate candidate, CancellationToken cancellationToken)
    {
        var probeTask = ProbeCandidateAsync(candidate, cancellationToken);
        var timeoutTask = Task.Delay(2_800, cancellationToken);
        var finished = await Task.WhenAny(probeTask, timeoutTask);
        return finished == probeTask ? await probeTask : null;
    }

    private static async Task<ProxyCandidate?> ProbeCandidateAsync(ProxyCandidate candidate, CancellationToken cancellationToken)
    {
        var firstTry = await CheckTcpAsync(candidate.Server, candidate.Port, 1_200, cancellationToken);
        if (firstTry <= 0)
        {
            return null;
        }

        var secondTry = await CheckTcpAsync(candidate.Server, candidate.Port, 1_200, cancellationToken);
        if (secondTry <= 0)
        {
            return null;
        }

        candidate.LatencyMs = Math.Min(firstTry, secondTry);
        return candidate;
    }

    private static async Task<int> CheckTcpAsync(string server, int port, int timeoutMs, CancellationToken cancellationToken)
    {
        using var tcpClient = new TcpClient();
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeoutMs);

        try
        {
            var watch = Stopwatch.StartNew();
            await tcpClient.ConnectAsync(server, port, timeoutCts.Token);
            watch.Stop();
            return (int)watch.ElapsedMilliseconds;
        }
        catch
        {
            return -1;
        }
    }

    private static int GetPriorityBucket(ProxyCandidate proxy)
    {
        var isFakeTls = proxy.Secret.StartsWith("ee", StringComparison.OrdinalIgnoreCase);
        var isPort443 = proxy.Port == 443;

        if (isFakeTls && isPort443)
        {
            return 0;
        }

        if (isFakeTls)
        {
            return 1;
        }

        if (isPort443)
        {
            return 2;
        }

        return 3;
    }
}
