namespace DelayStart.Localization;

public sealed partial class Strings
{
    public static Strings Bulgarian { get; } = new()
    {
        WindowTitle = "DelayStart – настройки",
        StartWithWindows = "Стартирай DelayStart при влизане в Windows (като администратор, без UAC прозорец)",
        Language = "Език:",
        LanguageAuto = "Автоматично (езика на Windows)",
        AddProgram = "Добави програма…",
        Remove = "Премахни",
        TestNow = "Тествай сега",
        TestNowTooltip = "Стартира избраната програма веднага, със зададените права",
        ColumnEnabled = "Вкл.",
        ColumnName = "Име",
        ColumnDelay = "Изчакване (сек)",
        ColumnAdmin = "Като админ",
        ColumnPath = "Път",
        SelectedProgram = "Избрана програма",
        FieldName = "Име:",
        FieldNameTooltip = "Ако е празно, се показва името на файла",
        FieldProgram = "Програма:",
        Browse = "Избери…",
        FieldArguments = "Аргументи:",
        FieldWorkingDirectory = "Работна папка:",
        FieldWorkingDirectoryTooltip = "Ако е празно, се ползва папката на програмата",
        FieldDelay = "Изчакване:",
        SecondsAfterSignIn = "секунди след влизане",
        RunAsAdmin = "Като администратор",
        Enabled = "Включена",
        LastRun = "Последно стартиране",
        Save = "Запази",
        Close = "Затвори",

        UnsavedChangesPrompt = "Има незапазени промени. Да ги запазя ли?",
        LoadFailed = "Настройките не можаха да бъдат заредени.",
        SaveFailed = "Настройките не можаха да бъдат запазени.",
        LaunchFailed = "„{0}“ не можа да бъде стартирана.",
        Launched = "Стартирано: {0} ({1}).",
        ModeAdmin = "админ",
        ModeUser = "потребител",
        PickProgramTitle = "Избери програма",
        PickProgramFilter = "Програми и преки пътища|*.exe;*.lnk;*.bat;*.cmd|Всички файлове|*.*",
        PickFolderTitle = "Работна папка",
        MissingPath = "Има програма без зададен път.",
        SavedAt = "Запазено в {0:HH:mm:ss}.",
        WarningUnprotectedLocation =
            "DelayStart е стартиран от „{0}“. В тази папка може да пише и програма без админ права – тя би могла " +
            "да подмени DelayStart.exe и да получи админ права при следващото влизане. Преди реална употреба " +
            @"инсталирай DelayStart с инсталатора (в C:\Program Files\DelayStart).",
        WarningTaskPointsElsewhere = "Автостартът в момента сочи към „{0}“. При запис ще бъде пренасочен към този файл.",

        TaskDescription = "Стартира програмите от DelayStart след зададеното изчакване.",
        NoPath = "Не е зададен път до програма.",
        FileNotFound = "Файлът не е намерен: {0}",
        ExplorerNotReady = "Explorer още не е стартирал.",
        ExplorerDesktopNotFound = "Работният плот на Explorer не е намерен.",

        LogSignedIn = "Влизане в Windows: {0:yyyy-MM-dd HH:mm:ss}",
        LogUnexpectedError = "Неочаквана грешка: {0}",
        LogNoPrograms = "Няма включени програми.",
        LogCancelled = "Отказано от потребителя. Не са стартирани: {0}.",
        LogOk = "OK",
        LogError = "ГРЕШКА",
        TrayStartAllNow = "Стартирай всички сега",
        TrayCancel = "Откажи",
        TrayCountdown = "DelayStart – остават {0}\n{1} след {2} сек",
        TrayHasErrors = "DelayStart – има грешки",
        OneFailed = "„{0}“ не успя да стартира.",
        ManyFailed = "{0} програми не успяха да стартират.",
        SeeSettingsForDetails = "Подробности има в настройките.",
    };
}
