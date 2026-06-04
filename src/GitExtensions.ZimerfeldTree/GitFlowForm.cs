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

    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GitExtensions", "ZimerfeldTree.gitflowsettings.json");

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

    // ── Bottom close button ──
    private Button _btnClose = null!;

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

        Text            = "ZimerfeldTree - GitFlow";
        Size            = new Size(688, 824);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        StartPosition   = FormStartPosition.Manual;   // caller controls position (side-by-side)
        Font            = new Font("Segoe UI", 9f);
        Icon            = TreeOfLifeIcon.ForForm();

        BuildHeader();
        BuildStartGroup();
        BuildManageGroup();
        BuildResultGroup();
        BuildCloseButton();

        CancelButton = _btnClose;

        SetTabOrder();
        Load += (_, _) =>
        {
            InitData();
            ApplySettings();
            if (_showControlIds) ApplyControlTooltips();
        };
    }

    // ── Build UI ────────────────────────────────────────────────────────────

    private void BuildHeader()
    {
        _lblHead = new Label
        {
            Name      = "lblHead",
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(120, 10, 400, 20),
            Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        _lnkAbout = new LinkLabel
        {
            Name      = "lnkAbout",
            Text      = "About GitFlow",
            AutoSize  = true,
            Anchor    = AnchorStyles.Top | AnchorStyles.Right,
            Location  = new Point(ClientSize.Width - 110, 12)
        };
        _lnkAbout.LinkClicked += (_, _) => ShowAbout();

        Controls.AddRange([_lblHead, _lnkAbout]);
    }

    private void BuildStartGroup()
    {
        // "Start branch" is now the GroupBox title — no separate lblType inside.
        _grpStart = new GroupBox
        {
            Name   = "grpStart",
            Text   = "Start branch",
            Bounds = new Rectangle(8, 36, 664, 120),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        // Row 1 — type selector (col label x=12, col input x=108)
        var lblType = new Label
        {
            Name      = "lblStartType",
            Text      = "Type:",
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
        };

        // Row 2 — expected name (prefix label + text input + Start button)
        var lblName = new Label
        {
            Name      = "lblStartName",
            Text      = "Expected name:",
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
            Text   = "Start",
            Bounds = new Rectangle(560, 52, 90, 26),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _btnStart.Click += (_, _) => DoStart();

        // Row 3 — optional base branch
        _chkBasedOn = new CheckBox
        {
            Name   = "chkBasedOn",
            Text   = "based on:",
            Bounds = new Rectangle(108, 84, 90, 22)
        };
        _chkBasedOn.CheckedChanged += (_, _) => _cboBasedOn.Enabled = _chkBasedOn.Checked;

        _cboBasedOn = new ComboBox
        {
            Name          = "cboBasedOn",
            Bounds        = new Rectangle(202, 82, 348, 24),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Enabled       = false,
            Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        _grpStart.Controls.AddRange(
            [lblType, _cboStartType, lblName, _lblStartPrefix, _txtStartName, _btnStart,
             _chkBasedOn, _cboBasedOn]);
        Controls.Add(_grpStart);
    }

    private void BuildManageGroup()
    {
        _grpManage = new GroupBox
        {
            Name   = "grpManage",
            Text   = "Manage existing branches",
            Bounds = new Rectangle(8, 164, 664, 192),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        // Row 1 — type selector (aligned with grpStart: label x=12, input x=108)
        var lblType = new Label
        {
            Name      = "lblManageType",
            Text      = "Type:",
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
            Text      = "Branch:",
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
            Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        // ── Buttons row: 4 × 140 px, gap 18 px, left margin 12 px ──────────────
        // (12)[Publish 140](18)[Track 140](18)[Update 140](18)[Finish 140](12) = 638 ✓
        _btnPublish = new Button { Name = "btnPublish", Text = "Publish", Bounds = new Rectangle( 12, 84, 140, 26) };
        _btnPublish.Click += (_, _) => DoPublish();

        _btnTrack = new Button { Name = "btnTrack", Text = "Track",   Bounds = new Rectangle(170, 84, 140, 26) };
        _btnTrack.Click += (_, _) => DoTrack();

        _btnUpdate = new Button { Name = "btnUpdate", Text = "Update",  Bounds = new Rectangle(328, 84, 140, 26) };
        _btnUpdate.Click += (_, _) => DoUpdate();

        _btnFinish = new Button { Name = "btnFinish", Text = "Finish",  Bounds = new Rectangle(486, 84, 140, 26) };
        _btnFinish.Click += (_, _) => DoFinish();

        // ── Checkboxes stacked below the Finish button ─────────────────────────
        _chkKeep = new CheckBox
        {
            Name    = "chkKeep",
            Text    = "Keep branch after finish",
            Bounds  = new Rectangle(486, 114, 170, 20),
            Checked = true  // default: keep branch; overridden by saved settings on Load
        };
        _chkKeep.CheckedChanged += (_, _) => SaveSettings(_chkKeep.Checked, _chkNoFetch.Checked);

        _chkNoFetch = new CheckBox
        {
            Name   = "chkNoFetch",
            Text   = "No fetch (--no-fetch)",
            Bounds = new Rectangle(486, 136, 170, 20)
        };
        _chkNoFetch.CheckedChanged += (_, _) => SaveSettings(_chkKeep.Checked, _chkNoFetch.Checked);

        // Descriptions for Track/Update live only in the "Sobre" (About) link text.

        _grpManage.Controls.AddRange(
        [
            lblType, _cboManageType, lblBranch, _lblManagePrefix, _cboManageBranch,
            _btnPublish, _btnTrack, _btnUpdate, _btnFinish, _chkKeep, _chkNoFetch
        ]);
        Controls.Add(_grpManage);
    }

    private void BuildResultGroup()
    {
        // Height reduced by 48 px to leave room for the Fechar button below.
        _grpResult = new GroupBox
        {
            Name   = "grpResult",
            Text   = "Result of git flow command run",
            Bounds = new Rectangle(8, 364, 664, 362),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        _txtResult = new TextBox
        {
            Name       = "txtResult",
            Multiline  = true,
            ReadOnly   = true,
            ScrollBars = ScrollBars.Both,
            WordWrap   = false,
            BackColor  = SystemColors.Window,
            Bounds     = new Rectangle(10, 22, 644, 310),
            Anchor     = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Font       = new Font("Consolas", 9f)
        };
        _grpResult.Controls.Add(_txtResult);
        Controls.Add(_grpResult);
    }

    private void BuildCloseButton()
    {
        _btnClose = new Button
        {
            Name         = "btnClose",
            Text         = "Fechar",
            Width        = 90,
            Height       = 28,
            Bounds       = new Rectangle(299, 736, 90, 28),
            Anchor       = AnchorStyles.Bottom,
            DialogResult = DialogResult.Cancel
        };
        _btnClose.Click += (_, _) => Close();
        Controls.Add(_btnClose);
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
    }

    private static void SetTooltipsRecursive(Control parent, ToolTip tip)
    {
        foreach (Control c in parent.Controls)
        {
            if (c.Name.Length > 0)
                tip.SetToolTip(c, $"TYPE: {c.GetType().Name}\nID: {c.Name}\nNome: {c.Text}");
            SetTooltipsRecursive(c, tip);
        }
    }

    // ── Settings persistence (checkboxes) ───────────────────────────────────

    private void ApplySettings()
    {
        var (keepBranch, noFetch) = LoadSettings();
        _chkKeep   .Checked = keepBranch;
        _chkNoFetch.Checked = noFetch;
    }

    private static (bool keepBranch, bool noFetch) LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsFilePath)) return (true, false);
            string json = File.ReadAllText(SettingsFilePath);
            bool keep    = json.Contains("\"keepBranchAfterFinish\":true");
            bool noFetch = json.Contains("\"noFetch\":true");
            return (keep, noFetch);
        }
        catch { return (true, false); }
    }

    private static void SaveSettings(bool keepBranch, bool noFetch)
    {
        try
        {
            string dir = Path.GetDirectoryName(SettingsFilePath)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(SettingsFilePath,
                $"{{\"keepBranchAfterFinish\":{(keepBranch ? "true" : "false")}," +
                $"\"noFetch\":{(noFetch ? "true" : "false")}}}");
        }
        catch { }
    }

    // ── Data ────────────────────────────────────────────────────────────────

    private void InitData()
    {
        _lblHead.Text = "HEAD:  " + _svc.GetHeadRef();

        _cboBasedOn.Items.Clear();
        _cboBasedOn.Items.Add("develop");
        foreach (var b in _svc.GetLocalBranches())
            if (!_cboBasedOn.Items.Contains(b.FullName))
                _cboBasedOn.Items.Add(b.FullName);
        _cboBasedOn.SelectedIndex = 0; // "develop" is the default base
        _cboBasedOn.Enabled = _chkBasedOn.Checked;

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
        foreach (var name in _svc.GetGitFlowBranches(prefix))
            if (!names.Contains(name)) names.Add(name);

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
            MessageBox.Show("Informe o nome da branch.", "GitFlow",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string baseArg = string.Empty;
        if (_chkBasedOn.Checked)
        {
            string baseBranch = Clean(_cboBasedOn.Text);
            if (baseBranch.Length > 0) baseArg = $" \"{baseBranch}\"";
        }

        bool ok = RunFlow($"flow {type} start \"{name}\"{baseArg}");
        _txtStartName.Clear();

        // Pre-select the new branch in "Manage existing branches".
        if (ok)
        {
            int typeIdx = _cboManageType.Items.IndexOf(type);
            if (typeIdx >= 0)
            {
                if (_cboManageType.SelectedIndex != typeIdx)
                    _cboManageType.SelectedIndex = typeIdx; // triggers ReloadManageBranches
                int branchIdx = _cboManageBranch.Items.IndexOf(name);
                if (branchIdx >= 0)
                    _cboManageBranch.SelectedIndex = branchIdx;
                else
                    _cboManageBranch.Text = name;
            }

            // Checkout the new branch and reveal it in the ZimerfeldTree (behind this modal).
            RevealInTree(_svc.GetGitFlowPrefix(type) + name, checkout: true);
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
            RunFlow($"checkout \"{fullBranch}\"", append: true, suppressError: true);
        RepoMutated?.Invoke(fullBranch.Length > 0 ? fullBranch : null);
        if (!IsDisposed) Activate();
    }

    private void DoPublish()
    {
        string type = _cboManageType.Text;
        string name = Clean(_cboManageBranch.Text);
        if (name.Length == 0) return;
        if (RunFlow($"flow {type} publish \"{name}\""))
            RevealInTree(_svc.GetGitFlowPrefix(type) + name, checkout: true);
    }

    private void DoTrack()
    {
        string type = _cboManageType.Text;
        string name = Clean(_cboManageBranch.Text);
        if (name.Length == 0) return;
        if (RunFlow($"flow {type} track \"{name}\""))
            RevealInTree(_svc.GetGitFlowPrefix(type) + name, checkout: true);
    }

    private void DoUpdate()
    {
        string type = _cboManageType.Text;
        string name = Clean(_cboManageBranch.Text);
        if (name.Length == 0) return;
        if (RunFlow($"flow {type} update \"{name}\""))
            RevealInTree(_svc.GetGitFlowPrefix(type) + name, checkout: true);
    }

    private void DoFinish()
    {
        string type = _cboManageType.Text;
        string name = Clean(_cboManageBranch.Text);
        if (name.Length == 0) return;

        bool isRelease = string.Equals(type, "release", StringComparison.OrdinalIgnoreCase);
        bool isHotfix  = string.Equals(type, "hotfix",  StringComparison.OrdinalIgnoreCase);

        string flags = string.Empty;
        if (_chkKeep.Checked)    flags += "-k ";
        if (_chkNoFetch.Checked) flags += "--no-fetch ";

        // release/hotfix finish creates an annotated tag. Without -m, git-flow opens an
        // editor for the tag message; in this non-interactive host that yields an empty
        // message and git aborts with "fatal: no tag message?". Supply the version as the
        // tag message so the finish runs unattended.
        if (isRelease || isHotfix)
            flags += $"-m \"{name}\" ";

        // Fetch before finishing to avoid divergences between local and remote branches.
        if (!_chkNoFetch.Checked)
            RunFlow("fetch", append: false, suppressError: true);

        // Publish branch to origin before finishing so the remote is up-to-date.
        // For release this also prevents "couldn't find remote ref release/X" during git-flow's fetch.
        // Skipped when --no-fetch is checked (user explicitly opts out of remote operations).
        if (!_chkNoFetch.Checked)
        {
            string pushRemote = _svc.GetDefaultRemote();
            if (pushRemote.Length > 0)
            {
                string fullBranch = _svc.GetGitFlowPrefix(type) + name;
                if (!RunFlow($"push {pushRemote} {fullBranch}")) return;
            }
        }

        bool ok = RunFlow($"flow {type} finish {flags}\"{name}\"", append: true, suppressError: true);

        if (!ok)
        {
            string resultText = _txtResult.Text;
            if (resultText.Contains("merge is already in progress", StringComparison.OrdinalIgnoreCase))
            {
                ok = ResolveInProgressMerge(type, name, flags, resultText);
            }
            else
            {
                ShowFlowError(resultText);
            }
        }

        // After a successful "release" finish: push master, push develop,
        // and (if both succeed) checkout develop.
        if (!ok) return;

        // finish deletes the affected branch and leaves you on its base (e.g. develop).
        // Reveal whatever branch is now current (no checkout — git-flow already switched).
        if (!isRelease)
        {
            RevealInTree(_svc.GetCurrentBranch(), checkout: false);
            return;
        }

        LastFinishedReleaseTag = name;

        string master  = _svc.GetGitFlowBranchName("master");
        string develop = _svc.GetGitFlowBranchName("develop");
        string remote  = _svc.GetDefaultRemote();
        if (remote.Length == 0)
        {
            MessageBox.Show(
                "Release finalizada localmente, mas nenhum remoto configurado para push.",
                "GitFlow", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!RunFlow($"push {remote} {master}",  append: true)) return;
        if (!RunFlow($"push {remote} {develop}", append: true)) return;

        // Push the release tag to the remote (git flow finish creates it locally only).
        RunFlow($"push {remote} refs/tags/{name}", append: true, suppressError: true);

        // Delete the remote release branch only if it still exists. git-flow usually removes it
        // during finish, so a blind delete prints a noisy "remote ref does not exist" error.
        // Probe with ls-remote first and skip cleanly (with a friendly note) when it is already gone.
        string remoteReleaseBranch = _svc.GetGitFlowPrefix(type) + name;
        var (lsOut, lsCode) = _svc.RunGitFlow($"ls-remote --heads {remote} {remoteReleaseBranch}");
        if (lsCode == 0 && lsOut.Trim().Length > 0)
            RunFlow($"push {remote} --delete {remoteReleaseBranch}", append: true, suppressError: true);
        else
            _txtResult.Text += $"\r\n\r\ncommand - git push {remote} --delete {remoteReleaseBranch}" +
                               $"\r\n\r\n(pulado: a branch remota '{remoteReleaseBranch}' já não existe)";

        RunFlow($"checkout {develop}", append: true);
        RevealInTree(develop, checkout: false);
    }

    /// <summary>
    /// Runs <c>git {args}</c>, shows the result in the textbox and returns true on exit code 0.
    /// When <paramref name="append"/> is true, the new output block is appended to the existing
    /// result text (so multi-step flows like release-finish + push + checkout stay visible).
    /// </summary>
    private bool RunFlow(string args, bool append = false, bool suppressError = false)
    {
        string output;
        int code;
        Cursor = Cursors.WaitCursor;
        try
        {
            (output, code) = _svc.RunGitFlow(args);
            string body = output.Length == 0
                ? (code == 0 ? "(comando concluído)" : "(sem saída)")
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

        _lblHead.Text = "HEAD:  " + _svc.GetHeadRef();
        ReloadManageBranches();

        if (code != 0 && !suppressError)
            ShowFlowError(output);

        return code == 0;
    }

    /// <summary>
    /// Shows a MessageBox after a failed git flow command. When the output points to a missing
    /// base/production branch (the common git-flow-next finish failure), it adds guidance.
    /// </summary>
    private static void ShowFlowError(string output)
    {
        bool missingBase =
            output.Contains("couldn't find remote ref", StringComparison.OrdinalIgnoreCase) ||
            output.Contains("does not exist", StringComparison.OrdinalIgnoreCase) ||
            output.Contains("start point branch", StringComparison.OrdinalIgnoreCase);

        string msg = missingBase
            ? "O git flow não encontrou a branch base/produção (ex.: 'main' ou 'develop').\n\n" +
              "Verifique se ela existe localmente:\n" +
              "    git branch --list main master develop\n\n" +
              "E a configuração do git flow:\n" +
              "    git config gitflow.branch.main\n" +
              "    git config gitflow.branch.develop\n\n" +
              "Crie a branch que falta ou ajuste a config. Se a falha for ao buscar do remoto, " +
              "marque \"No fetch (--no-fetch)\" e tente novamente."
            : "O comando git flow falhou. Veja os detalhes na janela de resultado.";

        MessageBox.Show(msg, "GitFlow — falha", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    /// <summary>
    /// Handles "merge is already in progress" from git-flow-next.
    /// Aborts the stuck merge state and retries the original finish.
    /// </summary>
    private bool ResolveInProgressMerge(string type, string name, string flags, string errorText)
    {
        var m = System.Text.RegularExpressions.Regex.Match(
            errorText,
            @"in progress for branch '([^']+)'",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        string stuckBranch = m.Success ? m.Groups[1].Value : string.Empty;
        string stuckType   = string.Empty;
        string stuckName   = string.Empty;
        if (stuckBranch.Length > 0)
        {
            int sl = stuckBranch.IndexOf('/');
            if (sl > 0) { stuckType = stuckBranch[..sl]; stuckName = stuckBranch[(sl + 1)..]; }
        }

        // Save current branch so we can return to it after abort.
        string originalBranch = _svc.GetCurrentBranch();

        // Abort the stuck merge state (both git-flow's lock and git's MERGE_HEAD).
        if (stuckType.Length > 0 && stuckName.Length > 0)
            RunFlow($"flow {stuckType} finish --abort \"{stuckName}\"",
                append: true, suppressError: true);

        RunFlow("merge --abort", append: true, suppressError: true);

        // Deadlock recovery: git-flow-next keeps a persistent state file
        // (.git/gitflow/state/*.json) that survives even when git has no MERGE_HEAD.
        // In that case both aborts above fail and the only fix is deleting the stale file.
        if (_svc.ClearGitFlowState())
            _txtResult.Text += "\r\n\r\n(estado órfão do git-flow removido: .git/gitflow/state)";

        // Return to original branch if abort changed it.
        string currentBranch = _svc.GetCurrentBranch();
        if (originalBranch.Length > 0 &&
            !string.Equals(originalBranch, currentBranch, StringComparison.OrdinalIgnoreCase))
        {
            RunFlow($"checkout {originalBranch}", append: true, suppressError: true);
        }

        // Retry the original finish.
        bool ok = RunFlow($"flow {type} finish {flags}\"{name}\"",
            append: true, suppressError: true);
        if (!ok) ShowFlowError(_txtResult.Text);
        return ok;
    }

    private void ShowAbout()
    {
        MessageBox.Show(
            "Botões:\n" +
            "  Publish — envia a branch para o remoto (git flow <tipo> publish).\n" +
            "  Track   — cria branch local rastreando a branch remota de mesmo nome.\n" +
            "  Update  — traz commits da branch pai (ex.: develop) para a branch atual.\n" +
            "  Finish  — mescla de volta e exclui a branch (git flow <tipo> finish).\n\n" +
            "Checkboxes do Finish:\n" +
            "  Keep branch after finish — mantém a branch após a mesclagem (flag -k).\n" +
            "  No fetch (--no-fetch)   — não busca no remoto antes de finalizar.\n\n" +
            "Requer git-flow-next instalado.",
            "About GitFlow", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>Removes double quotes to keep command arguments safe.</summary>
    private static string Clean(string s) => s.Trim().Replace("\"", "");
}
