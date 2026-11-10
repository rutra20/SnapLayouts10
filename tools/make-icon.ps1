param(
    [string]$OutputPath = (Join-Path $PSScriptRoot '..\assets\app.ico')
)

Add-Type -AssemblyName System.Drawing

function New-RoundedPath {
    param([System.Drawing.RectangleF]$Rect, [float]$Radius)
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = [Math]::Min($Radius * 2, [Math]::Min($Rect.Width, $Rect.Height))
    $arc = [System.Drawing.RectangleF]::new($Rect.X, $Rect.Y, $d, $d)
    $path.AddArc($arc, 180, 90)
    $arc.X = $Rect.Right - $d
    $path.AddArc($arc, 270, 90)
    $arc.Y = $Rect.Bottom - $d
    $path.AddArc($arc, 0, 90)
    $arc.X = $Rect.X
    $path.AddArc($arc, 90, 90)
    $path.CloseFigure()
    return $path
}

function New-IconBitmap {
    param([int]$Size)
    $bmp = [System.Drawing.Bitmap]::new($Size, $Size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)

    $pad = $Size * 0.055
    $bg = [System.Drawing.RectangleF]::new($pad, $pad, $Size - 2 * $pad, $Size - 2 * $pad)
    $bgPath = New-RoundedPath -Rect $bg -Radius ($Size * 0.22)
    $bgBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(250, 32, 32, 32))
    $g.FillPath($bgBrush, $bgPath)
    $bgBrush.Dispose()
    $bgPath.Dispose()

    $accent = [System.Drawing.Color]::FromArgb(255, 76, 194, 255)
    $zoneGap = [Math]::Max(1.2, $Size * 0.055)
    $left = $Size * 0.23
    $top = $Size * 0.23
    $right = $Size * 0.77
    $bottom = $Size * 0.77
    $midX = ($left + $right) / 2
    $midY = ($top + $bottom) / 2

    $zones = @(
        ([System.Drawing.RectangleF]::new($left, $top, $midX - $left - $zoneGap / 2, $midY - $top - $zoneGap / 2)),
        ([System.Drawing.RectangleF]::new($midX + $zoneGap / 2, $top, $right - $midX - $zoneGap / 2, $midY - $top - $zoneGap / 2)),
        ([System.Drawing.RectangleF]::new($left, $midY + $zoneGap / 2, $midX - $left - $zoneGap / 2, $bottom - $midY - $zoneGap / 2)),
        ([System.Drawing.RectangleF]::new($midX + $zoneGap / 2, $midY + $zoneGap / 2, $right - $midX - $zoneGap / 2, $bottom - $midY - $zoneGap / 2))
    )
    $brush = [System.Drawing.SolidBrush]::new($accent)
    foreach ($zone in $zones) {
        $zp = New-RoundedPath -Rect $zone -Radius ([Math]::Max(1, $Size * 0.03))
        $g.FillPath($brush, $zp)
        $zp.Dispose()
    }
    $brush.Dispose()
    $g.Dispose()
    return $bmp
}

$sizes = @(16, 20, 24, 32, 48, 64, 128, 256)
$images = @()
foreach ($size in $sizes) {
    $bmp = New-IconBitmap -Size $size
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $bytes = $ms.ToArray()
    $ms.Dispose()
    $bmp.Dispose()
    $images += ,@($size, $bytes)
}

$dir = Split-Path -Parent $OutputPath
if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }

$fs = [System.IO.File]::Create($OutputPath)
$bw = [System.IO.BinaryWriter]::new($fs)
$bw.Write([UInt16]0)
$bw.Write([UInt16]1)
$bw.Write([UInt16]$images.Count)
$offset = 6 + 16 * $images.Count
foreach ($entry in $images) {
    $size = [int]$entry[0]
    $bytes = [byte[]]$entry[1]
    $bw.Write([byte]($(if ($size -ge 256) { 0 } else { $size })))
    $bw.Write([byte]($(if ($size -ge 256) { 0 } else { $size })))
    $bw.Write([byte]0)
    $bw.Write([byte]0)
    $bw.Write([UInt16]1)
    $bw.Write([UInt16]32)
    $bw.Write([UInt32]$bytes.Length)
    $bw.Write([UInt32]$offset)
    $offset += $bytes.Length
}
foreach ($entry in $images) {
    $bw.Write([byte[]]$entry[1])
}
$bw.Flush()
$bw.Close()
$fs.Close()

Write-Output ("Icon created: {0} ({1} bytes)" -f (Resolve-Path $OutputPath), (Get-Item $OutputPath).Length)

