$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $root 'dist\SnapLayouts10.exe'
if (-not (Test-Path $exe)) {
    throw "$exe does not exist. Run build.ps1 before testing."
}

$assembly = [System.Reflection.Assembly]::LoadFile($exe)
$engine = $assembly.GetType('SnapLayouts10.SnapEngine')
$layoutsType = $assembly.GetType('SnapLayouts10.Layouts')
$method = $engine.GetMethod('CalculateZoneRect', [System.Reflection.BindingFlags]'Public,Static')
$layouts = $layoutsType.GetField('All').GetValue($null)
$expectedCounts = @(2, 2, 3, 3, 4, 4)
$work = New-Object System.Drawing.Rectangle 0, 0, 1920, 1040
$failures = New-Object System.Collections.Generic.List[string]

if ($layouts.Length -ne 6) {
    $failures.Add("Layout count: expected 6, found $($layouts.Length).")
}

for ($i = 0; $i -lt $layouts.Length; $i++) {
    $layout = $layouts[$i]
    $name = $layout.GetType().GetField('Name').GetValue($layout)
    $zones = $layout.GetType().GetField('Zones').GetValue($layout)
    if ($zones.Length -ne $expectedCounts[$i]) {
        $failures.Add("$name has $($zones.Length) zones; expected $($expectedCounts[$i]).")
    }

    $rects = @()
    for ($z = 0; $z -lt $zones.Length; $z++) {
        $rect = $method.Invoke($null, @($layout, $z, $work, 8))
        if ($rect.Width -lt 80 -or $rect.Height -lt 60) {
            $failures.Add("$name / zone $z is too small: $($rect.Width)x$($rect.Height).")
        }
        if ($rect.Left -lt $work.Left -or $rect.Top -lt $work.Top -or
            $rect.Right -gt $work.Right -or $rect.Bottom -gt $work.Bottom) {
            $failures.Add("$name / zone $z is outside the work area: $rect.")
        }
        $rects += $rect
    }

    for ($a = 0; $a -lt $rects.Count; $a++) {
        for ($b = $a + 1; $b -lt $rects.Count; $b++) {
            if ($rects[$a].IntersectsWith($rects[$b])) {
                $failures.Add("${name}: zones $a and $b overlap.")
            }
        }
    }
    Write-Host ("{0}: {1} zones, OK" -f $name, $zones.Length)
}

# Simulare monitor secundar, inclusiv coordonate absolute pozitive.
$farWork = New-Object System.Drawing.Rectangle 1920, 0, 2560, 1400
$farRect = $method.Invoke($null, @($layouts[4], 3, $farWork, 8))
if ($farRect.Width -lt 100 -or $farRect.Height -lt 100 -or
    $farRect.Left -lt $farWork.Left -or $farRect.Top -lt $farWork.Top -or
    $farRect.Right -gt $farWork.Right -or $farRect.Bottom -gt $farWork.Bottom) {
    $failures.Add("Secondary monitor test failed: $farRect.")
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Error $_ }
    throw "Geometry tests failed ($($failures.Count) issues)."
}

Write-Host "PASS: all layouts and the secondary monitor test passed."
