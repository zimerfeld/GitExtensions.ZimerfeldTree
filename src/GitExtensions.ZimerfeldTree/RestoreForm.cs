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
    private readonly Translator _t = I18n.Load("ZimerfeldRestore");

    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GitExtensions", "ZimerfeldRestore.settings.json");

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
    private Button   _btnClose         = null!;
    private CheckBox _chkDeveloperMode = null!;

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
        Size            = new Size(560, 824 + SponsorBanner.PanelHeight);
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
        Controls.Add(SponsorBanner.Create()); // Top — GitHub Sponsors banner

        CancelButton  = _btnClose;
        Load         += (_, _) =>
        {
            InitData();
            ApplyOrClearTooltips(_chkDeveloperMode.Checked);
        };
        FormClosing  += (_, _) => SaveSettings();
    }

    /// <summary>Creates a tab page for a former group, hosting its controls at their existing coords.</summary>
    private TabPage AddTab(string title, params Control[] controls)
    {
        var page = new TabPage(title) { UseVisualStyleBackColor = true, Padding = new Padding(3) };
        page.Controls.AddRange(controls);
        _tabs.TabPages.Add(page);
        return page;
    }

    // ── Build UI ────────────────────────────────────────────────────────────

    private void BuildHeader()
    {
        _headerPanel = new Panel { Name = "headerPanel", Dock = DockStyle.Top, Height = 26 };

        _lnkAbout = new LinkLabel
        {
            Name      = "lnkAbout",
            Text      = _t["aboutLink"],
            AutoSize  = true,
            Dock      = DockStyle.Right,
            TextAlign = ContentAlignment.MiddleRight,
            Padding   = new Padding(8, 4, 0, 0)
        };
        _lnkAbout.LinkClicked += (_, _) => ShowAbout();

        _lblHead = new Label
        {
            Name      = "lblHead",
            TextAlign = ContentAlignment.MiddleLeft,
            Dock      = DockStyle.Fill
        };

        _headerPanel.Controls.Add(_lblHead);   // Fill
        _headerPanel.Controls.Add(_lnkAbout);  // Right
    }

    private void BuildEmergencyTab()
    {
        var lblBranch = new Label
        {
            Text      = _t["branchLabel"],
            AutoSize  = false,
            Bounds    = new Rectangle(12, 26, 54, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboEmergencyBranch = new ComboBox
        {
            Name          = "cboEmergencyBranch",
            DropDownStyle = ComboBoxStyle.DropDownList,
            Sorted        = true,
            Bounds        = new Rectangle(70, 24, 210, 22)
        };

        var lblTag = new Label
        {
            Text      = _t["tagLabel"],
            AutoSize  = false,
            Bounds    = new Rectangle(296, 26, 36, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboEmergencyTag = new ComboBox
        {
            Name          = "cboEmergencyTag",
            DropDownStyle = ComboBoxStyle.DropDown,
            Bounds        = new Rectangle(334, 24, 188, 22),
            DropDownWidth = 260
        };

        _btnEmergencyRestore = new Button
        {
            Name   = "btnEmergencyRestore",
            Text   = _t["restoreToTag"],
            Bounds = new Rectangle(190, 62, 160, 26)
        };
        _btnEmergencyRestore.Click += BtnEmergencyRestore_Click;

        _btnEmergencyReset = new Button
        {
            Name      = "btnEmergencyReset",
            Text      = _t["resetToTag"],
            ForeColor = Color.DarkRed,
            Bounds    = new Rectangle(360, 62, 160, 26)
        };
        _btnEmergencyReset.Click += BtnEmergencyReset_Click;

        AddTab(_t["emergencyGroup"],
            lblBranch, _cboEmergencyBranch, lblTag, _cboEmergencyTag,
            _btnEmergencyRestore, _btnEmergencyReset);
    }

    private void BuildRestoreFileTab()
    {
        var lblHash = new Label
        {
            Text      = _t["commitHash"],
            AutoSize  = false,
            Bounds    = new Rectangle(12, 26, 90, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboRestoreHash = new ComboBox
        {
            Name          = "cboRestoreHash",
            DropDownStyle = ComboBoxStyle.DropDown,
            Bounds        = new Rectangle(106, 24, 270, 22),
            DropDownWidth = 380
        };

        var lblFile = new Label
        {
            Text      = _t["fileRelative"],
            AutoSize  = false,
            Bounds    = new Rectangle(12, 54, 172, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _txtRestoreFile = new TextBox
        {
            Name            = "txtRestoreFile",
            PlaceholderText = _t["filePlaceholder"],
            Bounds          = new Rectangle(188, 52, 218, 22)
        };

        _btnRestoreFile = new Button
        {
            Name   = "btnRestoreFile",
            Text   = _t["restoreFileBtn"],
            Bounds = new Rectangle(384, 82, 144, 24)
        };
        _btnRestoreFile.Click += BtnRestoreFile_Click;

        AddTab(_t["restoreFileGroup"], lblHash, _cboRestoreHash, lblFile, _txtRestoreFile, _btnRestoreFile);
    }

    private void BuildCherryPickTab()
    {
        var lblHash = new Label
        {
            Text      = _t["commits"],
            AutoSize  = false,
            Bounds    = new Rectangle(12, 22, 76, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboCherryHash = new ComboBox
        {
            Name          = "cboCherryHash",
            DropDownStyle = ComboBoxStyle.DropDown,
            Bounds        = new Rectangle(92, 20, 286, 22),
            DropDownWidth = 380
        };
        _btnCherryPick = new Button
        {
            Name   = "btnCherryPick",
            Text   = _t["applyCherryPick"],
            Bounds = new Rectangle(384, 20, 144, 24)
        };
        _btnCherryPick.Click += BtnCherryPick_Click;

        AddTab(_t["cherryPickGroup"], lblHash, _cboCherryHash, _btnCherryPick);
    }

    private void BuildResetTab()
    {
        var lblBranch = new Label
        {
            Text      = _t["branchLabel"],
            AutoSize  = false,
            Bounds    = new Rectangle(12, 26, 54, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboBranch = new ComboBox
        {
            Name          = "cboBranch",
            DropDownStyle = ComboBoxStyle.DropDownList,
            Sorted        = true,
            Bounds        = new Rectangle(70, 24, 210, 22)
        };

        var lblHash = new Label
        {
            Text      = _t["commitHash"],
            AutoSize  = false,
            Bounds    = new Rectangle(12, 54, 90, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboResetHash = new ComboBox
        {
            Name          = "cboResetHash",
            DropDownStyle = ComboBoxStyle.DropDown,
            Bounds        = new Rectangle(106, 52, 270, 22),
            DropDownWidth = 380
        };

        _rdMixed = new RadioButton
        {
            Text    = _t["resetMixed"],
            Bounds  = new Rectangle(12, 82, 350, 20),
            Checked = true
        };
        _rdSoft = new RadioButton
        {
            Text   = _t["resetSoft"],
            Bounds = new Rectangle(12, 104, 310, 20)
        };
        _rdHard = new RadioButton
        {
            Text      = _t["resetHard"],
            Bounds    = new Rectangle(12, 126, 310, 20),
            ForeColor = Color.DarkRed
        };

        _btnReset = new Button
        {
            Name   = "btnReset",
            Text   = _t["resetBtn"],
            Bounds = new Rectangle(384, 122, 144, 24)
        };
        _btnReset.Click += BtnReset_Click;

        AddTab(_t["resetGroup"], lblBranch, _cboBranch, lblHash, _cboResetHash, _rdMixed, _rdSoft, _rdHard, _btnReset);
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

        // "Modo Developer" toggles the debug TYPE/ID tooltips live. Defaults to the value passed in
        // (the owner ZimerfeldTree's Show-Debug state).
        _chkDeveloperMode = new CheckBox
        {
            Name     = "chkDeveloperMode",
            Text     = _t["developerMode"],
            AutoSize = true,
            Checked  = _showControlIds
        };
        _chkDeveloperMode.CheckedChanged += (_, _) => ApplyOrClearTooltips(_chkDeveloperMode.Checked);

        // Docked bottom bar: close button centered, the Developer-mode checkbox pinned left (mirrors ZimerfeldTree).
        _bottomPanel = new Panel { Name = "bottomPanel", Dock = DockStyle.Bottom, Height = 40 };
        _bottomPanel.Controls.Add(_btnClose);
        _bottomPanel.Controls.Add(_chkDeveloperMode);
        _bottomPanel.Layout += (_, _) =>
        {
            _btnClose.Location = new Point(
                (_bottomPanel.Width  - _btnClose.Width)  / 2,
                (_bottomPanel.Height - _btnClose.Height) / 2);
            _chkDeveloperMode.Location = new Point(
                8, (_bottomPanel.Height - _chkDeveloperMode.Height) / 2);
        };
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
        foreach (var r in refs)
        {
            _cboRestoreHash.Items.Add(r);
            _cboCherryHash.Items.Add(r);
            _cboResetHash.Items.Add(r);
        }

        var saved = LoadSettings();
        RestoreSettings(saved, refs, branches);

        // fallback: if no saved branch selection, pick develop or index 0
        if (_cboBranch.SelectedItem is null)
        {
            string? develop = branches.FirstOrDefault(b => b == "develop");
            if (develop != null) _cboBranch.SelectedItem  = develop;
            else if (branches.Count > 0) _cboBranch.SelectedIndex = 0;
        }

        // Emergency branch defaults to the currently checked-out branch (the one most likely
        // to be rolled back), falling back to main/master or index 0.
        if (_cboEmergencyBranch.SelectedItem is null && _cboEmergencyBranch.Items.Count > 0)
        {
            string current = _svc.GetCurrentBranch();
            int idx = _cboEmergencyBranch.Items.IndexOf(current);
            if (idx < 0) idx = _cboEmergencyBranch.Items.IndexOf("main");
            if (idx < 0) idx = _cboEmergencyBranch.Items.IndexOf("master");
            _cboEmergencyBranch.SelectedIndex = idx >= 0 ? idx : 0;
        }
    }

    private List<CommitRef> LoadCommitRefs()
    {
        var (output, _) = _svc.RunGitFlow("log --oneline --all -200");
        var refs = new List<CommitRef>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Trim().Split(' ', 2);
            if (parts.Length == 2 && parts[0].Length >= 7)
                refs.Add(new CommitRef(parts[1], parts[0]));
        }
        return refs;
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
            var settings = new Dictionary<string, string>
            {
                ["emergencyBranch"] = _cboEmergencyBranch.SelectedItem as string ?? string.Empty,
                ["emergencyTag"]    = _cboEmergencyTag.Text.Trim(),
                ["restoreHash"] = HashOf(_cboRestoreHash),
                ["restoreFile"] = _txtRestoreFile.Text.Trim(),
                ["cherryHash"]  = HashOf(_cboCherryHash),
                ["resetBranch"] = _cboBranch.SelectedItem as string ?? string.Empty,
                ["resetHash"]   = HashOf(_cboResetHash),
                ["resetMode"]   = _rdHard.Checked ? "hard" : _rdSoft.Checked ? "soft" : "mixed"
            };
            File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(settings));
        }
        catch { }
    }

    private void RestoreSettings(Dictionary<string, string> saved, List<CommitRef> refs, List<string> branches)
    {
        if (saved.TryGetValue("emergencyBranch", out var eb) && eb.Length > 0)
        {
            int idx = _cboEmergencyBranch.Items.IndexOf(eb);
            if (idx >= 0) _cboEmergencyBranch.SelectedIndex = idx;
        }
        if (saved.TryGetValue("emergencyTag", out var et) && et.Length > 0)
        {
            int idx = _cboEmergencyTag.Items.IndexOf(et);
            if (idx >= 0) _cboEmergencyTag.SelectedIndex = idx;
            else          _cboEmergencyTag.Text = et;
        }

        if (saved.TryGetValue("restoreHash", out var rh) && rh.Length > 0)
        {
            var match = refs.FirstOrDefault(r => r.Hash == rh || r.Hash.StartsWith(rh));
            if (match != null) _cboRestoreHash.SelectedItem = match;
            else               _cboRestoreHash.Text = rh;
        }
        if (saved.TryGetValue("restoreFile", out var rf) && rf.Length > 0)
            _txtRestoreFile.Text = rf;

        if (saved.TryGetValue("cherryHash", out var ch) && ch.Length > 0)
        {
            var match = refs.FirstOrDefault(r => r.Hash == ch || r.Hash.StartsWith(ch));
            if (match != null) _cboCherryHash.SelectedItem = match;
            else               _cboCherryHash.Text = ch;
        }

        if (saved.TryGetValue("resetBranch", out var rb) && rb.Length > 0)
        {
            int idx = branches.IndexOf(rb);
            if (idx >= 0) _cboBranch.SelectedIndex = idx;
        }
        if (saved.TryGetValue("resetHash", out var resetH) && resetH.Length > 0)
        {
            var match = refs.FirstOrDefault(r => r.Hash == resetH || r.Hash.StartsWith(resetH));
            if (match != null) _cboResetHash.SelectedItem = match;
            else               _cboResetHash.Text = resetH;
        }
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

    private static string HashOf(ComboBox cbo) =>
        cbo.SelectedItem is CommitRef r ? r.Hash : cbo.Text.Trim();

    private sealed class CommitRef
    {
        public string Hash { get; }
        private readonly string _display;
        public CommitRef(string name, string hash) { Hash = hash; _display = $"{name}  →  {hash}"; }
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
