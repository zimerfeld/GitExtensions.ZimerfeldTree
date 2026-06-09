#Requires -Version 5.1
<#
.SYNOPSIS
    Renders the ZimerfeldTree "Árvore da Vida" (Tree of Life) icon at any size and
    saves it as a PNG, for use as the embedded NuGet package icon (<icon> in the nuspec).

.DESCRIPTION
    Faithful PowerShell/GDI+ port of TreeOfLifeIcon.Render() (recovered from git at
    commit 5bd5446, file src/GitExtensions.ZimerfeldTree/TreeOfLifeIcon.cs). The plugin's
    own 16×16 icon (Resources\ico.png, loaded by PluginIcon.cs) is unchanged — this script
    only produces a larger image with the identical vector design and colours.

    All design coordinates live in a 32-unit space and are scaled by (Size/32), so the
    output is crisp at any resolution with the exact same proportions.

.PARAMETER Size
    Output edge length in pixels. Default 256 (NuGet recommends >=128, <1 MB).

.PARAMETER OutFile
    Destination PNG path. Default: ..\..\src\GitExtensions.ZimerfeldTree\Resources\icon-128.png
    (kept under that name to match the existing <file> entry in the nuspec).

.EXAMPLE
    .\Generate-PackageIcon.ps1                 # 256×256 -> Resources\icon-128.png
    .\Generate-PackageIcon.ps1 -Size 128       # 128×128
#>
param(
    [int]   $Size    = 256,
    [string]$OutFile = "$PSScriptRoot\..\..\src\GitExtensions.ZimerfeldTree\Resources\icon-128.png"
)

# Containment factor: the tree/branch/root/leaf vector originally reaches the circle's
# ring (the lowest root dots poke past it). The green ring occupies design-radius
# [13.25 .. 14.75]; the furthest content point (a root dot) sits at radius ~14.93.
# Scaling all content by this factor about the circle centre pulls every graphic just
# inside the ring's inner edge (13.25) while keeping the exact design and proportions.
$Fit = 0.84

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

# ── Palette (identical to TreeOfLifeIcon.cs) ─────────────────────────────────
$green = [System.Drawing.Color]::FromArgb(0x14, 0x5A, 0x29)  # deep forest green
$gold  = [System.Drawing.Color]::FromArgb(0xD4, 0xA0, 0x17)  # warm gold
$bg    = [System.Drawing.Color]::FromArgb(0xE8, 0xF5, 0xE9)  # pale green background

$s = $Size / 32.0   # design-space -> render-space scale

# ── Geometry helpers (mirror S / Pt / RoundPen) ──────────────────────────────
function Pt([double]$x, [double]$y) { New-Object System.Drawing.PointF (($x * $s), ($y * $s)) }
function RoundPen([System.Drawing.Color]$c, [double]$w) {
    $p = New-Object System.Drawing.Pen ($c, [single]$w)
    $p.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $p.EndCap   = [System.Drawing.Drawing2D.LineCap]::Round
    $p
}

$bmp = New-Object System.Drawing.Bitmap ($Size, $Size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g   = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode   = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$g.Clear([System.Drawing.Color]::Transparent)

# ── Outer circle ─────────────────────────────────────────────────────────────
$bb = New-Object System.Drawing.SolidBrush $bg
$g.FillEllipse($bb, (2*$s), (2*$s), (28*$s), (28*$s))
$cp = New-Object System.Drawing.Pen ($green, [single](1.5*$s))
$g.DrawEllipse($cp, (2*$s), (2*$s), (28*$s), (28*$s))

# ── Contain all tree content inside the ring ─────────────────────────────────
# Scale every subsequent draw by $Fit about the circle centre (16,16 in design space).
$cx = [single](16 * $s); $cy = [single](16 * $s)
$g.TranslateTransform($cx, $cy)
$g.ScaleTransform([single]$Fit, [single]$Fit)
$g.TranslateTransform(-$cx, -$cy)

# ── Trunk ────────────────────────────────────────────────────────────────────
$tp = RoundPen $green (2.0*$s)
$g.DrawLine($tp, (Pt 16 27), (Pt 16 5))

# ── Branches (3 levels + apical split) ───────────────────────────────────────
$b0 = RoundPen $green (1.5*$s)
$b1 = RoundPen $green (1.2*$s)
$b2 = RoundPen $green (0.9*$s)
# Level 1 — widest
$g.DrawLine($b0, (Pt 16 23), (Pt  7 17))
$g.DrawLine($b0, (Pt 16 23), (Pt 25 17))
# Level 2 — middle
$g.DrawLine($b1, (Pt 16 18), (Pt  8 12))
$g.DrawLine($b1, (Pt 16 18), (Pt 24 12))
# Level 3 — upper
$g.DrawLine($b2, (Pt 16 13), (Pt 11  8))
$g.DrawLine($b2, (Pt 16 13), (Pt 21  8))
# Apical split
$g.DrawLine($b2, (Pt 16  8), (Pt 13  5))
$g.DrawLine($b2, (Pt 16  8), (Pt 19  5))

# ── Roots (2 levels, shallow) ────────────────────────────────────────────────
$r1 = RoundPen $green (1.2*$s)
$r2 = RoundPen $green (0.9*$s)
$g.DrawLine($r1, (Pt 16 27), (Pt 11 29))
$g.DrawLine($r1, (Pt 16 27), (Pt 21 29))
$g.DrawLine($r2, (Pt 16 26), (Pt 13 29))
$g.DrawLine($r2, (Pt 16 26), (Pt 19 29))

# ── Leaf / fruit dots (gold) at branch tips ──────────────────────────────────
$lb = New-Object System.Drawing.SolidBrush $gold
$lr = 1.5 * $s
$leafTips = @((Pt 7 17),(Pt 25 17),(Pt 8 12),(Pt 24 12),(Pt 11 8),(Pt 21 8),(Pt 13 5),(Pt 19 5))
foreach ($lp in $leafTips) { $g.FillEllipse($lb, ($lp.X - $lr), ($lp.Y - $lr), (2*$lr), (2*$lr)) }

# Smaller fruit dots at root tips
$rr = 1.0 * $s
$rootTips = @((Pt 11 29),(Pt 21 29),(Pt 13 29),(Pt 19 29))
foreach ($rp in $rootTips) { $g.FillEllipse($lb, ($rp.X - $rr), ($rp.Y - $rr), (2*$rr), (2*$rr)) }

# ── Central "heart of life" fruit ────────────────────────────────────────────
$cr = 2.2 * $s
$fb = New-Object System.Drawing.SolidBrush $gold
$fp = New-Object System.Drawing.Pen ($green, [single](0.7*$s))
$g.FillEllipse($fb, ((16*$s) - $cr), ((15*$s) - $cr), (2*$cr), (2*$cr))
$g.DrawEllipse($fp, ((16*$s) - $cr), ((15*$s) - $cr), (2*$cr), (2*$cr))

$g.ResetTransform()

# ── Save ─────────────────────────────────────────────────────────────────────
$OutFile = [System.IO.Path]::GetFullPath($OutFile)
$bmp.Save($OutFile, [System.Drawing.Imaging.ImageFormat]::Png)

$g.Dispose(); $bmp.Dispose()
foreach ($d in @($bb,$cp,$tp,$b0,$b1,$b2,$r1,$r2,$lb,$fb,$fp)) { $d.Dispose() }

Write-Host "Gerado: $OutFile  ($Size x $Size, $((Get-Item $OutFile).Length) bytes)"
