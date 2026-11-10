param(
    [switch]$NoIcon
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$src = Join-Path $root 'src'
$dist = Join-Path $root 'dist'
$assets = Join-Path $root 'assets'
$icon = Join-Path $assets 'app.ico'
$exe = Join-Path $dist 'SnapLayouts10.exe'

$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) {
    throw "Nu găsesc compilatorul C# .NET Framework: $csc"
}

if (-not (Test-Path $icon) -and -not $NoIcon) {
    & (Join-Path $root 'tools\make-icon.ps1')
}
if (-not (Test-Path $dist)) {
    New-Item -ItemType Directory -Path $dist | Out-Null
}

$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$references = @(
    (Join-Path $framework 'System.dll'),
    (Join-Path $framework 'System.Core.dll'),
    (Join-Path $framework 'System.Drawing.dll'),
    (Join-Path $framework 'System.Windows.Forms.dll'),
    (Join-Path $framework 'System.Xml.dll')
)

$sources = Get-ChildItem (Join-Path $src '*.cs') | Sort-Object Name | ForEach-Object { $_.FullName }

$arguments = @(
    '/nologo',
    '/target:winexe',
    '/platform:x64',
    '/optimize+',
    '/debug-',
    '/warn:4',
    '/utf8output',
    ('/out:' + $exe),
    ('/win32manifest:' + (Join-Path $src 'app.manifest'))
)
if (Test-Path $icon) {
    $arguments += ('/win32icon:' + $icon)
}
foreach ($reference in $references) {
    $arguments += ('/reference:' + $reference)
}
$arguments += $sources

Write-Host 'Compilare SnapLayouts10.exe...'
& $csc $arguments
if ($LASTEXITCODE -ne 0) {
    throw "Compilarea a eșuat (exit code $LASTEXITCODE)."
}

$info = Get-Item $exe
Write-Host ("OK: {0} ({1:N0} bytes)" -f $info.FullName, $info.Length)
Write-Host ("Versiune: {0}" -f $info.VersionInfo.FileVersion)
