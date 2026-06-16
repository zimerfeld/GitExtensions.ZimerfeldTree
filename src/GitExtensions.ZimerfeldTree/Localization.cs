// Localization.cs — runtime language dictionaries for the ZimerfeldTree windows
// MIT License — Copyright (c) 2026 Zimerfeld

using System.Globalization;
using System.Text.Json;

namespace GitExtensions.ZimerfeldTree;

/// <summary>User-selectable UI language. <see cref="Automatic"/> follows the OS UI culture.</summary>
public enum AppLanguage { Automatic, English, Portuguese }

/// <summary>
/// Loads per-window text dictionaries (embedded JSON resources, one file per window per language)
/// and persists the user's language choice. The choice is read from disk only once, at process
/// start (the first screen load); thereafter it is driven in-memory by the Language dropdown.
/// </summary>
public static class I18n
{
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GitExtensions", "ZimerfeldTree.language.json");

    // Initialized once, at first access — this is the single disk read of the persisted choice.
    private static AppLanguage _current = LoadChoice();

    /// <summary>The in-memory language choice (read from disk only on first load).</summary>
    public static AppLanguage Current => _current;

    /// <summary>Updates the in-memory choice and persists it to disk.</summary>
    public static void SetLanguage(AppLanguage lang)
    {
        _current = lang;
        SaveChoice(lang);
    }

    /// <summary>Resolves the active choice to a concrete culture code ("en-US" / "pt-BR").</summary>
    public static string Culture => CultureOf(_current);

    /// <summary>Resolves an explicit language to a concrete culture code ("en-US" / "pt-BR").</summary>
    public static string CultureOf(AppLanguage lang) => lang switch
    {
        AppLanguage.English    => "en-US",
        AppLanguage.Portuguese => "pt-BR",
        _                      => AutoCulture(),
    };

    private static string AutoCulture() =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("pt", StringComparison.OrdinalIgnoreCase)
            ? "pt-BR" : "en-US";

    /// <summary>
    /// Loads the dictionary for a window (<paramref name="scope"/> = "ZimerfeldTree" /
    /// "ZimerfeldGitFlow" / "ZimerfeldRestore") in the active language, falling back to en-US then
    /// to an empty map (in which case <see cref="Translator"/> echoes each key back).
    /// </summary>
    public static Translator Load(string scope) => Load(scope, _current);

    /// <summary>
    /// Same as <see cref="Load(string)"/> but for an explicit <paramref name="lang"/> rather than the
    /// global choice — lets each window display and persist its own language independently.
    /// </summary>
    public static Translator Load(string scope, AppLanguage lang)
    {
        var map = ReadEmbedded($"{scope}.{CultureOf(lang)}.json")
               ?? ReadEmbedded($"{scope}.en-US.json")
               ?? new Dictionary<string, string>();
        return new Translator(map);
    }

    private static Dictionary<string, string>? ReadEmbedded(string fileName)
    {
        try
        {
            // Matches the default resource-name convention used elsewhere (NodeIcons): the
            // RootNamespace + folder path of the embedded file.
            string logicalName = $"GitExtensions.ZimerfeldTree.Resources.{fileName}";
            using var s = typeof(I18n).Assembly.GetManifestResourceStream(logicalName);
            if (s is null) return null;
            using var reader = new StreamReader(s);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd());
        }
        catch { return null; }
    }

    private static AppLanguage LoadChoice()
    {
        try
        {
            if (!File.Exists(ConfigPath)) return AppLanguage.Automatic;
            using var doc = JsonDocument.Parse(File.ReadAllText(ConfigPath));
            if (doc.RootElement.TryGetProperty("language", out var v)
                && Enum.TryParse<AppLanguage>(v.GetString(), out var lang))
                return lang;
        }
        catch { /* fall through to default */ }
        return AppLanguage.Automatic;
    }

    private static void SaveChoice(AppLanguage lang)
    {
        try
        {
            string dir = Path.GetDirectoryName(ConfigPath)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(ConfigPath, $"{{\"language\":\"{lang}\"}}");
        }
        catch { /* persistence is best-effort */ }
    }
}

/// <summary>Holds one window's loaded strings; returns the key itself when a key is missing.</summary>
public sealed class Translator
{
    private readonly Dictionary<string, string> _map;
    public Translator(Dictionary<string, string> map) => _map = map;

    /// <summary>Translated string for <paramref name="key"/>, or the key itself if absent.</summary>
    public string this[string key] => _map.TryGetValue(key, out var v) ? v : key;

    /// <summary><see cref="string.Format(string, object?[])"/> applied to the translated string.</summary>
    public string F(string key, params object?[] args) => string.Format(this[key], args);
}
