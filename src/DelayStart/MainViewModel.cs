using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using DelayStart.Localization;
using DelayStart.Models;
using DelayStart.Services;

namespace DelayStart;

public sealed class MainViewModel : ObservableObject
{
    private bool _loading;
    private bool _taskRegistered;
    private string? _taskExe;

    public MainViewModel()
    {
        Programs.CollectionChanged += OnProgramsChanged;
        Loc.Instance.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(WindowTitle));
            UpdateWarning();
        };
    }

    public static string ExePath { get; } = Environment.ProcessPath ?? "";

    public ObservableCollection<ProgramEntry> Programs { get; } = [];

    public ProgramEntry? SelectedProgram
    {
        get;
        set { if (Set(ref field, value)) OnPropertyChanged(nameof(HasSelection)); }
    }

    public bool HasSelection => SelectedProgram is not null;

    public bool StartWithWindows
    {
        get;
        set { if (Set(ref field, value)) MarkDirty(); }
    }

    /// <summary>"auto", "bg" или "en". Прилага се веднага, а се запазва с останалите настройки.</summary>
    public string Language
    {
        get;
        set
        {
            if (!Set(ref field, value))
                return;
            Loc.Apply(value);
            MarkDirty();
        }
    } = Loc.Auto;

    public bool IsDirty
    {
        get;
        private set { if (Set(ref field, value)) OnPropertyChanged(nameof(WindowTitle)); }
    }

    public string WindowTitle => IsDirty ? Loc.S.WindowTitle + " *" : Loc.S.WindowTitle;

    public string? Warning
    {
        get;
        private set { if (Set(ref field, value)) OnPropertyChanged(nameof(HasWarning)); }
    }

    public bool HasWarning => !string.IsNullOrEmpty(Warning);

    public string LastRun { get; private set => Set(ref field, value); } = "";

    public string Status { get; set => Set(ref field, value); } = "";

    public void Load()
    {
        _loading = true;
        try
        {
            bool firstRun = !ConfigStore.Exists;
            var config = ConfigStore.Load();

            Language = config.Language ?? Loc.Auto;
            foreach (var entry in config.Programs)
                Programs.Add(entry);

            _taskRegistered = AutostartTask.IsRegistered(out _taskExe);
            // При първо пускане отметката е включена – иначе лесно се забравя и нищо не тръгва.
            StartWithWindows = _taskRegistered || firstRun;
            LastRun = ConfigStore.LoadLastRun() ?? "";
            SelectedProgram = Programs.FirstOrDefault();
        }
        finally
        {
            _loading = false;
            UpdateWarning();
        }
    }

    public ProgramEntry Add(string path)
    {
        var entry = new ProgramEntry
        {
            Path = path,
            Name = Path.GetFileNameWithoutExtension(path),
            // Всяка нова програма по подразбиране е 10 секунди след последната.
            DelaySeconds = Programs.Count == 0 ? 30 : Programs.Max(p => p.DelaySeconds) + 10,
        };
        Programs.Add(entry);
        SelectedProgram = entry;
        return entry;
    }

    public void RemoveSelected()
    {
        if (SelectedProgram is not { } entry)
            return;

        int index = Programs.IndexOf(entry);
        Programs.Remove(entry);
        SelectedProgram = Programs.Count == 0 ? null : Programs[Math.Min(index, Programs.Count - 1)];
    }

    /// <summary>Записва настройките. Връща текст на грешка, ако има невалидни данни.</summary>
    public string? Save()
    {
        if (Programs.FirstOrDefault(p => string.IsNullOrWhiteSpace(p.Path)) is { } invalid)
        {
            SelectedProgram = invalid;
            return Loc.S.MissingPath;
        }

        ConfigStore.Save(new AppConfig
        {
            Language = Language == Loc.Auto ? null : Language,
            Programs = [.. Programs],
        });

        if (StartWithWindows)
            AutostartTask.Register(ExePath);
        else
            AutostartTask.Unregister();

        _taskRegistered = StartWithWindows;
        _taskExe = StartWithWindows ? ExePath : null;
        UpdateWarning();
        IsDirty = false;
        Status = string.Format(Loc.S.SavedAt, DateTime.Now);
        return null;
    }

    private void UpdateWarning()
    {
        var parts = new List<string>();

        if (!AutostartTask.IsInProtectedLocation(ExePath))
            parts.Add(string.Format(Loc.S.WarningUnprotectedLocation, Path.GetDirectoryName(ExePath)));

        if (_taskRegistered && _taskExe is not null && !string.Equals(_taskExe, ExePath, StringComparison.OrdinalIgnoreCase))
            parts.Add(string.Format(Loc.S.WarningTaskPointsElsewhere, _taskExe));

        Warning = parts.Count == 0 ? null : string.Join(Environment.NewLine + Environment.NewLine, parts);
    }

    private void OnProgramsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var entry in e.OldItems?.OfType<ProgramEntry>() ?? [])
            entry.PropertyChanged -= OnProgramChanged;
        foreach (var entry in e.NewItems?.OfType<ProgramEntry>() ?? [])
            entry.PropertyChanged += OnProgramChanged;

        MarkDirty();
    }

    private void OnProgramChanged(object? sender, PropertyChangedEventArgs e) => MarkDirty();

    private void MarkDirty()
    {
        if (!_loading)
            IsDirty = true;
    }
}
