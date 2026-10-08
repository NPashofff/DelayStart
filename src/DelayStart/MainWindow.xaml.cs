using System.ComponentModel;
using System.Windows;
using DelayStart.Localization;
using DelayStart.Services;
using Microsoft.Win32;

namespace DelayStart;

public partial class MainWindow : Window
{
    private const string Caption = "DelayStart";
    private readonly MainViewModel _vm = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;

        try
        {
            _vm.Load();
        }
        catch (Exception ex)
        {
            ShowError(Loc.S.LoadFailed, ex);
        }
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        if (PickProgram(initialPath: null) is not { } path)
            return;

        var entry = _vm.Add(path);
        ProgramsGrid.ScrollIntoView(entry);
    }

    private void Remove_Click(object sender, RoutedEventArgs e) => _vm.RemoveSelected();

    private async void Test_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedProgram is not { } entry)
            return;

        try
        {
            await ProgramLauncher.LaunchAsync(entry);
            _vm.Status = string.Format(Loc.S.Launched, entry.DisplayName, entry.RunAsAdmin ? Loc.S.ModeAdmin : Loc.S.ModeUser);
        }
        catch (Exception ex)
        {
            ShowError(string.Format(Loc.S.LaunchFailed, entry.DisplayName), ex);
        }
    }

    private void BrowsePath_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedProgram is not { } entry || PickProgram(entry.Path) is not { } path)
            return;

        entry.Path = path;
        if (string.IsNullOrWhiteSpace(entry.Name))
            entry.Name = Path.GetFileNameWithoutExtension(path);
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedProgram is not { } entry)
            return;

        var dialog = new OpenFolderDialog { Title = Loc.S.PickFolderTitle };
        if (Directory.Exists(entry.WorkingDirectory))
            dialog.InitialDirectory = entry.WorkingDirectory;

        if (dialog.ShowDialog(this) == true)
            entry.WorkingDirectory = dialog.FolderName;
    }

    private void Save_Click(object sender, RoutedEventArgs e) => TrySave();

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    protected override void OnClosing(CancelEventArgs e)
    {
        if (_vm.IsDirty)
        {
            var answer = MessageBox.Show(this, Loc.S.UnsavedChangesPrompt, Caption,
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (answer == MessageBoxResult.Cancel || (answer == MessageBoxResult.Yes && !TrySave()))
                e.Cancel = true;
        }

        base.OnClosing(e);
    }

    private bool TrySave()
    {
        try
        {
            if (_vm.Save() is { } validationError)
            {
                MessageBox.Show(this, validationError, Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            ShowError(Loc.S.SaveFailed, ex);
            return false;
        }
    }

    private string? PickProgram(string? initialPath)
    {
        var dialog = new OpenFileDialog
        {
            Title = Loc.S.PickProgramTitle,
            Filter = Loc.S.PickProgramFilter,
            // Иначе при избор на .lnk получаваме пътя до целта, а не до самия пряк път.
            DereferenceLinks = false,
        };

        string? folder = string.IsNullOrWhiteSpace(initialPath) ? null : Path.GetDirectoryName(Environment.ExpandEnvironmentVariables(initialPath));
        if (Directory.Exists(folder))
            dialog.InitialDirectory = folder;

        return dialog.ShowDialog(this) == true ? dialog.FileName : null;
    }

    private void ShowError(string message, Exception ex) =>
        MessageBox.Show(this, $"{message}{Environment.NewLine}{Environment.NewLine}{ex.Message}", Caption,
            MessageBoxButton.OK, MessageBoxImage.Error);
}
