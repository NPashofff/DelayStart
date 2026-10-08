using System.Diagnostics;
using System.Text;
using DelayStart.Localization;
using DelayStart.Models;
using DelayStart.Services;
using Forms = System.Windows.Forms;

namespace DelayStart;

/// <summary>
/// Режимът при влизане в Windows: иконка в tray-а с обратно броене, после програмите се стартират една по една.
/// Изчакването на всяка програма се брои от момента на влизане, а не от предишната програма.
/// </summary>
internal sealed class AutostartRunner
{
    private const int MaxTooltipLength = 127;
    private static readonly TimeSpan Tick = TimeSpan.FromSeconds(1);

    private readonly CancellationTokenSource _skipWaiting = new();
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly StringBuilder _log = new();
    private bool _cancelled;

    public async Task RunAsync()
    {
        try
        {
            var config = ConfigStore.Load();
            Loc.Apply(config.Language);
            _log.AppendLine(string.Format(Loc.S.LogSignedIn, DateTime.Now));
            await RunCoreAsync(config.Programs);
        }
        catch (Exception ex)
        {
            Log(string.Format(Loc.S.LogUnexpectedError, ex.Message));
        }
        finally
        {
            try { ConfigStore.SaveLastRun(_log.ToString()); } catch { /* няма къде другаде да го запишем */ }
        }
    }

    private async Task RunCoreAsync(IEnumerable<ProgramEntry> programs)
    {
        // Ако задачата е тръгнала с понижен приоритет, стартираните от нас програми ще го наследят.
        using (var self = Process.GetCurrentProcess())
            self.PriorityClass = ProcessPriorityClass.Normal;

        List<ProgramEntry> queue =
        [
            .. programs
                .Where(p => p.Enabled && !string.IsNullOrWhiteSpace(p.Path))
                .OrderBy(p => p.DelaySeconds),
        ];

        if (queue.Count == 0)
        {
            Log(Loc.S.LogNoPrograms);
            return;
        }

        using var tray = CreateTray();
        var failed = new List<string>();

        for (int i = 0; i < queue.Count; i++)
        {
            var entry = queue[i];
            await WaitUntilAsync(TimeSpan.FromSeconds(entry.DelaySeconds), entry, queue.Count - i, tray);

            if (_cancelled)
            {
                Log(string.Format(Loc.S.LogCancelled, queue.Count - i));
                break;
            }

            string mode = entry.RunAsAdmin ? Loc.S.ModeAdmin : Loc.S.ModeUser;
            try
            {
                await ProgramLauncher.LaunchAsync(entry);
                Log($"{Loc.S.LogOk,-7} {entry.DisplayName} ({mode})");
            }
            catch (Exception ex)
            {
                failed.Add(entry.DisplayName);
                Log($"{Loc.S.LogError,-7} {entry.DisplayName} ({mode}): {ex.Message}");
            }
        }

        if (failed.Count > 0)
        {
            string message = failed.Count == 1
                ? string.Format(Loc.S.OneFailed, failed[0])
                : string.Format(Loc.S.ManyFailed, failed.Count);
            SetTooltip(tray, Loc.S.TrayHasErrors);
            tray.ShowBalloonTip(10_000, "DelayStart", message + " " + Loc.S.SeeSettingsForDetails, Forms.ToolTipIcon.Warning);
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }

    private async Task WaitUntilAsync(TimeSpan dueAt, ProgramEntry next, int remaining, Forms.NotifyIcon tray)
    {
        while (!_skipWaiting.IsCancellationRequested)
        {
            var left = dueAt - _clock.Elapsed;
            if (left <= TimeSpan.Zero)
                return;

            SetTooltip(tray, string.Format(Loc.S.TrayCountdown, remaining, next.DisplayName, Math.Ceiling(left.TotalSeconds)));
            try
            {
                await Task.Delay(left < Tick ? left : Tick, _skipWaiting.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private Forms.NotifyIcon CreateTray()
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add(Loc.S.TrayStartAllNow, null, (_, _) => _skipWaiting.Cancel());
        menu.Items.Add(Loc.S.TrayCancel, null, (_, _) =>
        {
            _cancelled = true;
            _skipWaiting.Cancel();
        });

        var tray = new Forms.NotifyIcon
        {
            Icon = LoadTrayIcon(),
            Text = "DelayStart",
            ContextMenuStrip = menu,
            Visible = true,
        };
        tray.Disposed += (_, _) => menu.Dispose();
        return tray;
    }

    private static System.Drawing.Icon LoadTrayIcon()
    {
        using var stream = typeof(AutostartRunner).Assembly.GetManifestResourceStream("DelayStart.ico");
        return stream is null
            ? System.Drawing.SystemIcons.Application
            : new System.Drawing.Icon(stream, Forms.SystemInformation.SmallIconSize);
    }

    private static void SetTooltip(Forms.NotifyIcon tray, string text) =>
        tray.Text = text.Length <= MaxTooltipLength ? text : text[..(MaxTooltipLength - 1)] + "…";

    private void Log(string line) => _log.AppendLine($"[{_clock.Elapsed:mm\\:ss}] {line}");
}
