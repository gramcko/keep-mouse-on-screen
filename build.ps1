$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$src = Join-Path $root 'src\TouchMonitor.cs'
$outDir = Join-Path $root 'dist'
$out = Join-Path $outDir 'TouchMonitor.exe'

$cscCandidates = @(
    (Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'),
    (Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe')
)
$csc = $cscCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $csc) { throw 'csc.exe (.NET Framework 4.x) not found.' }
if (-not (Test-Path -LiteralPath $src)) { throw "Source not found: $src" }

New-Item -ItemType Directory -Force -Path $outDir | Out-Null

$refs = @(
    '/r:System.dll',
    '/r:System.Drawing.dll',
    '/r:System.Windows.Forms.dll'
)

& $csc /nologo /target:winexe /optimize+ /platform:anycpu "/out:$out" $refs $src
if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE" }

Write-Host "Built: $out"
