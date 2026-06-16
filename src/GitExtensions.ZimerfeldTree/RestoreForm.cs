// RestoreForm.cs — Git restore/cherry-pick/reset window for ZimerfeldTree plugin
// MIT License — Copyright (c) 2026 Zimerfeld

using System.Text.Json;

namespace GitExtensions.ZimerfeldTree;

/// <summary>
/// Modal window that drives git history-restoration operations: restoring a file from a commit,
/// cherry-picking commits onto the current branch, and resetting a branch to a prior commit.
/// </summary>
public sealed class RestoreForm : Form
{
    private readonly BranchHierarchyService _svc;
    private readonly bool    _showControlIds;
    private readonly ToolTip _mainTooltip = new ToolTip();

    // Loaded for the active language; reassigned by ApplyLanguage when the user switches languages
    // via the bottom-panel dropdown (so the open window re-localizes live, like ZimerfeldTree).
    private Translator _t = I18n.Load("ZimerfeldRestore");

    // (control, dictionary-key) pairs reapplied by ApplyLanguage so every label/button/tab re-localizes
    // in place when the Language dropdown changes.
    private readonly List<KeyValuePair<Control, string>> _localized = new();

    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GitExtensions", "ZimerfeldRestore.settings.json");

    // Left inset of the labels/radios inside a tab page; LayoutResponsive mirrors it on the right so
    // the input column has equal left/right margins regardless of window width or DPI.
    private const int SideMargin = 14;

    // ── Layout shell (docked) ──
    private Panel       _headerPanel = null!;
    private Panel       _bottomPanel = null!;
    private TabControl  _tabs        = null!;

    // ── Header ──
    private Label     _lblHead  = null!;
    private LinkLabel _lnkAbout = null!;

    // ── Plano de Emergência (restore/reset a branch to a tag) ──
    private ComboBox _cboEmergencyBranch = null!;
    private ComboBox _cboEmergencyTag    = null!;
    private Button   _btnEmergencyRestore = null!;
    private Button   _btnEmergencyReset  = null!;

    // ── Restore File ──
    private ComboBox _cboRestoreHash = null!;
    private TextBox  _txtRestoreFile = null!;
    private Button   _btnRestoreFile = null!;

    // ── Cherry-Pick ──
    private ComboBox _cboCherryHash = null!;
    private Button   _btnCherryPick = null!;

    // ── Reset Branch ──
    private ComboBox    _cboBranch   = null!;
    private ComboBox    _cboResetHash = null!;
    private RadioButton _rdMixed     = null!;
    private RadioButton _rdSoft      = null!;
    private RadioButton _rdHard      = null!;
    private Button      _btnReset    = null!;

    // ── Result (below the tabs, mirroring ZimerfeldGitFlow) ──
    private GroupBox _grpResult = null!;
    private TextBox  _txtResult = null!;

    // ── Bottom bar ──
    private Button   _btnClose     = null!;
    private CheckBox _chkShowDebug = null!;
    private Label    _lblLanguage  = null!;
    private ComboBox _cboLanguage  = null!;
    private bool     _suppressLangEvent;

    /// <summary>
    /// Raised after a restore operation mutates the repository so the owning ZimerfeldTree window
    /// can refresh its tree. The argument is the branch to reveal, or null to only refresh.
    /// </summary>
    public event Action<string?>? RepoMutated;

    public RestoreForm(BranchHierarchyService svc, bool showControlIds = false)
    {
        _svc            = svc;
        _showControlIds = showControlIds;

        Text            = _t["title"];
        // Width 830 (was 880): the hash dropdowns show "(YYYY-MM-dd HH:mm:ss) [branch] hash → message",
        // so the input column is wide. LayoutResponsive() stretches the combos/fields and right-aligns
        // the buttons at runtime so the right margin always equals the left (SideMargin), regardless of
        // DPI/border math; each combo's drop-down list is pinned to its field width so the open list
        // never spills past the right margin.
        Size            = new Size(830, 824 + SponsorBanner.PanelHeight);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        StartPosition   = FormStartPosition.Manual;
        Font            = new Font("Segoe UI", 9f);
        Icon            = PluginIcon.ForForm();

        // Docked shell (6 px padding → symmetric borders): a short tab control on top holding the
        // input groups, and the result group filling the space below the tabs (like ZimerfeldGitFlow),
        // with the sponsor banner on top and a centered close-button bar below.
        var content = new Panel { Name = "contentPanel", Dock = DockStyle.Fill, Padding = new Padding(6, 4, 6, 6) };
        _tabs = new TabControl { Name = "tabs", Dock = DockStyle.Top, Height = 196 };

        BuildHeader();
        BuildEmergencyTab();
        BuildRestoreFileTab();
        BuildCherryPickTab();
        BuildResetTab();
        BuildResultGroup();   // grpResult fills the area below the tabs (not a tab)
        BuildCloseButton();

        // Add back-to-front so Dock resolves: Fill first (backmost), then the tabs, header last (top).
        content.Controls.Add(_grpResult);     // Fill — result box below the tabs
        content.Controls.Add(_tabs);          // Top
        content.Controls.Add(_headerPanel);   // Top

        Controls.Add(content);                // Fill — between banner and bottom bar
        Controls.Add(_bottomPanel);           // Bottom
        Controls.Add(SponsorBanner.Create(_lnkAbout)); // Top — Sponsors banner hosts lnkAbout

        CancelButton  = _btnClose;
        Load         += (_, _) =>
        {
            InitData();
            ApplyOrClearTooltips(_chkShowDebug.Checked);
            LayoutResponsive();
        };
        // Re-run once the tab control has its final (DPI-scaled) size so the margins stay equal.
        _tabs.ClientSizeChanged += (_, _) => LayoutResponsive();
        FormClosing  += (_, _) => SaveSettings();
    }

    /// <summary>
    /// Stretches the wide inputs to the right margin and right-aligns the action buttons so each tab's
    /// input column has equal left/right margins (<see cref="SideMargin"/>), independent of window
    /// width or DPI. Each combo's drop-down list width is pinned to its field width so the open list
    /// never extends past the right margin.
    /// </summary>
    private void LayoutResponsive()
    {
        if (_tabs.TabPages.Count == 0) return;

        void Stretch(Control c)
        {
            int right = c.Parent!.ClientSize.Width - SideMargin;
            c.Width = Math.Max(60, right - c.Left);
            if (c is ComboBox cb) cb.DropDownWidth = cb.Width;
        }
        void RightAlign(Control c) => c.Left = c.Parent!.ClientSize.Width - SideMargin - c.Width;

        // Emergency: stretch both combos; right-align the Reset button and tuck Restore to its left.
        Stretch(_cboEmergencyBranch);
        Stretch(_cboEmergencyTag);
        RightAlign(_btnEmergencyReset);
        _btnEmergencyRestore.Left = _btnEmergencyReset.Left - 12 - _btnEmergencyRestore.Width;

        // Restore File
        Stretch(_cboRestoreHash);
        Stretch(_txtRestoreFile);
        RightAlign(_btnRestoreFile);

        // Cherry-Pick
        Stretch(_cboCherryHash);
        RightAlign(_btnCherryPick);

        // Reset Branch
        Stretch(_cboBranch);
        Stretch(_cboResetHash);
        Stretch(_rdMixed);
        Stretch(_rdSoft);
        Stretch(_rdHard);
        RightAlign(_btnReset);
    }

    /// <summary>
    /// Creates a tab page (title localized via <paramref name="titleKey"/>) hosting its controls at
    /// their existing coords, and registers the page so its title re-localizes on language change.
    /// </summary>
    private TabPage AddTab(string titleKey, params Control[] controls)
    {
        var page = new TabPage(_t[titleKey]) { UseVisualStyleBackColor = true, Padding = new Padding(3) };
        page.Controls.AddRange(controls);
        _tabs.TabPages.Add(page);
        _localized.Add(new(page, titleKey));
        return page;
    }

    // ── Build UI ────────────────────────────────────────────────────────────

    private void BuildHeader()
    {
        _headerPanel = new Panel { Name = "headerPanel", Dock = DockStyle.Top, Height = 26 };

        // lnkAbout is hosted by the sponsor banner (aligned with picSponsor), not the header panel.
        _lnkAbout = new LinkLabel
        {
            Name     = "lnkAbout",
            Text     = _t["aboutLink"],
            AutoSize = true
        };
        _lnkAbout.LinkClicked += (_, _) => ShowAbout();
        _localized.Add(new(_lnkAbout, "aboutLink"));

        _lblHead = new Label
        {
            Name      = "lblHead",
            TextAlign = ContentAlignment.MiddleLeft,
            Dock      = DockStyle.Fill
        };

        _headerPanel.Controls.Add(_lblHead);   // Fill
    }

    private void BuildEmergencyTab()
    {
        // Row 1 — branch combo on the shared input column x=110 (width set by LayoutResponsive).
        var lblBranch = new Label
        {
            Text      = _t["branchLabel"],
            AutoSize  = false,
            Bounds    = new Rectangle(14, 26, 90, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboEmergencyBranch = new ComboBox
        {
            Name          = "cboEmergencyBranch",
            DropDownStyle = ComboBoxStyle.DropDownList,
            Sorted        = true,
            Bounds        = new Rectangle(110, 24, 590, 22)
        };

        // Row 2 — tag combo on its own line (width set by LayoutResponsive).
        var lblTag = new Label
        {
            Text      = _t["tagLabel"],
            AutoSize  = false,
            Bounds    = new Rectangle(14, 54, 90, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboEmergencyTag = new ComboBox
        {
            Name          = "cboEmergencyTag",
            DropDownStyle = ComboBoxStyle.DropDown,
            Bounds        = new Rectangle(110, 52, 590, 22),
            DropDownWidth = 560
        };

        // Row 3 — action buttons as a right-aligned pair (equal 150 px width; positioned by LayoutResponsive).
        _btnEmergencyRestore = new Button
        {
            Name   = "btnEmergencyRestore",
            Text   = _t["restoreToTag"],
            Bounds = new Rectangle(388, 84, 150, 26)
        };
        _btnEmergencyRestore.Click += BtnEmergencyRestore_Click;

        _btnEmergencyReset = new Button
        {
            Name      = "btnEmergencyReset",
            Text      = _t["resetToTag"],
            ForeColor = Color.DarkRed,
            Bounds    = new Rectangle(550, 84, 150, 26)
        };
        _btnEmergencyReset.Click += BtnEmergencyReset_Click;

        AddTab("emergencyGroup",
            lblBranch, _cboEmergencyBranch, lblTag, _cboEmergencyTag,
            _btnEmergencyRestore, _btnEmergencyReset);

        _localized.AddRange(new KeyValuePair<Control, string>[]
        {
            new(lblBranch, "branchLabel"), new(lblTag, "tagLabel"),
            new(_btnEmergencyRestore, "restoreToTag"), new(_btnEmergencyReset, "resetToTag"),
        });
    }

    private void BuildRestoreFileTab()
    {
        // Both rows share the input column x=174 (wide enough for the long "Arquivo (caminho
        // relativo):" label); their widths are set by LayoutResponsive.
        var lblHash = new Label
        {
            Text      = _t["commitHash"],
            AutoSize  = false,
            Bounds    = new Rectangle(14, 26, 156, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboRestoreHash = new ComboBox
        {
            Name          = "cboRestoreHash",
            DropDownStyle = ComboBoxStyle.DropDown,
            Bounds        = new Rectangle(174, 24, 526, 22),
            DropDownWidth = 526   // pinned to the field width by LayoutResponsive (stays within the right margin)
        };

        var lblFile = new Label
        {
            Text      = _t["fileRelative"],
            AutoSize  = false,
            Bounds    = new Rectangle(14, 54, 156, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _txtRestoreFile = new TextBox
        {
            Name            = "txtRestoreFile",
            PlaceholderText = _t["filePlaceholder"],
            Bounds          = new Rectangle(174, 52, 526, 22)
        };

        _btnRestoreFile = new Button
        {
            Name   = "btnRestoreFile",
            Text   = _t["restoreFileBtn"],
            Bounds = new Rectangle(550, 84, 150, 24)
        };
        _btnRestoreFile.Click += BtnRestoreFile_Click;

        AddTab("restoreFileGroup", lblHash, _cboRestoreHash, lblFile, _txtRestoreFile, _btnRestoreFile);

        _localized.AddRange(new KeyValuePair<Control, string>[]
        {
            new(lblHash, "commitHash"), new(lblFile, "fileRelative"), new(_btnRestoreFile, "restoreFileBtn"),
        });
    }

    private void BuildCherryPickTab()
    {
        // Label + combo on the first row, at the same height as cboEmergencyBranch (combo y=24, label y=26).
        var lblHash = new Label
        {
            Text      = _t["commitHash"],
            AutoSize  = false,
            Bounds    = new Rectangle(14, 26, 90, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboCherryHash = new ComboBox
        {
            Name          = "cboCherryHash",
            DropDownStyle = ComboBoxStyle.DropDown,
            Bounds        = new Rectangle(110, 24, 590, 22),   // width set by LayoutResponsive
            DropDownWidth = 590   // pinned to the field width by LayoutResponsive (stays within the right margin)
        };
        // Button on its own row below the combo, right-aligned by LayoutResponsive.
        _btnCherryPick = new Button
        {
            Name   = "btnCherryPick",
            Text   = _t["applyCherryPick"],
            Bounds = new Rectangle(550, 52, 150, 24)
        };
        _btnCherryPick.Click += BtnCherryPick_Click;

        AddTab("cherryPickGroup", lblHash, _cboCherryHash, _btnCherryPick);

        _localized.AddRange(new KeyValuePair<Control, string>[]
        {
            new(lblHash, "commitHash"), new(_btnCherryPick, "applyCherryPick"),
        });
    }

    private void BuildResetTab()
    {
        var lblBranch = new Label
        {
            Text      = _t["branchLabel"],
            AutoSize  = false,
            Bounds    = new Rectangle(14, 26, 90, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboBranch = new ComboBox
        {
            Name          = "cboBranch",
            DropDownStyle = ComboBoxStyle.DropDownList,
            Sorted        = true,
            Bounds        = new Rectangle(110, 24, 590, 22)
        };

        var lblHash = new Label
        {
            Text      = _t["commitHash"],
            AutoSize  = false,
            Bounds    = new Rectangle(14, 54, 90, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboResetHash = new ComboBox
        {
            Name          = "cboResetHash",
            DropDownStyle = ComboBoxStyle.DropDown,
            Bounds        = new Rectangle(110, 52, 590, 22),
            DropDownWidth = 590   // pinned to the field width by LayoutResponsive (stays within the right margin)
        };

        // The three reset-mode radios share the left edge (x=14); the Reset button sits to their
        // right, right-aligned by LayoutResponsive and vertically centered against the radio block.
        _rdMixed = new RadioButton
        {
            Text    = _t["resetMixed"],
            Bounds  = new Rectangle(14, 84, 520, 20),
            Checked = true
        };
        _rdSoft = new RadioButton
        {
            Text   = _t["resetSoft"],
            Bounds = new Rectangle(14, 106, 520, 20)
        };
        _rdHard = new RadioButton
        {
            Text      = _t["resetHard"],
            Bounds    = new Rectangle(14, 128, 520, 20),
            ForeColor = Color.DarkRed
        };

        _btnReset = new Button
        {
            Name   = "btnReset",
            Text   = _t["resetBtn"],
            Bounds = new Rectangle(550, 106, 150, 24)
        };
        _btnReset.Click += BtnReset_Click;

        AddTab("resetGroup", lblBranch, _cboBranch, lblHash, _cboResetHash, _rdMixed, _rdSoft, _rdHard, _btnReset);

        _localized.AddRange(new KeyValuePair<Control, string>[]
        {
            new(lblBranch, "branchLabel"), new(lblHash, "commitHash"),
            new(_rdMixed, "resetMixed"), new(_rdSoft, "resetSoft"), new(_rdHard, "resetHard"),
            new(_btnReset, "resetBtn"),
        });
    }

    private void BuildResultGroup()
    {
        _grpResult = new GroupBox
        {
            Name    = "grpResult",
            Text    = _t["resultGroup"],
            Dock    = DockStyle.Fill,
            Padding = new Padding(10, 4, 10, 10)
        };
        _txtResult = new TextBox
        {
            Name       = "txtResult",
            Multiline  = true,
            ReadOnly   = true,
            ScrollBars = ScrollBars.Both,
            WordWrap   = false,
            // Match the GitExtensions native console output background (the beige seen in the
            // "Push to origin" / fetch windows) instead of plain white.
            BackColor  = Color.FromArgb(0xEF, 0xEB, 0xD8),
            Font       = new Font("Consolas", 9f),
            Dock       = DockStyle.Fill
        };
        _grpResult.Controls.Add(_txtResult);
        _localized.Add(new(_grpResult, "resultGroup"));
    }

    private void BuildCloseButton()
    {
        _btnClose = new Button
        {
            Name         = "btnClose",
            Text         = _t["closeBtn"],
            Width        = 90,
            Height       = 28,
            DialogResult = DialogResult.Cancel
        };
        _btnClose.Click += (_, _) => Close();

        // Show Debug toggles the debug TYPE/ID tooltips live; defaults to the owner's Show-Debug state.
        _chkShowDebug = new CheckBox
        {
            Name     = "chkShowDebug",
            Text     = _t["showDebug"],
            AutoSize = true,
            Checked  = _showControlIds
        };
        _chkShowDebug.CheckedChanged += (_, _) => ApplyOrClearTooltips(_chkShowDebug.Checked);

        // Language selector (right-aligned). Items + selection populated by PopulateLanguageCombo so
        // they stay localized; index order matches AppLanguage (0=Automatic, 1=English, 2=Portuguese).
        _lblLanguage = new Label
        {
            Name      = "lblLanguage",
            Text      = _t["language"],
            AutoSize  = true,
            TextAlign = ContentAlignment.MiddleRight
        };
        _cboLanguage = new ComboBox
        {
            Name          = "cboLanguage",
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width         = 120
        };
        _cboLanguage.SelectedIndexChanged += OnLanguageChanged;
        PopulateLanguageCombo();

        // Docked bottom bar: close button centered, Show Debug pinned left, Language at the right edge
        // (mirrors ZimerfeldTree).
        _bottomPanel = new Panel { Name = "bottomPanel", Dock = DockStyle.Bottom, Height = 40 };
        _bottomPanel.Controls.Add(_btnClose);
        _bottomPanel.Controls.Add(_chkShowDebug);
        _bottomPanel.Controls.Add(_lblLanguage);
        _bottomPanel.Controls.Add(_cboLanguage);
        _bottomPanel.Layout += (_, _) =>
        {
            _btnClose.Location = new Point(
                (_bottomPanel.Width  - _btnClose.Width)  / 2,
                (_bottomPanel.Height - _btnClose.Height) / 2);
            _chkShowDebug.Location = new Point(
                8, (_bottomPanel.Height - _chkShowDebug.Height) / 2);
            _cboLanguage.Location = new Point(
                _bottomPanel.Width - _cboLanguage.Width - 8,
                (_bottomPanel.Height - _cboLanguage.Height) / 2);
            _lblLanguage.Location = new Point(
                _cboLanguage.Left - _lblLanguage.Width - 6,
                (_bottomPanel.Height - _lblLanguage.Height) / 2);
        };

        _localized.AddRange(new KeyValuePair<Control, string>[]
        {
            new(_btnClose, "closeBtn"), new(_chkShowDebug, "showDebug"), new(_lblLanguage, "language"),
        });
    }

    // ── Language selection ────────────────────────────────────────────────────

    /// <summary>Persists the dropdown choice and re-localizes this window in place.</summary>
    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        if (_suppressLangEvent) return;
        var lang = _cboLanguage.SelectedIndex switch
        {
            1 => AppLanguage.English,
            2 => AppLanguage.Portuguese,
            _ => AppLanguage.Automatic,
        };
        I18n.SetLanguage(lang);
        ApplyLanguage();
    }

    /// <summary>Reloads the active-language dictionary and reapplies every registered text in place.</summary>
    private void ApplyLanguage()
    {
        _t = I18n.Load("ZimerfeldRestore");
        Text = _t["title"];
        foreach (var (c, key) in _localized) c.Text = _t[key];
        _lblHead.Text = _t.F("headLabel", _svc.GetHeadRef());
        _txtRestoreFile.PlaceholderText = _t["filePlaceholder"];

        // Re-localize the "Select..." prompt sitting at index 0 of each hash combo.
        foreach (var cbo in new[] { _cboRestoreHash, _cboCherryHash, _cboResetHash })
        {
            if (cbo.Items.Count == 0 || cbo.Items[0] is not string) continue;
            bool wasPrompt = cbo.SelectedIndex == 0;
            cbo.Items[0] = _t["selectPrompt"];
            if (wasPrompt) cbo.SelectedIndex = 0;
        }

        PopulateLanguageCombo();
    }

    /// <summary>Repopulates the language dropdown with localized item labels, preserving selection.</summary>
    private void PopulateLanguageCombo()
    {
        _suppressLangEvent = true;
        int sel = _cboLanguage.SelectedIndex >= 0 ? _cboLanguage.SelectedIndex : (int)I18n.Current;
        _cboLanguage.Items.Clear();
        _cboLanguage.Items.AddRange([_t["langAutomatic"], _t["langEnglish"], _t["langPortuguese"]]);
        _cboLanguage.SelectedIndex = sel;
        _suppressLangEvent = false;
    }

    /// <summary>Shows the debug TYPE/ID tooltips when enabled, or clears them when disabled.</summary>
    private void ApplyOrClearTooltips(bool show)
    {
        if (show) ApplyControlTooltips();
        else      _mainTooltip.RemoveAll();
    }

    // ── Initialization ───────────────────────────────────────────────────────

    private void InitData()
    {
        _lblHead.Text = _t.F("headLabel", _svc.GetHeadRef());

        var (branchOutput, _) = _svc.RunGitFlow("branch");
        var branches = branchOutput
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(b => b.TrimStart('*', ' ').Trim())
            .Where(b => b.Length > 0)
            .ToList();

        _cboBranch.Items.AddRange(branches.Cast<object>().ToArray());
        _cboEmergencyBranch.Items.AddRange(branches.Cast<object>().ToArray());

        // Plano de Emergência: the tag combo lists every tag (newest first).
        var tags = _svc.GetTags().Select(t => t.FullName).ToList();
        _cboEmergencyTag.Items.AddRange(tags.Cast<object>().ToArray());
        if (_cboEmergencyTag.Items.Count > 0) _cboEmergencyTag.SelectedIndex = 0;

        var refs = LoadCommitRefs();
        foreach (var cbo in new[] { _cboRestoreHash, _cboCherryHash, _cboResetHash })
        {
            // index 0 is the "Select..." prompt; the commit refs follow (already newest-first).
            cbo.Items.Add(_t["selectPrompt"]);
            cbo.Items.AddRange(refs.Cast<object>().ToArray());
            cbo.SelectedIndex = 0;
        }

        var saved = LoadSettings();
        RestoreSettings(saved);

        // Both branch combos default to the currently checked-out branch (the one most likely to be
        // restored/reset). If it isn't in the list, fall back to develop/main/master, then index 0.
        string current = _svc.GetCurrentBranch();
        SelectBranchDefault(_cboBranch, current);
        SelectBranchDefault(_cboEmergencyBranch, current);
    }

    /// <summary>
    /// Preselects <paramref name="current"/> in a branch combo, falling back to develop → main →
    /// master → first item when the checked-out branch isn't present.
    /// </summary>
    private static void SelectBranchDefault(ComboBox cbo, string current)
    {
        if (cbo.Items.Count == 0) return;
        int idx = cbo.Items.IndexOf(current);
        if (idx < 0) idx = cbo.Items.IndexOf("develop");
        if (idx < 0) idx = cbo.Items.IndexOf("main");
        if (idx < 0) idx = cbo.Items.IndexOf("master");
        cbo.SelectedIndex = idx >= 0 ? idx : 0;
    }

    private List<CommitRef> LoadCommitRefs()
    {
        // --source tags each commit with the ref by which the walk first reached it
        // (e.g. "refs/heads/develop"), exposed via %S and shown as the owning branch.
        // Fields are separated by the unit-separator byte (0x1F, %x1f) so subjects
        // containing spaces/tabs don't break parsing: "<hash>␟<source>␟<date>␟<subject>".
        var (output, _) = _svc.RunGitFlow(
            "log --all --source -200 --date=format:\"%Y-%m-%d %H:%M:%S\" " +
            "--pretty=format:\"%h%x1f%S%x1f%cd%x1f%s\"");
        var refs = new List<CommitRef>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('\x1f');
            if (parts.Length == 4 && parts[0].Length >= 7)
                refs.Add(new CommitRef(parts[3], parts[0], ShortRef(parts[1]), parts[2]));
        }
        // Newest first. The date string is fixed-width "YYYY-MM-dd HH:mm:ss", so an ordinal
        // descending sort orders by actual chronology without parsing to DateTime.
        refs.Sort((a, b) => string.CompareOrdinal(b.Date, a.Date));
        return refs;
    }

    /// <summary>Strips the <c>refs/heads/</c>, <c>refs/remotes/</c> and <c>refs/tags/</c>
    /// prefixes so the dropdown shows a compact branch/tag name.</summary>
    private static string ShortRef(string fullRef)
    {
        foreach (var prefix in new[] { "refs/heads/", "refs/remotes/", "refs/tags/" })
            if (fullRef.StartsWith(prefix))
                return fullRef[prefix.Length..];
        return fullRef;
    }

    // ── Persistence ──────────────────────────────────────────────────────────

    private static Dictionary<string, string> LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsFilePath)) return [];
            using var doc = JsonDocument.Parse(File.ReadAllText(SettingsFilePath));
            var dict = new Dictionary<string, string>();
            foreach (var prop in doc.RootElement.EnumerateObject())
                dict[prop.Name] = prop.Value.GetString() ?? string.Empty;
            return dict;
        }
        catch { return []; }
    }

    private void SaveSettings()
    {
        try
        {
            string dir = Path.GetDirectoryName(SettingsFilePath)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            // No ComboBox in this window is persisted — every combo (emergency branch/tag, hash
            // dropdowns and reset branch) reopens at its default each time, so a stale selection is
            // never silently reused. Only the non-combo fields below are remembered.
            var settings = new Dictionary<string, string>
            {
                ["restoreFile"] = _txtRestoreFile.Text.Trim(),
                ["resetMode"]   = _rdHard.Checked ? "hard" : _rdSoft.Checked ? "soft" : "mixed"
            };
            File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(settings));
        }
        catch { }
    }

    private void RestoreSettings(Dictionary<string, string> saved)
    {
        // No combo is restored — emergency branch/tag, the hash dropdowns and the reset branch all
        // open at their defaults. Only the non-combo fields (restore file, reset mode) are remembered.
        if (saved.TryGetValue("restoreFile", out var rf) && rf.Length > 0)
            _txtRestoreFile.Text = rf;

        if (saved.TryGetValue("resetMode", out var mode))
        {
            _rdHard.Checked  = mode == "hard";
            _rdSoft.Checked  = mode == "soft";
            _rdMixed.Checked = mode != "hard" && mode != "soft";
        }
    }

    // ── Git execution ────────────────────────────────────────────────────────

    private bool RunGit(string args, bool append = false)
    {
        int code;
        Cursor = Cursors.WaitCursor;
        try
        {
            var (output, exitCode) = _svc.RunGitFlow(args);
            code = exitCode;
            string body = output.Length == 0
                ? (code == 0 ? _t["cmdDone"] : _t["noOutput"])
                : output.Replace("\n", "\r\n");
            string block = $"command - git {args}\r\n\r\n{body}";
            _txtResult.Text = append && _txtResult.Text.Length > 0
                ? _txtResult.Text + "\r\n\r\n" + block
                : block;
        }
        finally
        {
            Cursor = Cursors.Default;
            if (!IsDisposed) Activate();
        }
        _lblHead.Text = _t.F("headLabel", _svc.GetHeadRef());
        return code == 0;
    }

    // ── Button handlers ──────────────────────────────────────────────────────

    private void BtnRestoreFile_Click(object? sender, EventArgs e)
    {
        string hash = HashOf(_cboRestoreHash);
        string file = _txtRestoreFile.Text.Trim();
        if (hash.Length == 0 || file.Length == 0)
        {
            MessageBox.Show(_t["informHashAndFile"],
                _t["restoreFileTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        bool ok = RunGit($"checkout {Clean(hash)} -- \"{Clean(file)}\"");
        if (ok) RevealInTree(null);
    }

    private void BtnCherryPick_Click(object? sender, EventArgs e)
    {
        string hash = HashOf(_cboCherryHash);
        if (hash.Length == 0)
        {
            MessageBox.Show(_t["informHashOrRange"],
                _t["cherryPickTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        bool ok = RunGit($"cherry-pick {hash.Replace("\"", "")}");
        if (ok) RevealInTree(null);
    }

    /// <summary>
    /// Asks the owner ZimerfeldTree window to refresh its tree and reveal/select the affected
    /// branch, then keeps focus on this (modal) window. Pass a null/empty branch to only refresh.
    /// Mirrors <see cref="GitFlowForm"/>'s helper so both windows route refreshes the same way.
    /// </summary>
    private void RevealInTree(string? fullBranch)
    {
        fullBranch = (fullBranch ?? string.Empty).Trim();
        RepoMutated?.Invoke(fullBranch.Length > 0 ? fullBranch : null);
        if (!IsDisposed) Activate();
    }

    private void BtnReset_Click(object? sender, EventArgs e)
    {
        if (_cboBranch.SelectedItem is not string branch || branch.Length == 0)
        {
            MessageBox.Show(_t["selectBranchReset"],
                _t["resetTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        string hash = HashOf(_cboResetHash);
        if (hash.Length == 0)
        {
            MessageBox.Show(_t["informHashTarget"],
                _t["resetTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string mode = _rdHard.Checked ? "--hard" : _rdSoft.Checked ? "--soft" : "--mixed";

        if (_rdHard.Checked)
        {
            var dr = MessageBox.Show(
                _t.F("confirmHardWarn", branch),
                _t["confirmHardTitle"], MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (dr != DialogResult.Yes) return;
        }

        string safeHash   = Clean(hash);
        string current    = _svc.GetCurrentBranch();
        bool   needSwitch = !string.Equals(current, branch, StringComparison.OrdinalIgnoreCase);

        if (needSwitch)
        {
            if (!RunGit($"checkout {branch}")) return;
        }

        bool ok = RunGit($"reset {mode} {safeHash}", append: needSwitch);
        if (ok) RevealInTree(branch);

        if (needSwitch)
            RunGit($"checkout {current}", append: true);
    }

    // ── Plano de Emergência ───────────────────────────────────────────────────

    /// <summary>
    /// Restores the selected branch's working tree to the state of the chosen tag without
    /// rewriting history: switches to the branch and runs <c>git checkout &lt;tag&gt; -- .</c>,
    /// leaving the differences staged so the user can review and commit them.
    /// </summary>
    private void BtnEmergencyRestore_Click(object? sender, EventArgs e)
    {
        if (_cboEmergencyBranch.SelectedItem is not string branch || branch.Length == 0)
        {
            MessageBox.Show(_t["selectBranchRestore"],
                _t["emergencyTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        string tag = _cboEmergencyTag.Text.Trim();
        if (tag.Length == 0)
        {
            MessageBox.Show(_t["selectTagRef"],
                _t["emergencyTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string safeTag    = Clean(tag);
        string current    = _svc.GetCurrentBranch();
        bool   needSwitch = !string.Equals(current, branch, StringComparison.OrdinalIgnoreCase);

        if (needSwitch && !RunGit($"checkout {branch}")) return;

        // git checkout <tag> -- .  → stages the tag's content over the branch; history untouched.
        bool ok = RunGit($"checkout {safeTag} -- .", append: needSwitch);
        if (ok) RevealInTree(branch);
    }

    /// <summary>
    /// Hard-resets the selected branch to the chosen tag (moves the branch pointer and discards
    /// local changes). Irreversible — guarded by a confirmation prompt.
    /// </summary>
    private void BtnEmergencyReset_Click(object? sender, EventArgs e)
    {
        if (_cboEmergencyBranch.SelectedItem is not string branch || branch.Length == 0)
        {
            MessageBox.Show(_t["selectBranchResetEmerg"],
                _t["emergencyTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        string tag = _cboEmergencyTag.Text.Trim();
        if (tag.Length == 0)
        {
            MessageBox.Show(_t["selectTagRef"],
                _t["emergencyTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var dr = MessageBox.Show(
            _t.F("confirmResetTagWarn", branch, tag),
            _t["confirmResetTagTitle"], MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (dr != DialogResult.Yes) return;

        string safeTag    = Clean(tag);
        string current    = _svc.GetCurrentBranch();
        bool   needSwitch = !string.Equals(current, branch, StringComparison.OrdinalIgnoreCase);

        if (needSwitch && !RunGit($"checkout {branch}")) return;

        bool ok = RunGit($"reset --hard {safeTag}", append: needSwitch);
        if (ok) RevealInTree(branch);

        if (needSwitch)
            RunGit($"checkout {current}", append: true);
    }

    // ── About ────────────────────────────────────────────────────────────────

    private void ShowAbout()
    {
        MessageBox.Show(_t["aboutBody"], _t["aboutTitle"],
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private string HashOf(ComboBox cbo)
    {
        if (cbo.SelectedItem is CommitRef r) return r.Hash;
        string text = cbo.Text.Trim();
        // The "Select..." prompt is not a hash — treat it as no selection.
        return text == _t["selectPrompt"] ? string.Empty : text;
    }

    private sealed class CommitRef
    {
        public string Hash { get; }
        /// <summary>Commit date as "YYYY-MM-dd HH:mm:ss" — lexically sortable, newest first.</summary>
        public string Date { get; }
        private readonly string _display;
        public CommitRef(string name, string hash, string source, string date)
        {
            Hash = hash;
            Date = date;
            // "(YYYY-MM-dd HH:mm:ss) [branch] hash  →  subject" — date in parentheses, then branch,
            // then the hash, then the commit message.
            _display = $"({date}) [{source}] {hash}  →  {name}";
        }
        public override string ToString() => _display;
    }

    // ── Tooltip debug ────────────────────────────────────────────────────────

    private void ApplyControlTooltips()
    {
        _mainTooltip.RemoveAll();
        SetTooltipsRecursive(this, _mainTooltip);
        _mainTooltip.SetToolTip(this, $"TYPE: {GetType().Name}\nHandle: 0x{Handle.ToInt64():X}");
    }

    private static void SetTooltipsRecursive(Control parent, ToolTip tip)
    {
        foreach (Control c in parent.Controls)
        {
            if (c.Name.Length > 0)
                tip.SetToolTip(c, $"TYPE: {c.GetType().Name}\nID: {c.Name}");
            SetTooltipsRecursive(c, tip);
        }
    }

    private static string Clean(string s) => s.Trim().Replace("\"", "");
}
