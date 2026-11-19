param(
    [Parameter(Mandatory = $true)][string]$Source,
    [Parameter(Mandatory = $true)][string]$Destination
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$sizes = @(16, 20, 24, 32, 40, 48, 64, 128, 256)

function New-IcoDib([System.Drawing.Bitmap]$Bitmap, [int]$Size) {
    $rect = [System.Drawing.Rectangle]::new(0, 0, $Size, $Size)
    $data = $Bitmap.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadOnly, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $stride = $data.Stride
    $pixels = New-Object byte[] ($stride * $Size)
    [System.Runtime.InteropServices.Marshal]::Copy($data.Scan0, $pixels, 0, $pixels.Length)
    $Bitmap.UnlockBits($data)

    $maskRowBytes = [int]([math]::Ceiling($Size / 32.0)) * 4
    $maskSize = $maskRowBytes * $Size
    $xorSize = $Size * $Size * 4

    $stream = [System.IO.MemoryStream]::new()
    $writer = [System.IO.BinaryWriter]::new($stream)

    $writer.Write([uint32]40)
    $writer.Write([int32]$Size)
    $writer.Write([int32]($Size * 2))
    $writer.Write([uint16]1)
    $writer.Write([uint16]32)
    $writer.Write([uint32]0)
    $writer.Write([uint32]($xorSize + $maskSize))
    $writer.Write([int32]0)
    $writer.Write([int32]0)
    $writer.Write([uint32]0)
    $writer.Write([uint32]0)

    for ($y = $Size - 1; $y -ge 0; $y--) {
        $writer.Write([byte[]]$pixels, $y * $stride, $Size * 4)
    }

    $mask = New-Object byte[] $maskSize
    $writer.Write([byte[]]$mask, 0, $mask.Length)
    $writer.Flush()

    $result = $stream.ToArray()
    $writer.Dispose()
    $stream.Dispose()
    return $result
}

$sourceImage = [System.Drawing.Image]::FromFile((Resolve-Path -LiteralPath $Source))
$images = New-Object System.Collections.Generic.List[byte[]]

try {
    foreach ($size in $sizes) {
        $bitmap = [System.Drawing.Bitmap]::new($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        try {
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
            $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
            $graphics.Clear([System.Drawing.Color]::Transparent)
            $graphics.DrawImage($sourceImage, 0, 0, $size, $size)
        }
        finally {
            $graphics.Dispose()
        }

        $images.Add((New-IcoDib $bitmap $size))
        $bitmap.Dispose()
    }
}
finally {
    $sourceImage.Dispose()
}

$headerSize = 6
$directorySize = 16 * $sizes.Count
$offset = $headerSize + $directorySize

$output = [System.IO.MemoryStream]::new()
$writer = [System.IO.BinaryWriter]::new($output)
try {
    $writer.Write([uint16]0)
    $writer.Write([uint16]1)
    $writer.Write([uint16]$sizes.Count)

    for ($i = 0; $i -lt $sizes.Count; $i++) {
        $size = $sizes[$i]
        $dimension = if ($size -ge 256) { [byte]0 } else { [byte]$size }
        $data = $images[$i]
        $writer.Write([byte]$dimension)
        $writer.Write([byte]$dimension)
        $writer.Write([byte]0)
        $writer.Write([byte]0)
        $writer.Write([uint16]1)
        $writer.Write([uint16]32)
        $writer.Write([uint32]$data.Length)
        $writer.Write([uint32]$offset)
        $offset += $data.Length
    }

    foreach ($data in $images) {
        $writer.Write([byte[]]$data)
    }
    $writer.Flush()
    $bytes = $output.ToArray()
}
finally {
    $writer.Dispose()
    $output.Dispose()
}

$destinationPath = [System.IO.Path]::GetFullPath($Destination)
$destinationDir = [System.IO.Path]::GetDirectoryName($destinationPath)
if (-not (Test-Path -LiteralPath $destinationDir)) {
    New-Item -ItemType Directory -Force -Path $destinationDir | Out-Null
}
[System.IO.File]::WriteAllBytes($destinationPath, $bytes)
Write-Host "Wrote $destinationPath ($($bytes.Length) bytes, $($sizes.Count) sizes)"
