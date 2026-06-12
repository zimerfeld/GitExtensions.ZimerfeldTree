// SponsorBanner.cs — top-of-window GitHub Sponsors badge shared by all ZimerfeldTree windows
// MIT License — Copyright (c) 2026 Zimerfeld

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

    // Display size of the badge (its native aspect is 198×28).
    private static readonly Size BadgeSize = new(198, 28);

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
        if (LoadBadge() is { } img) pic.Image = img;

        // Only the badge itself opens the sponsors page — clicks on the surrounding panel do nothing.
        void Open(object? _, EventArgs __) => OpenSponsors();
        pic.Click += Open;
        Tip.SetToolTip(pic, SponsorUrl);

        panel.Controls.Add(pic);

        if (aboutLink is not null)
        {
            aboutLink.AutoSize  = true;
            aboutLink.TextAlign = ContentAlignment.MiddleRight;
            panel.Controls.Add(aboutLink);
            aboutLink.BringToFront();
        }

        void Layout()
        {
            pic.Location = new Point(
                (panel.ClientSize.Width  - pic.Width)  / 2,
                (panel.ClientSize.Height - pic.Height) / 2);
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

    private static Image? LoadBadge()
    {
        try
        {
            using var s = typeof(SponsorBanner).Assembly.GetManifestResourceStream(BadgeResource);
            if (s is null) return null;
            using var tmp = new Bitmap(s);
            return new Bitmap(tmp);   // independent copy so the stream can be disposed safely
        }
        catch { return null; }
    }

    private static void OpenSponsors()
    {
        try { Process.Start(new ProcessStartInfo { FileName = SponsorUrl, UseShellExecute = true }); }
        catch { /* opening the browser is best-effort */ }
    }
}
