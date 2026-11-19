param(
    [int]$Size = 1024,
    [string]$Destination = 'assets\icon-source.png'
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$outPath = if ([System.IO.Path]::IsPathRooted($Destination)) { $Destination } else { Join-Path $root $Destination }

function Px([double]$value) { return [single]($value * $Size) }

$bitmap = [System.Drawing.Bitmap]::new($Size, $Size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)

try {
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $graphics.Clear([System.Drawing.Color]::Transparent)

    $inset = Px 0.015
    $radius = Px 0.22
    $inner = [single]((Px 1) - (2.0 * $inset))
    $backgroundRect = [System.Drawing.RectangleF]::new($inset, $inset, $inner, $inner)

    $backgroundPath = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $diameter = [single]($radius * 2.0)
    $backgroundPath.AddArc($backgroundRect.X, $backgroundRect.Y, $diameter, $diameter, 180, 90)
    $backgroundPath.AddArc($backgroundRect.Right - $diameter, $backgroundRect.Y, $diameter, $diameter, 270, 90)
    $backgroundPath.AddArc($backgroundRect.Right - $diameter, $backgroundRect.Bottom - $diameter, $diameter, $diameter, 0, 90)
    $backgroundPath.AddArc($backgroundRect.X, $backgroundRect.Bottom - $diameter, $diameter, $diameter, 90, 90)
    $backgroundPath.CloseFigure()

    $colorTop = [System.Drawing.Color]::FromArgb(255, 36, 52, 71)
    $colorBottom = [System.Drawing.Color]::FromArgb(255, 14, 22, 32)
    $backgroundBrush = [System.Drawing.Drawing2D.LinearGradientBrush]::new($backgroundRect, $colorTop, $colorBottom, [single]90.0)
    $graphics.FillPath($backgroundBrush, $backgroundPath)

    $edgeColor = [System.Drawing.Color]::FromArgb(42, 255, 255, 255)
    $edgePen = [System.Drawing.Pen]::new($edgeColor, (Px 0.006))
    $graphics.DrawPath($edgePen, $backgroundPath)

    $cyan = [System.Drawing.Color]::FromArgb(255, 34, 211, 238)
    $cyanSoft = [System.Drawing.Color]::FromArgb(150, 34, 211, 238)
    $rippleX = Px 0.705
    $rippleY = Px 0.285

    $ring1 = [System.Drawing.Pen]::new($cyan, (Px 0.055))
    $ring1.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $ring1.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $ring2 = [System.Drawing.Pen]::new($cyanSoft, (Px 0.04))
    $ring2.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $ring2.EndCap = [System.Drawing.Drawing2D.LineCap]::Round

    $graphics.DrawEllipse($ring1, ($rippleX - (Px 0.095)), ($rippleY - (Px 0.095)), (Px 0.19), (Px 0.19))
    $graphics.DrawEllipse($ring2, ($rippleX - (Px 0.175)), ($rippleY - (Px 0.175)), (Px 0.35), (Px 0.35))

    $tipX = Px 0.285
    $tipY = Px 0.275
    $scale = Px 0.41

    $points = [System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(($tipX + 0.000 * $scale), ($tipY + 0.000 * $scale)),
        [System.Drawing.PointF]::new(($tipX + 0.000 * $scale), ($tipY + 1.000 * $scale)),
        [System.Drawing.PointF]::new(($tipX + 0.281 * $scale), ($tipY + 0.719 * $scale)),
        [System.Drawing.PointF]::new(($tipX + 0.469 * $scale), ($tipY + 1.125 * $scale)),
        [System.Drawing.PointF]::new(($tipX + 0.625 * $scale), ($tipY + 1.063 * $scale)),
        [System.Drawing.PointF]::new(($tipX + 0.438 * $scale), ($tipY + 0.656 * $scale)),
        [System.Drawing.PointF]::new(($tipX + 0.750 * $scale), ($tipY + 0.656 * $scale))
    )

    $arrowColor = [System.Drawing.Color]::FromArgb(255, 8, 13, 20)
    $arrowPen = [System.Drawing.Pen]::new($arrowColor, (Px 0.030))
    $arrowPen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $graphics.FillPolygon([System.Drawing.Brushes]::White, $points)
    $graphics.DrawPolygon($arrowPen, $points)
}
finally {
    $graphics.Dispose()
}

$destinationDir = [System.IO.Path]::GetDirectoryName($outPath)
if (-not (Test-Path -LiteralPath $destinationDir)) {
    New-Item -ItemType Directory -Force -Path $destinationDir | Out-Null
}
$bitmap.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
$bitmap.Dispose()
Write-Host "Wrote $outPath"
