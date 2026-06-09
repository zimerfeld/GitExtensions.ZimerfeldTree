#Requires -Version 5.1
<#
.SYNOPSIS
    Renders the ZimerfeldTree context-menu "Restore" icon (a grey counter-clockwise
    "undo / go back" curved arrow) as a 16x16 PNG, matching the style of the other
    Resources\ctx-*.png menu glyphs (flat single-colour shape on a transparent canvas).

.DESCRIPTION
    Uses GDI+ (System.Drawing). The glyph is drawn in a high-resolution buffer
    (Size * Supersample) with anti-aliasing and then down-scaled to Size x Size so the
    16 px output is crisp. The grey colour mirrors the "↩ cinza" description used for the
    Restore item in README.md.

.PARAMETER Size
    Output edge length in pixels. Default 16 (the context-menu icon size).

.PARAMETER OutFile
    Destination PNG path. Default: ..\..\src\GitExtensions.ZimerfeldTree\Resources\ctx-restore.png

.EXAMPLE
    .\Generate-CtxRestoreIcon.ps1
#>
param(
    [int]   $Size    = 16,
    [string]$OutFile = "$PSScriptRoot\..\..\src\GitExtensions.ZimerfeldTree\Resources\ctx-restore.png"
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

# ── Render in a supersampled buffer, then downscale for crisp anti-aliasing ──
$Supersample = 8
$R = $Size * $Supersample            # working edge length
$s = $R / 16.0                       # 16-unit design space -> render space

# ── Palette ──────────────────────────────────────────────────────────────────
$grey = [System.Drawing.Color]::FromArgb(0x5F, 0x63, 0x68)   # neutral slate grey

$big = New-Object System.Drawing.Bitmap ($R, $R, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g   = [System.Drawing.Graphics]::FromImage($big)
$g.SmoothingMode   = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$g.Clear([System.Drawing.Color]::Transparent)

# ── Geometry (16-unit design space) ──────────────────────────────────────────
$cx = 8.0; $cy = 8.4          # arc centre (slightly low to balance the arrowhead)
$r  = 4.6                     # arc radius

function PtOnArc([double]$deg) {
    $t = [Math]::PI * $deg / 180.0
    New-Object System.Drawing.PointF (
        [single](($cx + $r * [Math]::Cos($t)) * $s),
        [single](($cy + $r * [Math]::Sin($t)) * $s))
}

$pen = New-Object System.Drawing.Pen ($grey, [single](1.7 * $s))
$pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$pen.EndCap   = [System.Drawing.Drawing2D.LineCap]::Round

# Counter-clockwise (undo) arc: start lower-right, sweep up-over-top to upper-left,
# leaving an open gap at the lower-right. GDI sweep is clockwise for positive angles,
# so a negative sweep draws counter-clockwise.
$startDeg = 40.0
$sweepDeg = -250.0
$x0 = [single](($cx - $r) * $s); $y0 = [single](($cy - $r) * $s); $d = [single](2 * $r * $s)
$g.DrawArc($pen, $x0, $y0, $d, $d, [single]$startDeg, [single]$sweepDeg)

# ── Arrowhead at the head of the arc (the swept end) ─────────────────────────
$endDeg  = $startDeg + $sweepDeg                 # -210 deg == 150 deg
$head    = PtOnArc $endDeg
$before  = PtOnArc ($endDeg + 12.0)              # a touch back along the path (theta decreasing)
# Unit vector along direction of travel (head - before)
$dx = $head.X - $before.X
$dy = $head.Y - $before.Y
$len = [Math]::Sqrt($dx * $dx + $dy * $dy)
if ($len -lt 1e-3) { $len = 1.0 }
$ux = $dx / $len; $uy = $dy / $len               # forward
$px = -$uy;       $py = $ux                       # perpendicular

$tip  = 2.4 * $s                                  # how far the point extends past the arc end
$back = 1.4 * $s                                  # base offset behind the arc end
$wing = 3.2 * $s                                  # half-width of the arrowhead base

$ptTip  = New-Object System.Drawing.PointF ([single]($head.X + $ux * $tip),  [single]($head.Y + $uy * $tip))
$baseCx = $head.X - $ux * $back; $baseCy = $head.Y - $uy * $back
$ptL    = New-Object System.Drawing.PointF ([single]($baseCx + $px * $wing), [single]($baseCy + $py * $wing))
$ptR    = New-Object System.Drawing.PointF ([single]($baseCx - $px * $wing), [single]($baseCy - $py * $wing))

# Filled triangular arrowhead (base covers the arc's round cap)
$tri = New-Object System.Drawing.Drawing2D.GraphicsPath
$tri.AddPolygon([System.Drawing.PointF[]]@($ptTip, $ptL, $ptR))
$fill = New-Object System.Drawing.SolidBrush $grey
$g.FillPath($fill, $tri)

# ── Downscale to final size ──────────────────────────────────────────────────
$out = New-Object System.Drawing.Bitmap ($Size, $Size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$go  = [System.Drawing.Graphics]::FromImage($out)
$go.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$go.PixelOffsetMode   = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$go.SmoothingMode     = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$go.Clear([System.Drawing.Color]::Transparent)
$go.DrawImage($big, (New-Object System.Drawing.Rectangle 0, 0, $Size, $Size))

# ── Save ─────────────────────────────────────────────────────────────────────
$OutFile = [System.IO.Path]::GetFullPath($OutFile)
$out.Save($OutFile, [System.Drawing.Imaging.ImageFormat]::Png)

$go.Dispose(); $out.Dispose(); $g.Dispose(); $big.Dispose()
foreach ($disp in @($pen, $fill, $tri)) { $disp.Dispose() }

Write-Host "Gerado: $OutFile  ($Size x $Size, $((Get-Item $OutFile).Length) bytes)"
