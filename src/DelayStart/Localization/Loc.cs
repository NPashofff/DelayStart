using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace DelayStart.Localization;

/// <summary>Текущият език. При смяна всички текстове в прозореца се обновяват веднага.</summary>
public sealed class Loc : INotifyPropertyChanged
{
    public const string Auto = "auto";
    public const string Bulgarian = "bg";
    public const string English = "en";

    public static Loc Instance { get; } = new();

    /// <summary>Текстовете на текущия език – за ползване от кода.</summary>
    public static Strings S => Instance.Current;

    public Strings Current { get; private set; } = Resolve(Auto);

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <param name="language">"bg", "en" или "auto"/null – по езика на Windows.</param>
    public static void Apply(string? language)
    {
        var strings = Resolve(language);
        if (ReferenceEquals(strings, Instance.Current))
            return;

        Instance.Current = strings;
        Instance.PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs(nameof(Current)));
    }

    private static Strings Resolve(string? language) => language switch
    {
        Bulgarian => Strings.Bulgarian,
        English => Strings.English,
        _ => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "bg" ? Strings.Bulgarian : Strings.English,
    };
}

/// <summary>
/// Преведен текст в XAML: <c>Text="{l:Tr Save}"</c>. Името е на свойство от <see cref="Strings"/>.
/// </summary>
public sealed class TrExtension(string key) : MarkupExtension
{
    public string Key { get; set; } = key;

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new Binding($"{nameof(Loc.Current)}.{Key}") { Source = Loc.Instance, Mode = BindingMode.OneWay }
            .ProvideValue(serviceProvider);
}
