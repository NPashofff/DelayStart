using System.Windows;
using DelayStart.Services;

namespace DelayStart;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Пуснати от Task Scheduler при влизане в Windows: само tray иконка, без прозорец.
        if (e.Args.Contains(AutostartTask.AutostartArgument, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                await new AutostartRunner().RunAsync();
            }
            finally
            {
                Shutdown();
            }
            return;
        }

        ShutdownMode = ShutdownMode.OnMainWindowClose;
        MainWindow = new MainWindow();
        MainWindow.Show();
    }
}
