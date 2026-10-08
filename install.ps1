<#
.SYNOPSIS
    Installs the latest DelayStart release. / Инсталира последната версия на DelayStart.

.DESCRIPTION
    irm https://raw.githubusercontent.com/NPashofff/DelayStart/main/install.ps1 | iex

    1. Finds the latest release on GitHub and picks the installer in the Windows language (Bulgarian or English).
    2. Downloads it and checks its SHA256 against SHA256SUMS.txt from the same release.
    3. Installs the .NET 10 Desktop Runtime if it is missing.
    4. Runs the MSI (Windows asks for administrator rights).
#>
param(
    # Only download and verify the installer, do not install anything.
    [switch]$DownloadOnly,
    # Reinstall even if the same version is already installed.
    [switch]$Force
)

# Separate scope, so that the settings below do not leak into the caller's session when run via iex.
& {
    $ErrorActionPreference = 'Stop'
    $ProgressPreference = 'SilentlyContinue'
    [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12

    $repo = 'NPashofff/DelayStart'
    $bulgarian = (Get-UICulture).TwoLetterISOLanguageName -eq 'bg'
    function Say([string]$en, [string]$bg) { Write-Host $(if ($bulgarian) { $bg } else { $en }) }
    function Fail([string]$en, [string]$bg) { throw $(if ($bulgarian) { $bg } else { $en }) }
    function Get-Text($url) {
        $content = (Invoke-WebRequest $url -UseBasicParsing).Content
        if ($content -is [byte[]]) { [Text.Encoding]::UTF8.GetString($content) } else { $content }
    }

    if (-not [Environment]::Is64BitOperatingSystem) {
        Fail 'DelayStart needs 64-bit Windows.' 'DelayStart изисква 64-битов Windows.'
    }

    # 1. Последното издание и инсталаторът на езика на Windows
    $release = Invoke-RestMethod "https://api.github.com/repos/$repo/releases/latest" -Headers @{ 'User-Agent' = 'DelayStart-installer' }
    $version = $release.tag_name.TrimStart('v')
    $culture = if ($bulgarian) { 'bg-BG' } else { 'en-US' }
    $msiName = "DelayStart-$version-$culture.msi"
    $msiAsset = $release.assets | Where-Object name -eq $msiName
    $sumsAsset = $release.assets | Where-Object name -eq 'SHA256SUMS.txt'
    if (-not $msiAsset -or -not $sumsAsset) {
        Fail "Release $version has no $msiName or SHA256SUMS.txt." "В издание $version липсва $msiName или SHA256SUMS.txt."
    }

    $installed = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*' -ErrorAction SilentlyContinue |
        Where-Object DisplayName -eq 'DelayStart' | Select-Object -First 1
    if ($installed -and $installed.DisplayVersion -eq $version -and -not $Force -and -not $DownloadOnly) {
        Say "DelayStart $version is already installed." "DelayStart $version вече е инсталиран."
        return
    }

    # 2. Изтегляне и проверка на SHA256
    $dir = Join-Path $env:TEMP "DelayStart-setup-$version"
    New-Item -ItemType Directory -Force $dir | Out-Null
    $msi = Join-Path $dir $msiName
    Say "Downloading DelayStart $version ($culture)..." "Изтегляне на DelayStart $version ($culture)..."
    Invoke-WebRequest $msiAsset.browser_download_url -OutFile $msi -UseBasicParsing

    $expected = $null
    foreach ($line in (Get-Text $sumsAsset.browser_download_url) -split "`n") {
        if ($line -match "^\s*([0-9a-fA-F]{64})\s+\*?$([regex]::Escape($msiName))\s*$") { $expected = $Matches[1]; break }
    }
    $actual = (Get-FileHash $msi -Algorithm SHA256).Hash
    if (-not $expected -or $actual -ne $expected) {
        Fail "Checksum mismatch for $msiName - the download is not installed." "Контролната сума на $msiName не съвпада – изтегленият файл не се инсталира."
    }
    Say "SHA256 OK: $actual" "SHA256 е наред: $actual"

    if ($DownloadOnly) {
        Say "Downloaded: $msi" "Изтеглено: $msi"
        return
    }

    # 3. .NET 10 Desktop Runtime
    $runtime = "$env:ProgramFiles\dotnet\shared\Microsoft.WindowsDesktop.App\10.*"
    if (-not (Test-Path $runtime)) {
        Say 'Installing the .NET 10 Desktop Runtime...' 'Инсталиране на .NET 10 Desktop Runtime...'
        if (Get-Command winget -ErrorAction SilentlyContinue) {
            winget install --id Microsoft.DotNet.DesktopRuntime.10 --exact --silent --accept-package-agreements --accept-source-agreements
        }
        else {
            $setup = Join-Path $dir 'windowsdesktop-runtime-win-x64.exe'
            Invoke-WebRequest 'https://aka.ms/dotnet/10.0/windowsdesktop-runtime-win-x64.exe' -OutFile $setup -UseBasicParsing
            $signature = Get-AuthenticodeSignature $setup
            if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch 'O=Microsoft Corporation') {
                Fail 'The .NET runtime installer is not signed by Microsoft.' 'Инсталаторът на .NET не е подписан от Microsoft.'
            }
            Start-Process $setup -ArgumentList '/install /quiet /norestart' -Verb RunAs -Wait
        }
        if (-not (Test-Path $runtime)) {
            Fail 'The .NET 10 Desktop Runtime could not be installed.' 'Не успях да инсталирам .NET 10 Desktop Runtime.'
        }
    }

    # 4. Инсталиране (UAC)
    Say 'Installing DelayStart - Windows will ask for administrator rights...' 'Инсталиране на DelayStart – Windows ще поиска администраторски права...'
    $process = Start-Process msiexec.exe -ArgumentList "/i `"$msi`" /passive /norestart" -Verb RunAs -Wait -PassThru
    if ($process.ExitCode -notin 0, 3010) {
        Fail "Installation failed (msiexec exit code $($process.ExitCode))." "Инсталирането не успя (msiexec код $($process.ExitCode))."
    }

    Say "DelayStart $version is installed. Open it from the Start menu to add programs." "DelayStart $version е инсталиран. Отвори го от Start менюто, за да добавиш програми."
}
