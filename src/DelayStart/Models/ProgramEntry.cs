using System.Text.Json.Serialization;

namespace DelayStart.Models;

public sealed class ProgramEntry : ObservableObject
{
    public const int MaxDelaySeconds = 24 * 60 * 60;

    public bool Enabled { get; set => Set(ref field, value); } = true;

    public string Name
    {
        get;
        set { if (Set(ref field, value)) OnPropertyChanged(nameof(DisplayName)); }
    } = "";

    /// <summary>Път до .exe, .lnk, .bat и т.н. Може да съдържа променливи като %ProgramFiles%.</summary>
    public string Path
    {
        get;
        set { if (Set(ref field, value)) OnPropertyChanged(nameof(DisplayName)); }
    } = "";

    public string Arguments { get; set => Set(ref field, value); } = "";

    /// <summary>Празно = папката на програмата.</summary>
    public string WorkingDirectory { get; set => Set(ref field, value); } = "";

    /// <summary>Секунди след влизане в Windows.</summary>
    public int DelaySeconds { get; set => Set(ref field, Math.Clamp(value, 0, MaxDelaySeconds)); } = 30;

    public bool RunAsAdmin { get; set => Set(ref field, value); } = true;

    [JsonIgnore]
    public string DisplayName =>
        string.IsNullOrWhiteSpace(Name) ? System.IO.Path.GetFileNameWithoutExtension(Path) : Name;
}
