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

    /// <summary>Creates a top-docked panel with the badge centered horizontally and click-to-open wired up.</summary>
    public static Panel Create()
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

        void Open(object? _, EventArgs __) => OpenSponsors();
        pic.Click   += Open;
        panel.Click += Open;   // clicking the panel margin around the badge also opens the link
        Tip.SetToolTip(pic, SponsorUrl);

        panel.Controls.Add(pic);
        void Centre() => pic.Location = new Point(
            (panel.ClientSize.Width  - pic.Width)  / 2,
            (panel.ClientSize.Height - pic.Height) / 2);
        panel.Resize += (_, _) => Centre();
        Centre();

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
