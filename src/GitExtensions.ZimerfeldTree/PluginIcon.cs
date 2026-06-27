// PluginIcon.cs — Loads the embedded ZimerfeldTree icon (Resources\ico.png).
// Licensed under CC BY-NC-ND 4.0 — Copyright (c) 2026 Zimerfeld

namespace GitExtensions.ZimerfeldTree;

/// <summary>
/// Provides the plugin/window icon from the embedded <c>Resources\ico.png</c> (16×16).
/// Loaded once and cached for the process lifetime — no per-call work.
/// </summary>
internal static class PluginIcon
{
    private static readonly Lazy<Image> _image = new(LoadImage);
    private static readonly Lazy<Icon>  _icon  = new(BuildIcon);

    /// <summary>16×16 <see cref="Image"/> for the GitExtensions plugin-menu entry.</summary>
    public static Image ForMenu() => _image.Value;

    /// <summary><see cref="Icon"/> for form title bars and the Windows task-bar.</summary>
    public static Icon ForForm() => _icon.Value;

    private static Image LoadImage()
    {
        var asm          = typeof(PluginIcon).Assembly;
        var resourceName = $"{typeof(PluginIcon).Namespace}.Resources.ico.png";
        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
        // Copy into an independent bitmap so it doesn't depend on the stream staying open.
        using var src = new Bitmap(stream);
        return new Bitmap(src);
    }

    private static Icon BuildIcon()
    {
        // Form.Icon needs an Icon; derive one from the loaded bitmap. The HICON is created
        // once and kept for the process lifetime, so no handle churn.
        var hicon = ((Bitmap)_image.Value).GetHicon();
        return Icon.FromHandle(hicon);
    }
}
