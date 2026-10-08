using System.Diagnostics;
using DelayStart.Localization;
using DelayStart.Models;

namespace DelayStart.Services;

public static class ProgramLauncher
{
    private static readonly TimeSpan ShellRetryDelay = TimeSpan.FromSeconds(2);
    private const int ShellAttempts = 30;

    public static async Task LaunchAsync(ProgramEntry entry, CancellationToken cancellationToken = default)
    {
        var (file, arguments, workingDirectory) = Resolve(entry);

        if (entry.RunAsAdmin || !Environment.IsPrivilegedProcess)
        {
            var psi = new ProcessStartInfo(file, arguments)
            {
                UseShellExecute = true,
                WorkingDirectory = workingDirectory,
                // Нормално DelayStart вече е админ и детето наследява правата. Ако не е – иска ги през UAC.
                Verb = entry.RunAsAdmin && !Environment.IsPrivilegedProcess ? "runas" : "",
            };
            Process.Start(psi)?.Dispose();
            return;
        }

        // Ние сме админ, а програмата трябва да тръгне като обикновен потребител – пускаме я през Explorer.
        // Веднага след влизане Explorer може още да не е готов, затова опитваме няколко пъти.
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                if (!ShellLauncher.IsShellReady)
                    throw new InvalidOperationException(Loc.S.ExplorerNotReady);

                ShellLauncher.ShellExecute(file, arguments, workingDirectory);
                return;
            }
            catch (Exception) when (attempt < ShellAttempts)
            {
                await Task.Delay(ShellRetryDelay, cancellationToken);
            }
        }
    }

    private static (string File, string Arguments, string WorkingDirectory) Resolve(ProgramEntry entry)
    {
        string file = Expand(entry.Path).Trim('"');
        if (file.Length == 0)
            throw new InvalidOperationException(Loc.S.NoPath);

        if (Path.IsPathRooted(file) && !File.Exists(file) && !Directory.Exists(file))
            throw new FileNotFoundException(string.Format(Loc.S.FileNotFound, file), file);

        string workingDirectory = Expand(entry.WorkingDirectory);
        // За .lnk не подаваме папка, за да важи тази от самия пряк път.
        if (workingDirectory.Length == 0 && Path.IsPathRooted(file) &&
            string.Equals(Path.GetExtension(file), ".exe", StringComparison.OrdinalIgnoreCase))
        {
            workingDirectory = Path.GetDirectoryName(file) ?? "";
        }

        return (file, Expand(entry.Arguments), workingDirectory);
    }

    private static string Expand(string? value) => Environment.ExpandEnvironmentVariables(value?.Trim() ?? "");
}
