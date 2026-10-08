using System.Text.Encodings.Web;
using System.Text.Json;
using DelayStart.Models;
using Microsoft.Win32;

namespace DelayStart.Services;

/// <summary>
/// Настройките се пазят в HKLM\SOFTWARE\DelayStart. Там могат да пишат само администратори,
/// така че програма без админ права не може да се добави в списъка и после да бъде пусната като админ.
/// </summary>
public static class ConfigStore
{
    private const string KeyPath = @"SOFTWARE\DelayStart";
    private const string ConfigValue = "Config";
    private const string LastRunValue = "LastRun";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        // Кирилицата да се записва четимо, а не като \uXXXX.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static bool Exists
    {
        get
        {
            using var key = OpenKey(writable: false);
            return key?.GetValue(ConfigValue) is not null;
        }
    }

    public static AppConfig Load()
    {
        using var key = OpenKey(writable: false);
        return key?.GetValue(ConfigValue) is string { Length: > 0 } json
            ? JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig()
            : new AppConfig();
    }

    public static void Save(AppConfig config)
    {
        using var key = OpenKey(writable: true)!;
        key.SetValue(ConfigValue, JsonSerializer.Serialize(config, JsonOptions), RegistryValueKind.String);
    }

    public static string? LoadLastRun()
    {
        using var key = OpenKey(writable: false);
        return key?.GetValue(LastRunValue) as string;
    }

    public static void SaveLastRun(string text)
    {
        using var key = OpenKey(writable: true)!;
        key.SetValue(LastRunValue, text, RegistryValueKind.String);
    }

    private static RegistryKey? OpenKey(bool writable)
    {
        using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        return writable ? hklm.CreateSubKey(KeyPath, writable: true) : hklm.OpenSubKey(KeyPath);
    }
}
