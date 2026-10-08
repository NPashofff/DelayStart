namespace DelayStart.Models;

public sealed class AppConfig
{
    /// <summary>"bg", "en" или null – по езика на Windows.</summary>
    public string? Language { get; set; }

    public List<ProgramEntry> Programs { get; set; } = [];
}
