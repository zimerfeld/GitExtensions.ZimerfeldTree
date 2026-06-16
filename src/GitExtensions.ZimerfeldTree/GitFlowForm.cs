// GitFlowForm.cs — Git Flow operations window for ZimerfeldTree plugin
// MIT License — Copyright (c) 2026 Zimerfeld

namespace GitExtensions.ZimerfeldTree;

/// <summary>
/// Modal window that drives <c>git flow</c> commands: starting feature/release/hotfix
/// branches and publishing, pulling and finishing existing ones.  The raw command output
/// is shown so the user can see exactly what git flow did.
/// </summary>
public sealed class GitFlowForm : Form
{
    private readonly BranchHierarchyService _svc;
    private readonly bool _showControlIds;
    private readonly ToolTip _mainTooltip = new ToolTip();

    // Loaded for the active language; reassigned by ApplyLanguage when the user switches languages
    // via the bottom-panel dropdown (so the open window re-localizes live, like ZimerfeldTree).
    private Translator _t = I18n.Load("ZimerfeldGitFlow");

    // (control, dictionary-key) pairs reapplied by ApplyLanguage so every label/button re-localizes
    // in place when the Language dropdown changes.
    private readonly List<KeyValuePair<Control, string>> _localized = new();

    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GitExtensions", "ZimerfeldTree.gitflowsettings.json");

    // ── Layout shell (docked) ──
    private Panel _headerPanel = null!;
    private Panel _content     = null!;
    private Panel _bottomPanel = null!;

    // ── Header ──
    private Label     _lblHead   = null!;
    private LinkLabel _lnkAbout  = null!;

    // ── Start branch ──
    private GroupBox _grpStart      = null!;
    private ComboBox _cboStartType  = null!;
    private Label    _lblStartPrefix = null!;
    private TextBox  _txtStartName  = null!;
    private Button   _btnStart      = null!;
    private CheckBox _chkBasedOn    = null!;
    private ComboBox _cboBasedOn    = null!;

    // ── Manage existing branches ──
    private GroupBox _grpManage      = null!;
    private ComboBox _cboManageType  = null!;
    private Label    _lblManagePrefix = null!;
    private ComboBox _cboManageBranch = null!;
    private Button   _btnPublish     = null!;
    private Button   _btnTrack       = null!;
    private Button   _btnUpdate      = null!;
    private Button   _btnFinish      = null!;
    private CheckBox _chkKeep        = null!;
    private CheckBox _chkNoFetch     = null!;

    // ── Result ──
    private GroupBox _grpResult = null!;
    private TextBox  _txtResult = null!;

    // ── Bottom bar ──
    private Button   _btnClose     = null!;
    private CheckBox _chkShowDebug = null!;
    private Label    _lblLanguage  = null!;
    private ComboBox _cboLanguage  = null!;
    private bool     _suppressLangEvent;

    /// <summary>Set after a successful "release" finish; BranchHierarchyForm reads this to focus the new tag.</summary>
    public string? LastFinishedReleaseTag { get; private set; }

    /// <summary>
    /// Raised after a git-flow operation mutates the repository while this (modal) window is open,
    /// so the owning ZimerfeldTree window can refresh its tree without waiting for the dialog to close.
    /// The argument is the full name of the affected branch to reveal/select in the tree
    /// (e.g. "feature/x", "develop"), or <c>null</c> to only refresh.
    /// </summary>
    public event Action<string?>? RepoMutated;

    public GitFlowForm(BranchHierarchyService svc, bool showControlIds = false)
    {
        _svc            = svc;
        _showControlIds = showControlIds;

        Text            = _t["title"];
        // Width 720 (client 704 → content 688) gives the Finish-aligned Keep/No-fetch checkboxes room
        // for the longer Portuguese labels without clipping at the group's right border.
        Size            = new Size(720, 824 + SponsorBanner.PanelHeight);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        StartPosition   = FormStartPosition.Manual;   // caller controls position (side-by-side)
        Font            = new Font("Segoe UI", 9f);
        Icon            = PluginIcon.ForForm();

        // Docked shell: a Fill content area (8 px padding → symmetric side borders) holding the
        // header + groups, with the sponsor banner on top and a centered close-button bar below.
        _content = new Panel { Name = "contentPanel", Dock = DockStyle.Fill, Padding = new Padding(8, 4, 8, 4) };

        BuildHeader();
        BuildStartGroup();
        BuildManageGroup();
        BuildResultGroup();
        BuildCloseButton();

        // Add to the content panel back-to-front so Dock resolves correctly: Fill first (backmost),
        // then the Top-docked groups bottom-up, header last (topmost).
        _content.Controls.Add(_grpResult);   // Fill
        _content.Controls.Add(_grpManage);   // Top
        _content.Controls.Add(_grpStart);    // Top
        _content.Controls.Add(_headerPanel); // Top (just under the banner)

        Controls.Add(_content);              // Fill — between banner and bottom bar
        Controls.Add(_bottomPanel);          // Bottom
        Controls.Add(SponsorBanner.Create(_lnkAbout)); // Top — Sponsors banner hosts lnkAbout

        CancelButton = _btnClose;

        SetTabOrder();
        Load += (_, _) =>
        {
            InitData();
            ApplySettings();
            ApplyOrClearTooltips(_chkShowDebug.Checked);
        };
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
            TextAlign = ContentAlignment.MiddleLeft,   // HEAD aligned to the left edge of the window
            Dock      = DockStyle.Fill
        };

        _headerPanel.Controls.Add(_lblHead);   // Fill
    }

    private void BuildStartGroup()
    {
        // "Start branch" is now the GroupBox title — no separate lblType inside.
        _grpStart = new GroupBox
        {
            Name   = "grpStart",
            Text   = _t["startGroup"],
            Dock   = DockStyle.Top,
            Height = 120,
            // Final docked width (content 704 client − 16 padding). Set BEFORE its right-anchored
            // children are added so their anchor margins are computed against the real width.
            Width  = 688
        };

        // Row 1 — type selector (col label x=12, col input x=108)
        var lblType = new Label
        {
            Name      = "lblStartType",
            Text      = _t["typeLabel"],
            TextAlign = ContentAlignment.MiddleLeft,
            Bounds    = new Rectangle(12, 24, 90, 22)
        };
        _cboStartType = new ComboBox
        {
            Name          = "cboStartType",
            Bounds        = new Rectangle(108, 22, 180, 24),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cboStartType.Items.AddRange([.. BranchHierarchyService.GitFlowTypes]);
        _cboStartType.SelectedIndexChanged += (_, _) =>
        {
            _lblStartPrefix.Text = _svc.GetGitFlowPrefix(_cboStartType.Text);

            // Default release name follows the convention yyyyMMddHHmm (e.g. 202605311230).
            // Only auto-fill when the field is empty so manual input is never overwritten.
            if (string.Equals(_cboStartType.Text, "release", StringComparison.OrdinalIgnoreCase)
                && _txtStartName.Text.Trim().Length == 0)
                _txtStartName.Text = DateTime.Now.ToString("yyyyMMddHHmm");

            // Drive the "based on" combo + checkbox from the selected type (see ApplyStartTypeRule).
            ApplyStartTypeRule();
        };

        // Row 2 — expected name (prefix label + text input + Start button)
        var lblName = new Label
        {
            Name      = "lblStartName",
            Text      = _t["expectedName"],
            TextAlign = ContentAlignment.MiddleLeft,
            Bounds    = new Rectangle(12, 54, 90, 22)
        };
        _lblStartPrefix = new Label
        {
            Name      = "lblStartPrefix",
            TextAlign = ContentAlignment.MiddleRight,
            Bounds    = new Rectangle(108, 54, 60, 22)
        };
        _txtStartName = new TextBox
        {
            Name   = "txtStartName",
            Bounds = new Rectangle(172, 54, 382, 22),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        _btnStart = new Button
        {
            Name   = "btnStart",
            Text   = _t["startBtn"],
            Bounds = new Rectangle(560, 52, 90, 26),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _btnStart.Click += (_, _) => DoStart();

        // Row 3 — optional base branch
        _chkBasedOn = new CheckBox
        {
            Name   = "chkBasedOn",
            Text   = _t["basedOn"],
            Bounds = new Rectangle(108, 84, 90, 22)
        };
        _chkBasedOn.CheckedChanged += (_, _) => _cboBasedOn.Enabled = _chkBasedOn.Checked;

        _cboBasedOn = new ComboBox
        {
            Name          = "cboBasedOn",
            Bounds        = new Rectangle(202, 82, 348, 24),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Sorted        = true,
            Enabled       = false,
            Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        _grpStart.Controls.AddRange(
            [lblType, _cboStartType, lblName, _lblStartPrefix, _txtStartName, _btnStart,
             _chkBasedOn, _cboBasedOn]);

        _localized.AddRange(new KeyValuePair<Control, string>[]
        {
            new(_grpStart, "startGroup"), new(lblType, "typeLabel"), new(lblName, "expectedName"),
            new(_btnStart, "startBtn"),   new(_chkBasedOn, "basedOn"),
        });
    }

    private void BuildManageGroup()
    {
        _grpManage = new GroupBox
        {
            Name   = "grpManage",
            Text   = _t["manageGroup"],
            Dock   = DockStyle.Top,
            Height = 192,
            Width  = 688   // see grpStart: real width before right-anchored children are added
        };

        // Row 1 — type selector (aligned with grpStart: label x=12, input x=108)
        var lblType = new Label
        {
            Name      = "lblManageType",
            Text      = _t["typeLabel"],
            TextAlign = ContentAlignment.MiddleLeft,
            Bounds    = new Rectangle(12, 24, 90, 22)
        };
        _cboManageType = new ComboBox
        {
            Name          = "cboManageType",
            Bounds        = new Rectangle(108, 22, 180, 24),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cboManageType.Items.AddRange([.. BranchHierarchyService.GitFlowTypes]);
        _cboManageType.SelectedIndexChanged += (_, _) => ReloadManageBranches();

        // Row 2 — branch selector (same column positions as grpStart name row)
        var lblBranch = new Label
        {
            Name      = "lblManageBranch",
            Text      = _t["branchLabel"],
            TextAlign = ContentAlignment.MiddleLeft,
            Bounds    = new Rectangle(12, 54, 90, 22)
        };
        _lblManagePrefix = new Label
        {
            Name      = "lblManagePrefix",
            Text      = "/",
            TextAlign = ContentAlignment.MiddleRight,
            Bounds    = new Rectangle(108, 54, 60, 22)
        };
        _cboManageBranch = new ComboBox
        {
            Name          = "cboManageBranch",
            Bounds        = new Rectangle(172, 52, 478, 24),
            DropDownStyle = ComboBoxStyle.DropDown,
            Sorted        = true,
            Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        // ── Buttons row: 4 × 140 px, gap 18 px, left margin 12 px ──────────────
        // (12)[Publish 140](18)[Track 140](18)[Update 140](18)[Finish 140](12) = 638 ✓
        _btnPublish = new Button { Name = "btnPublish", Text = _t["publishBtn"], Bounds = new Rectangle( 12, 84, 140, 26) };
        _btnPublish.Click += (_, _) => DoPublish();

        _btnTrack = new Button { Name = "btnTrack", Text = _t["trackBtn"],   Bounds = new Rectangle(170, 84, 140, 26) };
        _btnTrack.Click += (_, _) => DoTrack();

        _btnUpdate = new Button { Name = "btnUpdate", Text = _t["updateBtn"],  Bounds = new Rectangle(328, 84, 140, 26) };
        _btnUpdate.Click += (_, _) => DoUpdate();

        _btnFinish = new Button { Name = "btnFinish", Text = _t["finishBtn"],  Bounds = new Rectangle(486, 84, 140, 26) };
        _btnFinish.Click += (_, _) => DoFinish();

        // ── Checkboxes stacked below the Finish button, left edge aligned with btnFinish (x=486) ──
        _chkKeep = new CheckBox
        {
            Name     = "chkKeep",
            Text     = _t["keepBranch"],
            AutoSize = true,
            Location = new Point(486, 114),
            Checked  = true  // default: keep branch; overridden by saved settings on Load
        };
        _chkKeep.CheckedChanged += (_, _) => SaveSettings();

        _chkNoFetch = new CheckBox
        {
            Name     = "chkNoFetch",
            Text     = _t["noFetch"],
            AutoSize = true,
            Location = new Point(486, 136)
        };
        _chkNoFetch.CheckedChanged += (_, _) => SaveSettings();

        // Descriptions for Track/Update live only in the "Sobre" (About) link text.

        _grpManage.Controls.AddRange(
        [
            lblType, _cboManageType, lblBranch, _lblManagePrefix, _cboManageBranch,
            _btnPublish, _btnTrack, _btnUpdate, _btnFinish, _chkKeep, _chkNoFetch
        ]);

        _localized.AddRange(new KeyValuePair<Control, string>[]
        {
            new(_grpManage, "manageGroup"), new(lblType, "typeLabel"),   new(lblBranch, "branchLabel"),
            new(_btnPublish, "publishBtn"), new(_btnTrack, "trackBtn"),  new(_btnUpdate, "updateBtn"),
            new(_btnFinish, "finishBtn"),   new(_chkKeep, "keepBranch"), new(_chkNoFetch, "noFetch"),
        });
    }

    private void BuildResultGroup()
    {
        // Fills the remaining content area between grpManage and the close-button bar.
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
            Dock       = DockStyle.Fill,
            Font       = new Font("Consolas", 9f)
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

        // Show Debug toggles the TYPE/ID tooltips live; defaults to the owner's Show-Debug state.
        _chkShowDebug = new CheckBox
        {
            Name     = "chkShowDebug",
            Text     = _t["showDebug"],
            AutoSize = true,
            Checked  = _showControlIds
        };
        _chkShowDebug.CheckedChanged += (_, _) =>
        {
            ApplyOrClearTooltips(_chkShowDebug.Checked);
            SaveSettings();   // remember this window's Show-Debug state individually
        };

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
        _t = I18n.Load("ZimerfeldGitFlow");
        Text = _t["title"];
        foreach (var (c, key) in _localized) c.Text = _t[key];
        _lblHead.Text = _t.F("headLabel", _svc.GetHeadRef());
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

    // ── Tab order ───────────────────────────────────────────────────────────

    private void SetTabOrder()
    {
        // Form-level: top→bottom visually, right→left within rows
        _lnkAbout  .TabIndex = 0;
        _grpStart  .TabIndex = 1;
        _grpManage .TabIndex = 2;
        _grpResult .TabIndex = 3;
        _btnClose  .TabIndex = 4;

        // grpStart — top→bottom, left→right
        _cboStartType.TabIndex = 0;
        _txtStartName.TabIndex = 1;
        _btnStart    .TabIndex = 2;   // row 2, rightmost
        _chkBasedOn  .TabIndex = 3;
        _cboBasedOn  .TabIndex = 4;   // row 3, right

        // grpManage — top→bottom, left→right
        _cboManageType  .TabIndex = 0;
        _cboManageBranch.TabIndex = 1;
        _btnPublish     .TabIndex = 2;  // row 3, leftmost
        _btnTrack       .TabIndex = 3;
        _btnUpdate      .TabIndex = 4;
        _btnFinish      .TabIndex = 5;  // row 3, rightmost
        _chkKeep        .TabIndex = 6;
        _chkNoFetch     .TabIndex = 7;

        // grpResult
        _txtResult.TabIndex = 0;
    }

    // ── Tooltip debug ────────────────────────────────────────────────────────

    private void ApplyControlTooltips()
    {
        _mainTooltip.RemoveAll();
        SetTooltipsRecursive(this, _mainTooltip);
        // Also show the window's own TYPE and Handle (HWND) on hover over any uncovered area.
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

    // ── Settings persistence (checkboxes) ───────────────────────────────────

    private void ApplySettings()
    {
        var (keepBranch, noFetch, showDebug) = LoadSettings();
        _chkKeep     .Checked = keepBranch;
        _chkNoFetch  .Checked = noFetch;
        // Show Debug persists per-window; fall back to the owner's state on first open (no saved value).
        _chkShowDebug.Checked = showDebug ?? _showControlIds;
    }

    private static (bool keepBranch, bool noFetch, bool? showDebug) LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsFilePath)) return (true, false, null);
            string json = File.ReadAllText(SettingsFilePath);
            bool keep    = json.Contains("\"keepBranchAfterFinish\":true");
            bool noFetch = json.Contains("\"noFetch\":true");
            // Distinguish "absent" (→ null, use owner default) from an explicit saved false.
            bool? showDebug = json.Contains("\"showDebug\":")
                ? json.Contains("\"showDebug\":true")
                : null;
            return (keep, noFetch, showDebug);
        }
        catch { return (true, false, null); }
    }

    /// <summary>Persists all three window checkboxes (Keep branch, No fetch, Show Debug).</summary>
    private void SaveSettings()
    {
        try
        {
            string dir = Path.GetDirectoryName(SettingsFilePath)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(SettingsFilePath,
                $"{{\"keepBranchAfterFinish\":{(_chkKeep.Checked ? "true" : "false")}," +
                $"\"noFetch\":{(_chkNoFetch.Checked ? "true" : "false")}," +
                $"\"showDebug\":{(_chkShowDebug.Checked ? "true" : "false")}}}");
        }
        catch { }
    }

    // ── Data ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Populates the "based on" combo and sets the "based on" checkbox enabled/checked state
    /// according to the selected Start type, per the ZimerfeldGitFlow rule:
    ///   • hotfix  → base = main,            checkbox disabled (base is fixed)
    ///   • release → base = develop,         checkbox disabled (base is fixed)
    ///   • feature → "develop" + feature/*,  checkbox enabled (user may rebase)
    ///   • bugfix  → release/* only,         checkbox enabled (user picks a release)
    ///   • other   → develop + all locals,   checkbox enabled (generic fallback)
    /// The combo is usable only when the checkbox is both enabled and checked.
    /// </summary>
    private void ApplyStartTypeRule()
    {
        string develop = _svc.GetGitFlowBranchName("develop");
        string main    = _svc.GetGitFlowBranchName("main");

        _cboBasedOn.Items.Clear();

        switch (_cboStartType.Text.ToLowerInvariant())
        {
            case "hotfix":
                _cboBasedOn.Items.Add(main);
                _cboBasedOn.SelectedIndex = 0;
                _chkBasedOn.Checked = false;
                _chkBasedOn.Enabled = false;
                break;

            case "release":
                _cboBasedOn.Items.Add(develop);
                _cboBasedOn.SelectedIndex = 0;
                _chkBasedOn.Checked = false;
                _chkBasedOn.Enabled = false;
                break;

            case "feature":
                _cboBasedOn.Items.Add(develop);
                foreach (var f in FullNamesWithPrefix(_svc.GetGitFlowPrefix("feature")))
                    _cboBasedOn.Items.Add(f);
                _cboBasedOn.SelectedItem = develop;
                _chkBasedOn.Enabled = true;
                break;

            case "bugfix":
                foreach (var r in FullNamesWithPrefix(_svc.GetGitFlowPrefix("release")))
                    _cboBasedOn.Items.Add(r);
                if (_cboBasedOn.Items.Count > 0) _cboBasedOn.SelectedIndex = 0;
                _chkBasedOn.Enabled = true;
                break;

            case "support":
                // Support branches are anchored to a production release, so the base is one of
                // the existing tags. Required base → checkbox checked and enabled.
                foreach (var t in _svc.GetTags())
                    _cboBasedOn.Items.Add(t.FullName);
                if (_cboBasedOn.Items.Count > 0) _cboBasedOn.SelectedIndex = 0;
                _chkBasedOn.Checked = true;
                _chkBasedOn.Enabled = true;

                // Cascade to the Manage panel: auto-select "support" there too, which makes
                // ReloadManageBranches fill cboManageBranch with the existing tags.
                int supportMng = _cboManageType.Items.IndexOf("support");
                if (supportMng >= 0 && _cboManageType.SelectedIndex != supportMng)
                    _cboManageType.SelectedIndex = supportMng;
                break;

            default:
                _cboBasedOn.Items.Add(develop);
                foreach (var b in _svc.GetLocalBranches())
                    if (!_cboBasedOn.Items.Contains(b.FullName))
                        _cboBasedOn.Items.Add(b.FullName);
                _cboBasedOn.SelectedItem = develop;
                _chkBasedOn.Enabled = true;
                break;
        }

        _cboBasedOn.Enabled = _chkBasedOn.Enabled && _chkBasedOn.Checked;
    }

    /// <summary>Full names of local branches that begin with <paramref name="prefix"/> (e.g. "feature/").</summary>
    private IEnumerable<string> FullNamesWithPrefix(string prefix) =>
        _svc.GetLocalBranches()
            .Select(b => b.FullName)
            .Where(n => n.StartsWith(prefix, StringComparison.Ordinal));

    private void InitData()
    {
        _lblHead.Text = _t.F("headLabel", _svc.GetHeadRef());

        // _cboStartType.SelectedIndex = 0 below fires SelectedIndexChanged → ApplyStartTypeRule,
        // which populates the "based on" combo for the initial type.

        // Detect git-flow type of the currently checked-out branch so the Manage
        // panel opens already pointing at it (matching what the user is on).
        string current = _svc.GetCurrentBranch();
        int matchIdx = -1;
        string matchName = string.Empty;
        for (int i = 0; i < BranchHierarchyService.GitFlowTypes.Length; i++)
        {
            string prefix = _svc.GetGitFlowPrefix(BranchHierarchyService.GitFlowTypes[i]);
            if (prefix.Length > 0 && current.StartsWith(prefix, StringComparison.Ordinal))
            {
                matchIdx  = i;
                matchName = current[prefix.Length..];
                break;
            }
        }

        _cboStartType.SelectedIndex  = 0; // triggers prefix update
        _cboManageType.SelectedIndex = matchIdx >= 0 ? matchIdx : 0; // triggers branch reload

        if (matchName.Length > 0)
        {
            int branchIdx = _cboManageBranch.Items.IndexOf(matchName);
            if (branchIdx >= 0)
                _cboManageBranch.SelectedIndex = branchIdx;
            else
                _cboManageBranch.Text = matchName;
        }
    }

    private void ReloadManageBranches()
    {
        if (_cboManageType.SelectedIndex < 0) return;

        string prefix = _svc.GetGitFlowPrefix(_cboManageType.Text);
        _lblManagePrefix.Text = prefix;

        // The dropdown reflects only branches that exist LOCALLY. Reloaded after every
        // git flow command (see RunFlow), so a branch deleted by finish disappears here.
        var names = new List<string>();
        if (string.Equals(_cboManageType.Text, "support", StringComparison.OrdinalIgnoreCase))
        {
            // Support is driven from production tags (emergency workflow), so the branch
            // selector lists the existing tags instead of support/* branches.
            foreach (var t in _svc.GetTags())
                if (!names.Contains(t.FullName)) names.Add(t.FullName);
        }
        else
        {
            foreach (var name in _svc.GetGitFlowBranches(prefix))
                if (!names.Contains(name)) names.Add(name);
        }

        _cboManageBranch.Items.Clear();
        foreach (var n in names) _cboManageBranch.Items.Add(n);
        if (_cboManageBranch.Items.Count > 0)
            _cboManageBranch.SelectedIndex = 0;
        else
            _cboManageBranch.Text = string.Empty;
    }

    // ── Actions ─────────────────────────────────────────────────────────────

    private void DoStart()
    {
        string type = _cboStartType.Text;
        string name = Clean(_txtStartName.Text);
        if (name.Length == 0)
        {
            MessageBox.Show(_t["informBranchName"], _t["gitFlowTitle"],
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _txtResult.Clear();

        // Default base: hotfix/support branch from main; everything else from develop.
        string baseBranch = (_chkBasedOn.Checked && Clean(_cboBasedOn.Text).Length > 0)
            ? Clean(_cboBasedOn.Text)
            : (string.Equals(type, "hotfix",  StringComparison.OrdinalIgnoreCase) ||
               string.Equals(type, "support", StringComparison.OrdinalIgnoreCase)
                   ? _svc.GetGitFlowBranchName("main")
                   : _svc.GetGitFlowBranchName("develop"));

        string fullBranch = _svc.GetGitFlowPrefix(type) + name;

        // Reject before touching git if the branch already exists locally.
        if (_svc.GetLocalBranches().Any(b => string.Equals(b.FullName, fullBranch, StringComparison.Ordinal)))
        {
            MessageBox.Show(_t.F("branchExists", fullBranch), _t["gitFlowTitle"],
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // git checkout -b {prefix}{name} {base} — creates and switches in one command.
        bool ok = RunFlow($"checkout -b \"{fullBranch}\" \"{baseBranch}\"");
        _txtStartName.Clear();

        if (ok)
        {
            // Based on a custom branch (not main/develop): record a pure-visual child link so
            // the tree nests it under that base — no empty commit, git history stays clean.
            // Based on main/develop: keep the empty-commit trick so ancestry shows divergence.
            string mainName = _svc.GetGitFlowBranchName("main");
            string devName  = _svc.GetGitFlowBranchName("develop");
            bool baseIsRoot = string.Equals(baseBranch, mainName, StringComparison.Ordinal)
                           || string.Equals(baseBranch, devName,  StringComparison.Ordinal);

            if (_chkBasedOn.Checked && !baseIsRoot)
                _svc.SaveBasedOnOverride(fullBranch, baseBranch);
            else if (_chkBasedOn.Checked)
                RunFlow($"commit --allow-empty -m \"chore: start {fullBranch}\"");

            int typeIdx = _cboManageType.Items.IndexOf(type);
            if (typeIdx >= 0)
            {
                if (_cboManageType.SelectedIndex != typeIdx)
                    _cboManageType.SelectedIndex = typeIdx;
                int branchIdx = _cboManageBranch.Items.IndexOf(name);
                if (branchIdx >= 0)
                    _cboManageBranch.SelectedIndex = branchIdx;
                else
                    _cboManageBranch.Text = name;
            }
            // checkout -b already switched — reveal without a second checkout.
            RevealInTree(fullBranch, checkout: false);
            // Refresh the "based on" combo so a newly created feature/release branch
            // appears in the filtered list the next time it is used as a base.
            ApplyStartTypeRule();
        }
        else if (!IsDisposed) Activate();
    }

    /// <summary>
    /// After a successful git-flow op: optionally checks out the affected branch, then asks the
    /// owner ZimerfeldTree window to refresh its tree and reveal/select that branch. Keeps focus
    /// on this (modal) window. Pass an empty branch to only refresh.
    /// </summary>
    private void RevealInTree(string fullBranch, bool checkout)
    {
        fullBranch = (fullBranch ?? string.Empty).Trim();
        if (checkout && fullBranch.Length > 0)
            RunFlow($"checkout \"{fullBranch}\"", suppressError: true);
        RepoMutated?.Invoke(fullBranch.Length > 0 ? fullBranch : null);
        if (!IsDisposed) Activate();
    }

    private void DeleteRemoteBranchIfExists(string remote, string fullBranch)
    {
        if (remote.Length == 0) return;
        var (lsOut, lsCode) = _svc.RunGitFlow($"ls-remote --heads \"{remote}\" \"{fullBranch}\"");
        if (lsCode == 0 && lsOut.Trim().Length > 0)
            RunFlow($"push \"{remote}\" --delete \"{fullBranch}\"", suppressError: true);
        else
            _txtResult.AppendText($"\r\n\r\ncommand - git push {remote} --delete {fullBranch}" +
                                   $"\r\n\r\n{_t.F("skippedRemoteBranch", fullBranch)}");
    }

    private void DoPublish()
    {
        string type = _cboManageType.Text;
        string name = Clean(_cboManageBranch.Text);
        if (name.Length == 0) return;

        string fullBranch = _svc.GetGitFlowPrefix(type) + name;
        string remote     = _svc.GetDefaultRemote();
        if (remote.Length == 0)
        {
            MessageBox.Show(_t["noRemote"], _t["gitFlowTitle"],
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _txtResult.Clear();
        // git push --set-upstream <remote> <branch>
        if (RunFlow($"push --set-upstream \"{remote}\" \"{fullBranch}\""))
            RevealInTree(fullBranch, checkout: false);
    }

    private void DoTrack()
    {
        string type = _cboManageType.Text;
        string name = Clean(_cboManageBranch.Text);
        if (name.Length == 0) return;

        string fullBranch = _svc.GetGitFlowPrefix(type) + name;
        string remote     = _svc.GetDefaultRemote();
        if (remote.Length == 0)
        {
            MessageBox.Show(_t["noRemote"], _t["gitFlowTitle"],
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _txtResult.Clear();
        // Fetch so the remote-tracking ref exists before checkout --track.
        bool fetchRan = !_chkNoFetch.Checked;
        if (fetchRan)
            RunFlow($"fetch \"{remote}\"", suppressError: true);

        // git checkout -b {branch} --track {remote}/{branch}
        if (RunFlow($"checkout -b \"{fullBranch}\" --track \"{remote}/{fullBranch}\""))
            RevealInTree(fullBranch, checkout: false);
    }

    private void DoUpdate()
    {
        string type = _cboManageType.Text;
        string name = Clean(_cboManageBranch.Text);
        if (name.Length == 0) return;

        string fullBranch   = _svc.GetGitFlowPrefix(type) + name;
        string remote       = _svc.GetDefaultRemote();
        // Parent branch: hotfix/support branch from main; everything else from develop.
        string parentBranch = (string.Equals(type, "hotfix",  StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(type, "support", StringComparison.OrdinalIgnoreCase))
            ? _svc.GetGitFlowBranchName("main")
            : _svc.GetGitFlowBranchName("develop");

        _txtResult.Clear();
        bool fetchRan = !_chkNoFetch.Checked && remote.Length > 0;
        if (fetchRan)
            RunFlow($"fetch \"{remote}\"", suppressError: true);

        // Switch to the branch, then merge the parent (remote-tracking when available).
        if (!RunFlow($"checkout \"{fullBranch}\"")) return;

        bool ok = fetchRan
            ? RunFlow($"merge \"{remote}/{parentBranch}\"")
            : RunFlow($"merge \"{parentBranch}\"");

        if (ok)
            RevealInTree(fullBranch, checkout: false);
    }

    private void DoFinish()
    {
        string type = _cboManageType.Text;
        string name = Clean(_cboManageBranch.Text);
        if (name.Length == 0) return;

        bool isRelease = string.Equals(type, "release", StringComparison.OrdinalIgnoreCase);
        bool isHotfix  = string.Equals(type, "hotfix",  StringComparison.OrdinalIgnoreCase);
        bool isSupport = string.Equals(type, "support", StringComparison.OrdinalIgnoreCase);

        string fullBranch  = _svc.GetGitFlowPrefix(type) + name;
        string safeName    = Clean(name);
        string remote      = _svc.GetDefaultRemote();
        string mainBranch  = _svc.GetGitFlowBranchName("main");
        string devBranch   = _svc.GetGitFlowBranchName("develop");

        _txtResult.Clear();

        // ── 1. Optional fetch ──────────────────────────────────────────────────
        bool fetchRan = !_chkNoFetch.Checked && remote.Length > 0;
        if (fetchRan)
            RunFlow($"fetch \"{remote}\"", suppressError: true);

        // ── 2. Merge sequence ──────────────────────────────────────────────────
        // Set for feature/bugfix finishes to the branch the work was merged into (based-on
        // parent or develop); drives the based-on cleanup after the branch is deleted.
        string? featureMergeTarget = null;
        if (isRelease || isHotfix)
        {
            // → checkout main → merge --no-ff → tag → checkout develop → merge --no-ff
            if (!RunFlow($"checkout \"{mainBranch}\""))                   return;
            if (!RunFlow($"merge --no-ff \"{fullBranch}\""))              return;
            if (!RunFlow($"tag -a \"{safeName}\" -m \"{safeName}\""))     return;
            if (!RunFlow($"checkout \"{devBranch}\""))                    return;
            if (!RunFlow($"merge --no-ff \"{fullBranch}\""))              return;
        }
        else if (isSupport)
        {
            // support branches target main only; no tag, no develop merge
            if (!RunFlow($"checkout \"{mainBranch}\""))      return;
            if (!RunFlow($"merge --no-ff \"{fullBranch}\"")) return;
        }
        else
        {
            // feature / bugfix → merge into its based-on parent when it has one (so the work lands
            // in the parent node shown in the tree), otherwise into develop as usual.
            string? parent = _svc.GetBasedOnParent(fullBranch);
            bool parentUsable = !string.IsNullOrEmpty(parent)
                && !string.Equals(parent, mainBranch, StringComparison.Ordinal)
                && !string.Equals(parent, devBranch,  StringComparison.Ordinal)
                && _svc.GetLocalBranches().Any(b => string.Equals(b.FullName, parent, StringComparison.Ordinal));

            featureMergeTarget = parentUsable ? parent! : devBranch;

            if (!RunFlow($"checkout \"{featureMergeTarget}\"")) return;
            if (!RunFlow($"merge --no-ff \"{fullBranch}\""))    return;
        }

        // ── 3. Delete the local branch (unless keepBranch) ────────────────────
        if (!_chkKeep.Checked)
        {
            RunFlow($"branch -d \"{fullBranch}\"", suppressError: true);
            // Branch is gone: drop its based-on link and re-point its children to the
            // branch it was merged into, keeping the visual tree connected.
            if (featureMergeTarget != null)
                _svc.RebaseBasedOnOnFinish(fullBranch, featureMergeTarget);
        }

        // ── 4. Non-release: remove remote branch (unless keepBranch) and reveal ─
        if (!isRelease)
        {
            if (!_chkKeep.Checked)
                DeleteRemoteBranchIfExists(remote, fullBranch);
            RevealInTree(_svc.GetCurrentBranch(), checkout: false);
            return;
        }

        // ── 5. Release: push main, develop, tag; clean up remote branch ────────
        LastFinishedReleaseTag = safeName;

        if (remote.Length == 0)
        {
            MessageBox.Show(
                _t["releaseLocalNoRemote"],
                _t["gitFlowTitle"], MessageBoxButtons.OK, MessageBoxIcon.Information);
            RevealInTree(devBranch, checkout: false);
            return;
        }

        if (!RunFlow($"push \"{remote}\" \"{mainBranch}\"")) return;
        if (!RunFlow($"push \"{remote}\" \"{devBranch}\""))  return;

        // Push the release tag (created locally by the merge step above).
        RunFlow($"push \"{remote}\" \"refs/tags/{safeName}\"", suppressError: true);

        // Delete the remote release branch only if it still exists and keepBranch is off.
        if (!_chkKeep.Checked)
            DeleteRemoteBranchIfExists(remote, fullBranch);

        RunFlow($"checkout \"{devBranch}\"");
        RevealInTree(devBranch, checkout: false);
    }

    /// <summary>
    /// Runs <c>git {args}</c>, appends the result block to the textbox (auto-scrolling to the
    /// bottom) and returns true on exit code 0.  Call <c>_txtResult.Clear()</c> at the start of
    /// each user action so that every button press begins with a clean slate.
    /// </summary>
    private bool RunFlow(string args, bool suppressError = false)
    {
        string output;
        int code;
        Cursor = Cursors.WaitCursor;
        try
        {
            (output, code) = _svc.RunGitFlow(args);
            string body = output.Length == 0
                ? (code == 0 ? _t["cmdDone"] : _t["noOutput"])
                : output.Replace("\n", "\r\n");
            string block = $"command - git {args}\r\n\r\n{body}";
            if (_txtResult.TextLength > 0)
                _txtResult.AppendText("\r\n\r\n" + block);
            else
                _txtResult.AppendText(block);
        }
        finally
        {
            Cursor = Cursors.Default;
            if (!IsDisposed) Activate();
        }

        _lblHead.Text = _t.F("headLabel", _svc.GetHeadRef());
        ReloadManageBranches();

        if (code != 0 && !suppressError)
            ShowFlowError(output);

        return code == 0;
    }

    /// <summary>
    /// Shows a MessageBox after a failed git command. When the output indicates a missing
    /// base/target branch, it adds guidance for diagnosing the problem.
    /// </summary>
    private void ShowFlowError(string output)
    {
        bool missingBranch =
            output.Contains("does not exist", StringComparison.OrdinalIgnoreCase) ||
            output.Contains("not found",      StringComparison.OrdinalIgnoreCase) ||
            output.Contains("unknown revision", StringComparison.OrdinalIgnoreCase) ||
            output.Contains("pathspec",       StringComparison.OrdinalIgnoreCase);

        string msg = missingBranch ? _t["flowErrorMissingBranch"] : _t["flowErrorGeneric"];

        MessageBox.Show(msg, _t["flowErrorTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void ShowAbout()
    {
        MessageBox.Show(_t["aboutBody"], _t["aboutTitle"],
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>Removes double quotes to keep command arguments safe.</summary>
    private static string Clean(string s) => s.Trim().Replace("\"", "");
}
