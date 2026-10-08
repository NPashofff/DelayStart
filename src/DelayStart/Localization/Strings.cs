namespace DelayStart.Localization;

/// <summary>
/// Всички текстове на интерфейса. Преводите са в Strings.English.cs и Strings.Bulgarian.cs.
/// Всяко поле е required, така че ако на някой език липсва превод, проектът не се компилира.
/// </summary>
public sealed partial class Strings
{
    // Прозорец с настройки
    public required string WindowTitle { get; init; }
    public required string StartWithWindows { get; init; }
    public required string Language { get; init; }
    public required string LanguageAuto { get; init; }
    public required string AddProgram { get; init; }
    public required string Remove { get; init; }
    public required string TestNow { get; init; }
    public required string TestNowTooltip { get; init; }
    public required string ColumnEnabled { get; init; }
    public required string ColumnName { get; init; }
    public required string ColumnDelay { get; init; }
    public required string ColumnAdmin { get; init; }
    public required string ColumnPath { get; init; }
    public required string SelectedProgram { get; init; }
    public required string FieldName { get; init; }
    public required string FieldNameTooltip { get; init; }
    public required string FieldProgram { get; init; }
    public required string Browse { get; init; }
    public required string FieldArguments { get; init; }
    public required string FieldWorkingDirectory { get; init; }
    public required string FieldWorkingDirectoryTooltip { get; init; }
    public required string FieldDelay { get; init; }
    public required string SecondsAfterSignIn { get; init; }
    public required string RunAsAdmin { get; init; }
    public required string Enabled { get; init; }
    public required string LastRun { get; init; }
    public required string Save { get; init; }
    public required string Close { get; init; }

    // Съобщения в прозореца с настройки
    public required string UnsavedChangesPrompt { get; init; }
    public required string LoadFailed { get; init; }
    public required string SaveFailed { get; init; }
    /// <summary>{0} = име на програмата</summary>
    public required string LaunchFailed { get; init; }
    /// <summary>{0} = име на програмата, {1} = ModeAdmin/ModeUser</summary>
    public required string Launched { get; init; }
    public required string ModeAdmin { get; init; }
    public required string ModeUser { get; init; }
    public required string PickProgramTitle { get; init; }
    public required string PickProgramFilter { get; init; }
    public required string PickFolderTitle { get; init; }
    public required string MissingPath { get; init; }
    /// <summary>{0} = час</summary>
    public required string SavedAt { get; init; }
    /// <summary>{0} = папката, от която е пуснат DelayStart</summary>
    public required string WarningUnprotectedLocation { get; init; }
    /// <summary>{0} = пътят, към който сочи задачата</summary>
    public required string WarningTaskPointsElsewhere { get; init; }

    // Task Scheduler и стартиране
    public required string TaskDescription { get; init; }
    public required string NoPath { get; init; }
    /// <summary>{0} = път</summary>
    public required string FileNotFound { get; init; }
    public required string ExplorerNotReady { get; init; }
    public required string ExplorerDesktopNotFound { get; init; }

    // Режим при влизане в Windows (tray и дневник)
    /// <summary>{0} = дата и час</summary>
    public required string LogSignedIn { get; init; }
    /// <summary>{0} = текст на грешката</summary>
    public required string LogUnexpectedError { get; init; }
    public required string LogNoPrograms { get; init; }
    /// <summary>{0} = брой нестартирани програми</summary>
    public required string LogCancelled { get; init; }
    public required string LogOk { get; init; }
    public required string LogError { get; init; }
    public required string TrayStartAllNow { get; init; }
    public required string TrayCancel { get; init; }
    /// <summary>{0} = оставащи програми, {1} = следваща програма, {2} = секунди</summary>
    public required string TrayCountdown { get; init; }
    public required string TrayHasErrors { get; init; }
    /// <summary>{0} = име на програмата</summary>
    public required string OneFailed { get; init; }
    /// <summary>{0} = брой програми</summary>
    public required string ManyFailed { get; init; }
    public required string SeeSettingsForDetails { get; init; }
}
