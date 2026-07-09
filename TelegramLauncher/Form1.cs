using System.Diagnostics;
using System.Reflection;
using TelegramLauncher.Models;
using TelegramLauncher.Services;

namespace TelegramLauncher;

public partial class Form1 : Form
{
    private const int WindowWidth = 360;
    private const int SidePadding = 16;
    private const int ContentWidth = WindowWidth - SidePadding * 2;
    private const int ContentPadding = 14;
    private const int RowGap = 8;

    private readonly TelegramWindowService _telegramWindowService = new();
    private readonly ProxySourceService _proxySourceService = new();
    private readonly ProxyProbeService _proxyProbeService = new();
    private readonly LastProxyStorageService _lastProxyStorageService = new();
    private readonly GitHubUpdateService _gitHubUpdateService = new();

    private CancellationTokenSource? _pickCts;
    private ProxyCandidate? _selectedProxy;
    private bool _startupPromptShown;
    private bool _lastTelegramForeground;
    private bool _startupUpdateChecked;

    public Form1()
    {
        InitializeComponent();
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        UpdateTelegramStateUi();
        LayoutControls();
        StickToTelegramIfExists();
        stickTimer.Start();
        _ = LoadLastProxyAsync();
        _ = CheckUpdatesOnStartupAsync();
    }

    private async void pickProxyButton_Click(object? sender, EventArgs e)
    {
        if (!_telegramWindowService.TryGetTelegramWindow(out _))
        {
            PromptStartTelegram();
            SetStatus("Запустите Telegram Desktop.");
            return;
        }

        _pickCts?.Cancel();
        _pickCts = new CancellationTokenSource();
        _selectedProxy = null;
        connectButton.Text = "Подключиться к прокси";
        connectButton.Visible = false;
        stopPickButton.Visible = true;
        detailsTextBox.Visible = false;
        detailsTextBox.Text = string.Empty;
        statusLabel.Visible = true;
        pickProxyButton.Enabled = false;
        LayoutControls();

        try
        {
            SetStatus("Загрузка источников...");
            var candidates = await _proxySourceService.LoadCandidatesAsync(SetStatus, _pickCts.Token);
            if (candidates.Count == 0)
            {
                ShowFailureDetails("Не удалось получить прокси.\nНи один источник не вернул кандидатов.");
                return;
            }

            SetStatus($"Проверка 0/{candidates.Count}");
            var proxy = await _proxyProbeService.FindBestWorkingAsync(
                candidates,
                SetStatus,
                _pickCts.Token
            );

            if (proxy is null)
            {
                ShowFailureDetails($"Рабочий прокси не найден.\nПроверено кандидатов: {candidates.Count}.");
                return;
            }

            _selectedProxy = proxy;
            await _lastProxyStorageService.SaveAsync(proxy, CancellationToken.None);
            stopPickButton.Visible = false;
            connectButton.Visible = true;
            SetStatus($"Прокси найден ({proxy.LatencyMs} мс).");
            TrySendToTelegram();
        }
        catch (OperationCanceledException)
        {
            SetStatus("Подбор остановлен.");
        }
        finally
        {
            pickProxyButton.Enabled = true;
            stopPickButton.Visible = false;
            LayoutControls();
        }
    }

    private void connectButton_Click(object? sender, EventArgs e)
    {
        if (connectButton.Text == "Продолжить подбор")
        {
            connectButton.Text = "Подключиться к прокси";
            pickProxyButton_Click(sender, e);
            return;
        }

        if (_selectedProxy is null)
        {
            return;
        }

        TrySendToTelegram();
        connectButton.Text = "Продолжить подбор";
    }

    private void TrySendToTelegram()
    {
        if (_selectedProxy is null)
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _selectedProxy.TgLink,
                UseShellExecute = true
            });
            SetStatus("Ссылка отправлена в Telegram.");
        }
        catch
        {
            ShowFailureDetails($"Не удалось открыть ссылку.\n{_selectedProxy.TgLink}");
        }
    }

    private void stickTimer_Tick(object? sender, EventArgs e)
    {
        StickToTelegramIfExists();
        SyncLauncherForegroundWithTelegram();
    }

    private void stopPickButton_Click(object? sender, EventArgs e)
    {
        _pickCts?.Cancel();
    }

    private void aboutButton_Click(object? sender, EventArgs e)
    {
        _ = ShowAboutAsync();
    }

    private void StickToTelegramIfExists()
    {
        if (!_telegramWindowService.TryGetTelegramWindow(out var tgBounds))
        {
            UpdateTelegramStateUi();
            return;
        }

        var offset = 8;
        var nextX = tgBounds.Left;
        var nextY = tgBounds.Top - Height - offset;
        if (nextY < 0)
        {
            nextY = tgBounds.Bottom + offset;
        }

        if (Location.X == nextX && Location.Y == nextY)
        {
            return;
        }

        Location = new Point(nextX, nextY);
        if (_pickCts is null || _pickCts.IsCancellationRequested)
        {
            UpdateTelegramStateUi();
        }
    }

    private void SetStatus(string status)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => SetStatus(status));
            return;
        }

        statusLabel.Text = status;
        statusLabel.Visible = true;
        LayoutControls();
    }

    private void ShowFailureDetails(string details)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => ShowFailureDetails(details));
            return;
        }

        var shortStatus = details.Split('\n')[0];
        statusLabel.Text = shortStatus;
        statusLabel.Visible = true;
        detailsTextBox.Text = details;
        detailsTextBox.Visible = true;
        connectButton.Visible = false;
        LayoutControls();
    }

    private async Task LoadLastProxyAsync()
    {
        var lastProxy = await _lastProxyStorageService.TryLoadAsync(CancellationToken.None);
        if (lastProxy is null)
        {
            return;
        }

        _selectedProxy = lastProxy;
        detailsTextBox.Visible = false;
        connectButton.Visible = false;
        statusLabel.Visible = false;
        LayoutControls();
    }

    private void UpdateTelegramStateUi()
    {
        var telegramRunning = _telegramWindowService.TryGetTelegramWindow(out _);
        if (telegramRunning)
        {
            if (pickProxyButton.Enabled && !stopPickButton.Visible && !connectButton.Visible && !detailsTextBox.Visible)
            {
                statusLabel.Visible = false;
                LayoutControls();
            }

            return;
        }

        if (!_startupPromptShown)
        {
            PromptStartTelegram();
        }

        SetStatus("Запустите Telegram Desktop.");
    }

    private void PromptStartTelegram()
    {
        _startupPromptShown = true;
        MessageBox.Show(
            this,
            "Telegram Desktop не запущен.\nПожалуйста, запустите Telegram.",
            "Нужен Telegram",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }

    private void SyncLauncherForegroundWithTelegram()
    {
        var telegramForeground = _telegramWindowService.IsTelegramForeground();
        if (telegramForeground == _lastTelegramForeground)
        {
            return;
        }

        _lastTelegramForeground = telegramForeground;
        if (!telegramForeground)
        {
            return;
        }

        Activate();
    }

    private void LayoutControls()
    {
        var y = pickProxyButton.Bottom + RowGap;

        if (statusLabel.Visible)
        {
            statusLabel.Location = new Point(SidePadding, y);
            statusLabel.Width = ContentWidth;
            statusLabel.Height = MeasureTextHeight(statusLabel.Text, statusLabel.Font, ContentWidth);
            y = statusLabel.Bottom + RowGap;
        }

        if (detailsTextBox.Visible)
        {
            detailsTextBox.Location = new Point(SidePadding, y);
            detailsTextBox.Width = ContentWidth;
            detailsTextBox.Height = Math.Max(56, MeasureTextHeight(detailsTextBox.Text, detailsTextBox.Font, ContentWidth) + 8);
            y = detailsTextBox.Bottom + RowGap;
        }

        if (stopPickButton.Visible)
        {
            stopPickButton.Location = new Point(SidePadding, y);
            stopPickButton.Width = ContentWidth;
            y = stopPickButton.Bottom + RowGap;
        }

        if (connectButton.Visible)
        {
            connectButton.Location = new Point(SidePadding, y);
            connectButton.Width = ContentWidth;
            y = connectButton.Bottom + RowGap;
        }

        var targetHeight = y + ContentPadding - RowGap;
        var targetSize = new Size(WindowWidth, Math.Max(pickProxyButton.Bottom + ContentPadding, targetHeight));
        if (ClientSize != targetSize)
        {
            ClientSize = targetSize;
        }
    }

    private static int MeasureTextHeight(string text, Font font, int maxWidth)
    {
        var size = TextRenderer.MeasureText(
            text,
            font,
            new Size(maxWidth, int.MaxValue),
            TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl
        );
        return Math.Max(20, size.Height + 4);
    }

    private async Task ShowAboutAsync()
    {
        var versionText = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
        var currentVersion = Version.TryParse(versionText, out var parsed) ? parsed : new Version(1, 0, 0);

        var update = await _gitHubUpdateService.CheckAsync(
            AppMetadata.GitHubOwner,
            AppMetadata.GitHubRepository,
            currentVersion,
            CancellationToken.None
        );

        var message =
            $"{AppMetadata.DisplayName}\n" +
            $"Версия: {versionText}\n" +
            $"Лицензия: {AppMetadata.LicenseName}\n\n" +
            $"Обновления: {update.Message}\n\n" +
            $"Proxy sources: kort0881/telegram-proxy-collector, SoliSpirit/mtproto и другие открытые списки.";

        var buttons = update.HasNewVersion ? MessageBoxButtons.YesNo : MessageBoxButtons.OK;
        var result = MessageBox.Show(
            this,
            message,
            "О программе",
            buttons,
            MessageBoxIcon.Information
        );

        if (!update.HasNewVersion || result != DialogResult.Yes)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(update.ReleaseUrl))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = update.ReleaseUrl,
                UseShellExecute = true
            });
        }
        catch
        {
            // noop
        }
    }

    private async Task CheckUpdatesOnStartupAsync()
    {
        if (_startupUpdateChecked)
        {
            return;
        }

        _startupUpdateChecked = true;

        var versionText = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
        var currentVersion = Version.TryParse(versionText, out var parsed) ? parsed : new Version(1, 0, 0);
        var update = await _gitHubUpdateService.CheckAsync(
            AppMetadata.GitHubOwner,
            AppMetadata.GitHubRepository,
            currentVersion,
            CancellationToken.None
        );

        if (!update.HasNewVersion || string.IsNullOrWhiteSpace(update.ReleaseUrl))
        {
            return;
        }

        var result = MessageBox.Show(
            this,
            $"Доступна новая версия: {update.LatestTag}\nОткрыть страницу релиза?",
            "Обновление",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Information
        );

        if (result != DialogResult.Yes)
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = update.ReleaseUrl,
                UseShellExecute = true
            });
        }
        catch
        {
            // noop
        }
    }
}
