using System.Diagnostics;
using System.Security.Principal;
using DelayStart.Localization;
using Microsoft.Win32.TaskScheduler;

namespace DelayStart.Services;

/// <summary>
/// Задача в Task Scheduler, която пуска DelayStart при влизане в Windows с най-високи права.
/// Така DelayStart тръгва като админ без UAC прозорец, а програмите, пуснати от него, наследяват правата му.
/// </summary>
public static class AutostartTask
{
    public const string TaskName = "DelayStart";
    public const string AutostartArgument = "--autostart";

    public static bool IsRegistered(out string? exePath)
    {
        using var ts = new TaskService();
        using var task = ts.GetTask(TaskName);
        exePath = task?.Definition.Actions.OfType<ExecAction>().FirstOrDefault()?.Path;
        return task is not null;
    }

    public static void Register(string exePath)
    {
        string user = WindowsIdentity.GetCurrent().Name;

        using var ts = new TaskService();
        using var td = ts.NewTask();

        td.RegistrationInfo.Description = Loc.S.TaskDescription;

        td.Principal.UserId = user;
        td.Principal.LogonType = TaskLogonType.InteractiveToken;
        td.Principal.RunLevel = TaskRunLevel.Highest;

        td.Triggers.Add(new LogonTrigger { UserId = user });
        td.Actions.Add(new ExecAction(exePath, AutostartArgument, Path.GetDirectoryName(exePath)));

        // По подразбиране Task Scheduler пуска с понижен приоритет, а стартираните програми го наследяват.
        td.Settings.Priority = ProcessPriorityClass.Normal;
        // По подразбиране задачата се спира принудително след 72 часа.
        td.Settings.ExecutionTimeLimit = TimeSpan.Zero;
        // По подразбиране задачата не тръгва, ако лаптопът е на батерия.
        td.Settings.DisallowStartIfOnBatteries = false;
        td.Settings.StopIfGoingOnBatteries = false;
        td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;

        ts.RootFolder.RegisterTaskDefinition(TaskName, td, TaskCreation.CreateOrUpdate, user, null, TaskLogonType.InteractiveToken);
    }

    public static void Unregister()
    {
        using var ts = new TaskService();
        ts.RootFolder.DeleteTask(TaskName, exceptionOnNotExists: false);
    }

    /// <summary>
    /// Дали exe-то е в папка, в която пишат само администратори.
    /// Ако не е, програма без админ права може да го подмени и да получи админ права при следващото влизане.
    /// </summary>
    public static bool IsInProtectedLocation(string exePath)
    {
        string[] roots =
        [
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
        ];

        return roots.Any(root => root.Length > 0 &&
            exePath.StartsWith(Path.TrimEndingDirectorySeparator(root) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
    }
}
