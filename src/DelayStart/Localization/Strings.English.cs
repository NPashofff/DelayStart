namespace DelayStart.Localization;

public sealed partial class Strings
{
    public static Strings English { get; } = new()
    {
        WindowTitle = "DelayStart – Settings",
        StartWithWindows = "Start DelayStart when I sign in to Windows (as administrator, no UAC prompt)",
        Language = "Language:",
        LanguageAuto = "Automatic (Windows language)",
        AddProgram = "Add program…",
        Remove = "Remove",
        TestNow = "Test now",
        TestNowTooltip = "Starts the selected program right away, with the chosen rights",
        ColumnEnabled = "On",
        ColumnName = "Name",
        ColumnDelay = "Delay (s)",
        ColumnAdmin = "As admin",
        ColumnPath = "Path",
        SelectedProgram = "Selected program",
        FieldName = "Name:",
        FieldNameTooltip = "If empty, the file name is shown",
        FieldProgram = "Program:",
        Browse = "Browse…",
        FieldArguments = "Arguments:",
        FieldWorkingDirectory = "Working folder:",
        FieldWorkingDirectoryTooltip = "If empty, the program's own folder is used",
        FieldDelay = "Delay:",
        SecondsAfterSignIn = "seconds after sign-in",
        RunAsAdmin = "Run as administrator",
        Enabled = "Enabled",
        LastRun = "Last run",
        Save = "Save",
        Close = "Close",

        UnsavedChangesPrompt = "You have unsaved changes. Save them?",
        LoadFailed = "The settings could not be loaded.",
        SaveFailed = "The settings could not be saved.",
        LaunchFailed = "“{0}” could not be started.",
        Launched = "Started: {0} ({1}).",
        ModeAdmin = "admin",
        ModeUser = "user",
        PickProgramTitle = "Choose a program",
        PickProgramFilter = "Programs and shortcuts|*.exe;*.lnk;*.bat;*.cmd|All files|*.*",
        PickFolderTitle = "Working folder",
        MissingPath = "One of the programs has no path.",
        SavedAt = "Saved at {0:HH:mm:ss}.",
        WarningUnprotectedLocation =
            "DelayStart is running from “{0}”. Programs without admin rights can write to this folder, so they could " +
            "replace DelayStart.exe and get admin rights at your next sign-in. Install DelayStart with the installer " +
            @"(into C:\Program Files\DelayStart) before real use.",
        WarningTaskPointsElsewhere = "Autostart currently points to “{0}”. Saving will point it to this file.",

        TaskDescription = "Starts the DelayStart programs after their delays.",
        NoPath = "No program path is set.",
        FileNotFound = "File not found: {0}",
        ExplorerNotReady = "Explorer has not started yet.",
        ExplorerDesktopNotFound = "The Explorer desktop was not found.",

        LogSignedIn = "Signed in to Windows: {0:yyyy-MM-dd HH:mm:ss}",
        LogUnexpectedError = "Unexpected error: {0}",
        LogNoPrograms = "No enabled programs.",
        LogCancelled = "Cancelled by the user. Not started: {0}.",
        LogOk = "OK",
        LogError = "ERROR",
        TrayStartAllNow = "Start all now",
        TrayCancel = "Cancel",
        TrayCountdown = "DelayStart – {0} left\n{1} in {2} s",
        TrayHasErrors = "DelayStart – there were errors",
        OneFailed = "“{0}” failed to start.",
        ManyFailed = "{0} programs failed to start.",
        SeeSettingsForDetails = "Details are in the settings.",
    };
}
