# DelayStart

Starts programs after you sign in to Windows, each with its own delay – **as administrator without a UAC prompt**, or as a normal user. English and Bulgarian interface.

## Install

Run in PowerShell:

```powershell
irm https://raw.githubusercontent.com/NPashofff/DelayStart/main/install.ps1 | iex
```

The script downloads the latest [release](https://github.com/NPashofff/DelayStart/releases) in your Windows language, checks its SHA256, installs the [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) if it is missing and runs the installer. You can also download the `.msi` from the releases page yourself.

Requirements: Windows 10/11 x64.

To uninstall: *Settings → Apps → Installed apps → DelayStart → Uninstall*. This also removes the autostart task and the settings.

## How it works

- **Settings** – `DelayStart.exe` (asks for administrator rights). The program list is stored in `HKLM\SOFTWARE\DelayStart`, which only administrators can write.
- **Autostart** – a Task Scheduler task `DelayStart` with *Run with highest privileges* starts `DelayStart.exe --autostart` at sign-in, so there is no UAC prompt.
- **Countdown** – a tray icon shows what starts next; you can start everything now or cancel.
- **As administrator** – the program is started directly and inherits DelayStart's rights.
- **As normal user** – the program is started through Explorer, so it runs without administrator rights.

## Security

DelayStart starts programs as administrator without asking, so everything it starts must be in a folder only administrators can write to:

- the installer always puts DelayStart in `C:\Program Files\DelayStart`; if it runs from anywhere else, the settings window shows a warning;
- programs you add with *As admin* should also live in `Program Files` (not in `Downloads`, the desktop, etc.), otherwise any program without admin rights could replace them and get admin rights at your next sign-in.

## Build

Requires the .NET 10 SDK.

```powershell
dotnet build                            # the app
dotnet build installer -c Release       # MSI installers: installer\bin\Release\{en-US,bg-BG}\
```

Releases are built by GitHub Actions: set `<Version>` in [Directory.Build.props](Directory.Build.props), then push a tag `v<version>` (e.g. `v0.2.0`).

## Български

DelayStart стартира програми след влизане в Windows, всяка със собствено изчакване – като администратор без UAC прозорец или като обикновен потребител. Интерфейсът е на български и английски.

Инсталиране – в PowerShell:

```powershell
irm https://raw.githubusercontent.com/NPashofff/DelayStart/main/install.ps1 | iex
```

Скриптът изтегля последната версия на езика на Windows, проверява SHA256, при нужда инсталира .NET 10 Desktop Runtime и пуска инсталатора. Програмите, които се пускат като администратор, трябва да са в `Program Files`, а не в `Downloads` или на работния плот.

## License

[MIT](LICENSE)
