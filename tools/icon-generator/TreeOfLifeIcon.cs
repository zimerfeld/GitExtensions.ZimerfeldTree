// TreeOfLifeIcon.cs — Programmatically-rendered "Árvore da Vida" icon for ZimerfeldTree.
// Licensed under CC BY-NC-ND 4.0 — Copyright (c) 2026 Zimerfeld

using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace GitExtensions.ZimerfeldTree;

/// <summary>
/// Generates the ZimerfeldTree "Árvore da Vida" (Tree of Life) icon at runtime using GDI+.
/// The design features a symmetric sacred tree — trunk, three levels of branches and two
/// levels of roots — inside a circle, rendered in forest green with gold leaf/fruit tips.
/// No external resources required; the icon is drawn fresh from vector coordinates.
/// </summary>
internal static class TreeOfLifeIcon
{
    // Lazily created, cached for the process lifetime.
    private static readonly Lazy<Image> _menu = new(() => Render(16));
    private static readonly Lazy<Icon>  _form = new(BuildIcon);

    /// <summary>
    /// 16×16 <see cref="Image"/> for the GitExtensions plugin-menu entry
    /// (<c>GitPluginBase.Icon</c>).
    /// </summary>
    public static Image ForMenu() => _menu.Value;

    /// <summary>
    /// Multi-size <see cref="Icon"/> (32 px + 16 px, PNG-encoded ICO) for the
    /// form title bar and Windows task-bar (<c>Form.Icon</c>).
    /// </summary>
    public static Icon ForForm() => _form.Value;

    // ── Rendering ─────────────────────────────────────────────────────────────
    // All design coordinates are in a 32-unit space; scaled by (sz/32) at render time.

    private static Bitmap Render(int sz)
    {
        var bmp = new Bitmap(sz, sz, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode   = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(Color.Transparent);

        float s = sz / 32f;

        var green = Color.FromArgb(0x14, 0x5A, 0x29);  // deep forest green
        var gold  = Color.FromArgb(0xD4, 0xA0, 0x17);  // warm gold
        var bg    = Color.FromArgb(0xE8, 0xF5, 0xE9);  // pale green background

        // ── Outer circle ──────────────────────────────────────────────────────
        using (var bb = new SolidBrush(bg))
            g.FillEllipse(bb, S(2, s), S(2, s), S(28, s), S(28, s));

        using (var cp = new Pen(green, 1.5f * s))
            g.DrawEllipse(cp, S(2, s), S(2, s), S(28, s), S(28, s));

        // ── Trunk ─────────────────────────────────────────────────────────────
        using (var tp = RoundPen(green, 2f * s))
            g.DrawLine(tp, Pt(16, 27, s), Pt(16, 5, s));

        // ── Branches (3 levels + apical split) ────────────────────────────────
        using var b0 = RoundPen(green, 1.5f * s);
        using var b1 = RoundPen(green, 1.2f * s);
        using var b2 = RoundPen(green, 0.9f * s);

        // Level 1 — widest (attach point: trunk @ y=23)
        g.DrawLine(b0, Pt(16, 23, s), Pt( 7, 17, s));
        g.DrawLine(b0, Pt(16, 23, s), Pt(25, 17, s));
        // Level 2 — middle (trunk @ y=18)
        g.DrawLine(b1, Pt(16, 18, s), Pt( 8, 12, s));
        g.DrawLine(b1, Pt(16, 18, s), Pt(24, 12, s));
        // Level 3 — upper (trunk @ y=13)
        g.DrawLine(b2, Pt(16, 13, s), Pt(11,  8, s));
        g.DrawLine(b2, Pt(16, 13, s), Pt(21,  8, s));
        // Apical split (trunk @ y=8)
        g.DrawLine(b2, Pt(16,  8, s), Pt(13,  5, s));
        g.DrawLine(b2, Pt(16,  8, s), Pt(19,  5, s));

        // ── Roots (2 levels, shallow, stay inside circle) ─────────────────────
        using var r1 = RoundPen(green, 1.2f * s);
        using var r2 = RoundPen(green, 0.9f * s);

        g.DrawLine(r1, Pt(16, 27, s), Pt(11, 29, s));
        g.DrawLine(r1, Pt(16, 27, s), Pt(21, 29, s));
        g.DrawLine(r2, Pt(16, 26, s), Pt(13, 29, s));
        g.DrawLine(r2, Pt(16, 26, s), Pt(19, 29, s));

        // ── Leaf / fruit dots (gold) at branch tips ────────────────────────────
        using var lb = new SolidBrush(gold);

        float lr = 1.5f * s;
        PointF[] leafTips =
        [
            Pt( 7, 17, s), Pt(25, 17, s),   // level 1
            Pt( 8, 12, s), Pt(24, 12, s),   // level 2
            Pt(11,  8, s), Pt(21,  8, s),   // level 3
            Pt(13,  5, s), Pt(19,  5, s),   // apical
        ];
        foreach (var lp in leafTips)
            g.FillEllipse(lb, lp.X - lr, lp.Y - lr, 2 * lr, 2 * lr);

        // Smaller fruit dots at root tips
        float rr = 1.0f * s;
        PointF[] rootTips =
        [
            Pt(11, 29, s), Pt(21, 29, s),
            Pt(13, 29, s), Pt(19, 29, s),
        ];
        foreach (var rp in rootTips)
            g.FillEllipse(lb, rp.X - rr, rp.Y - rr, 2 * rr, 2 * rr);

        // ── Central "heart of life" fruit ──────────────────────────────────────
        float cr = 2.2f * s;
        using var fb = new SolidBrush(gold);
        using var fp = new Pen(green, 0.7f * s);
        g.FillEllipse(fb, S(16, s) - cr, S(15, s) - cr, 2 * cr, 2 * cr);
        g.DrawEllipse(fp, S(16, s) - cr, S(15, s) - cr, 2 * cr, 2 * cr);

        return bmp;
    }

    // ── Multi-size ICO packer ─────────────────────────────────────────────────

    private static Icon BuildIcon()
    {
        byte[] b32 = RenderToPng(32);
        byte[] b16 = RenderToPng(16);

        using var ms = new MemoryStream();
        using var w  = new BinaryWriter(ms);

        // ICO header: reserved=0, type=1 (ICO), count=2
        w.Write((short)0);
        w.Write((short)1);
        w.Write((short)2);

        // Directory entries (offset = header(6) + 2×entry(16) = 38)
        int off = 6 + 2 * 16;
        WriteIcoEntry(w, 32, b32.Length, off);
        WriteIcoEntry(w, 16, b16.Length, off + b32.Length);

        // PNG-encoded image data (Vista+ ICO format)
        w.Write(b32);
        w.Write(b16);

        ms.Position = 0;
        return new Icon(ms);
    }

    private static byte[] RenderToPng(int size)
    {
        using var bmp = Render(size);
        using var ms  = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    private static void WriteIcoEntry(BinaryWriter w, int size, int dataLen, int offset)
    {
        w.Write((byte)(size > 255 ? 0 : size));  // width  (0 means 256)
        w.Write((byte)(size > 255 ? 0 : size));  // height
        w.Write((byte)0);                         // color count (0 = true-color)
        w.Write((byte)0);                         // reserved
        w.Write((short)1);                        // color planes
        w.Write((short)32);                       // bits per pixel
        w.Write(dataLen);                         // image data size (bytes)
        w.Write(offset);                          // image data offset from file start
    }

    // ── Geometry helpers ──────────────────────────────────────────────────────

    /// <summary>Scales a design-space coordinate to render-space.</summary>
    private static float S(float v, float scale) => v * scale;

    /// <summary>Creates a scaled design-space <see cref="PointF"/>.</summary>
    private static PointF Pt(float x, float y, float scale) => new(x * scale, y * scale);

    /// <summary>Creates a <see cref="Pen"/> with round start and end caps.</summary>
    private static Pen RoundPen(Color color, float width) =>
        new(color, width) { StartCap = LineCap.Round, EndCap = LineCap.Round };
}
