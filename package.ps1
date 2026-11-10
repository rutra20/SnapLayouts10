param(
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$dist = Join-Path $root 'dist'
$src = Join-Path $root 'src'

if (-not $SkipBuild) {
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $root 'build.ps1')
    if ($LASTEXITCODE -ne 0) {
        throw "Compilarea a eșuat."
    }
}

$exe = Join-Path $dist 'SnapLayouts10.exe'
if (-not (Test-Path $exe)) {
    throw "Nu există $exe. Rulați mai întâi build.ps1."
}

$assemblyInfo = Get-Content (Join-Path $src 'AssemblyInfo.cs') -Raw
$match = [regex]::Match($assemblyInfo, 'AssemblyVersion\("([0-9]+\.[0-9]+\.[0-9]+)')
if (-not $match.Success) {
    throw "Nu pot determina versiunea din AssemblyInfo.cs."
}
$version = $match.Groups[1].Value
$portable = Join-Path $dist ("SnapLayouts10-{0}-portable.zip" -f $version)
$source = Join-Path $dist ("SnapLayouts10-{0}-source.zip" -f $version)
$portableStage = Join-Path $env:TEMP ("SnapLayouts10-portable-" + [Guid]::NewGuid().ToString('N'))
$sourceStage = Join-Path $env:TEMP ("SnapLayouts10-source-" + [Guid]::NewGuid().ToString('N'))

function Copy-IfExists {
    param([string]$Path, [string]$Destination)
    if (Test-Path $Path) {
        Copy-Item $Path $Destination -Recurse -Force
    }
}

try {
    New-Item -ItemType Directory -Force -Path $dist, $portableStage, $sourceStage | Out-Null
    Remove-Item $portable, $source -Force -ErrorAction SilentlyContinue

    # Portable archive: only what an end user needs.
    Copy-Item $exe $portableStage -Force
    foreach ($file in @('README.md', 'LICENSE', 'NOTICE.md', 'CHANGELOG.md',
                        'CONTRIBUTING.md', 'SECURITY.md', 'CODE_OF_CONDUCT.md')) {
        Copy-IfExists (Join-Path $root $file) $portableStage
    }
    Copy-IfExists (Join-Path $root 'docs') $portableStage
    Compress-Archive -Path (Join-Path $portableStage '*') -DestinationPath $portable -CompressionLevel Optimal

    # Source archive: sources, scripts, documentation, GitHub metadata.
    foreach ($dir in @('src', 'tools', 'assets', 'docs', '.github')) {
        Copy-IfExists (Join-Path $root $dir) $sourceStage
    }
    foreach ($file in @('build.ps1', 'package.ps1', 'README.md', 'LICENSE', 'NOTICE.md',
                        'CHANGELOG.md', 'CONTRIBUTING.md', 'SECURITY.md', 'CODE_OF_CONDUCT.md',
                        '.gitignore', '.gitattributes', '.editorconfig')) {
        Copy-IfExists (Join-Path $root $file) $sourceStage
    }
    Compress-Archive -Path (Join-Path $sourceStage '*') -DestinationPath $source -CompressionLevel Optimal

    $files = @((Split-Path $exe -Leaf), (Split-Path $portable -Leaf), (Split-Path $source -Leaf))
    $hashes = foreach ($file in $files) {
        $hash = Get-FileHash (Join-Path $dist $file) -Algorithm SHA256
        "{0}  {1}" -f $hash.Hash, $file
    }
    $hashes | Set-Content (Join-Path $dist 'SHA256SUMS.txt') -Encoding ASCII

    Write-Host ("Versiune: {0}" -f $version)
    Get-ChildItem $dist -File | Where-Object { $_.Name -in $files -or $_.Name -eq 'SHA256SUMS.txt' } |
        Select-Object Name, Length | Format-Table -AutoSize
    Write-Host "Pachete create în $dist"
}
finally {
    Remove-Item $portableStage, $sourceStage -Recurse -Force -ErrorAction SilentlyContinue
}
