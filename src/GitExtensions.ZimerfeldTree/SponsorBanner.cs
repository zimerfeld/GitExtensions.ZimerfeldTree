// SponsorBanner.cs — top-of-window GitHub Sponsors badge shared by all ZimerfeldTree windows
// Licensed under CC BY-NC-ND 4.0 — Copyright (c) 2026 Zimerfeld

using System.Diagnostics;

namespace GitExtensions.ZimerfeldTree;

/// <summary>
/// Builds the clickable GitHub Sponsors banner placed at the top of every plugin window. The badge
/// is a baked PNG copy of the shields.io "for-the-badge" SVG (WinForms cannot render SVG natively);
/// clicking it opens the sponsors page in the default browser.
/// </summary>
internal static class SponsorBanner
{
    /// <summary>Height of the docked top panel; windows grow by this to make room for it.</summary>
    public const int PanelHeight = 40;

    private const string SponsorUrl    = "https://github.com/sponsors/zimerfeld";
    private const string BadgeResource = "GitExtensions.ZimerfeldTree.Resources.sponsor-badge.png";

    private const string KofiUrl       = "https://ko-fi.com/C0D621FCGD";
    private const string KofiResource  = "GitExtensions.ZimerfeldTree.Resources.kofi-badge.png";

    // Display size of the badge (its native aspect is 198×28).
    private static readonly Size BadgeSize = new(198, 28);

    // Horizontal gap between the sponsor badge and the Ko-fi badge.
    private const int BadgeGap = 12;

    private static readonly ToolTip Tip = new();

    /// <summary>
    /// Creates a top-docked panel with the badge centered horizontally and click-to-open wired up.
    /// When <paramref name="aboutLink"/> is supplied it is hosted at the right edge of the banner,
    /// vertically aligned with the sponsor badge (so every window's "About" link sits at the same
    /// height as <c>picSponsor</c>). The caller keeps the reference (e.g. for live re-localization)
    /// and wires its click handler before passing it in.
    /// </summary>
    public static Panel Create(LinkLabel? aboutLink = null)
    {
        var panel = new Panel { Name = "sponsorPanel", Dock = DockStyle.Top, Height = PanelHeight };

        var pic = new PictureBox
        {
            Name     = "picSponsor",
            Size     = BadgeSize,
            SizeMode = PictureBoxSizeMode.Zoom,
            Cursor   = Cursors.Hand
        };
        if (LoadImage(BadgeResource) is { } img) pic.Image = img;

        // Only the badge itself opens the sponsors page — clicks on the surrounding panel do nothing.
        void OpenSponsor(object? _, EventArgs __) => OpenUrl(SponsorUrl);
        pic.Click += OpenSponsor;
        Tip.SetToolTip(pic, SponsorUrl);

        panel.Controls.Add(pic);

        // Ko-fi "Buy me a coffee" badge, same display size, sitting just to the right of the sponsor badge.
        var picKofi = new PictureBox
        {
            Name     = "picKofi",
            Size     = BadgeSize,
            SizeMode = PictureBoxSizeMode.Zoom,
            Cursor   = Cursors.Hand
        };
        if (LoadImage(KofiResource) is { } kofiImg) picKofi.Image = kofiImg;

        void OpenKofi(object? _, EventArgs __) => OpenUrl(KofiUrl);
        picKofi.Click += OpenKofi;
        Tip.SetToolTip(picKofi, KofiUrl);

        panel.Controls.Add(picKofi);

        if (aboutLink is not null)
        {
            aboutLink.AutoSize  = true;
            aboutLink.TextAlign = ContentAlignment.MiddleRight;
            panel.Controls.Add(aboutLink);
            aboutLink.BringToFront();
        }

        void Layout()
        {
            // Center the sponsor + Ko-fi badges as one group (badge | gap | badge).
            int groupWidth = pic.Width + BadgeGap + picKofi.Width;
            int startX = (panel.ClientSize.Width - groupWidth) / 2;
            pic.Location = new Point(
                startX,
                (panel.ClientSize.Height - pic.Height) / 2);
            picKofi.Location = new Point(
                startX + pic.Width + BadgeGap,
                (panel.ClientSize.Height - picKofi.Height) / 2);
            if (aboutLink is not null)
                // Right-aligned with an 8 px margin; AutoSize keeps the full text visible.
                aboutLink.Location = new Point(
                    panel.ClientSize.Width - aboutLink.Width - 8,
                    (panel.ClientSize.Height - aboutLink.Height) / 2);
        }
        panel.Resize += (_, _) => Layout();
        // Re-align when the link text changes width (e.g. after a language switch) so the whole,
        // right-aligned text stays on screen.
        if (aboutLink is not null) aboutLink.SizeChanged += (_, _) => Layout();
        Layout();

        return panel;
    }

    private static Image? LoadImage(string resourceName)
    {
        try
        {
            using var s = typeof(SponsorBanner).Assembly.GetManifestResourceStream(resourceName);
            if (s is null) return null;
            using var tmp = new Bitmap(s);
            return new Bitmap(tmp);   // independent copy so the stream can be disposed safely
        }
        catch { return null; }
    }

    private static void OpenUrl(string url)
    {
        try { Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true }); }
        catch { /* opening the browser is best-effort */ }
    }
}
