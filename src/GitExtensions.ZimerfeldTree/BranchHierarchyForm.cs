// BranchHierarchyForm.cs — Main WinForms window for ZimerfeldTree plugin
// MIT License — Copyright (c) 2026 Zimerfeld

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace GitExtensions.ZimerfeldTree;

/// <summary>
/// Non-modal window that displays local branches, remote-tracking branches and tags in a
/// path-based hierarchy.  Stays open alongside GitExtensions while the user works.
/// </summary>
public sealed class BranchHierarchyForm : Form
{
    // ── Services ─────────────────────────────────────────────────────────────
    private readonly BranchHierarchyService _svc;
    private readonly Action? _notifyRepoChanged; // called after checkout so GitExtensions refreshes
    /// <summary>
    /// Delegate provided by the plugin that opens the native GitExtensions commit dialog in-process.
    /// The string argument is the working directory to commit against — passed so the dialog targets
    /// the repository currently selected in cboRepo (and thus its checked-out branch shown in lblBranch),
    /// not the GitExtensions host's active repository.
    /// Returns true = commits were made, false = dialog closed without committing, null = unavailable (fall back).
    /// </summary>
    private readonly Func<IWin32Window, string, bool?>? _openCommitDialog;
    /// <summary>
    /// Delegate provided by the plugin that opens the native GitExtensions push dialog in-process.
    /// Returns true if push was completed, false otherwise.
    /// </summary>
    private readonly Func<IWin32Window, bool>? _openPushDialog;

    // ── Cached data ───────────────────────────────────────────────────────────
    private List<BranchInfo>             _localBranches  = [];
    private List<BranchInfo>             _remoteBranches = [];
    private List<BranchInfo>             _tags           = [];
    private Dictionary<string, string?>  _localParentMap  = []; // real git ancestry
    private Dictionary<string, string?>  _remoteParentMap = [];
    private bool                         _gitFlowForced   = false;
    private bool                         _gitFlowUserToggled = false; // user clicked the button → stop auto-organizing
    private Action?                      _postRefreshAction;          // runs once after the next RefreshTreeAsync completes

    // Open modal child dialogs, tracked so they can be force-closed when GitExtensions switches the
    // active repository (Change Working Directory): a GitFlow/Restore window must not linger over a
    // repo it no longer matches. Set while ShowDialog runs (DoGitFlow/DoRestore), cleared on close.
    private GitFlowForm? _gitFlowForm;
    private RestoreForm? _restoreForm;

    // ── Controls ─────────────────────────────────────────────────────────────
    private Panel            _topPanel    = null!;
    private Label            _lblWD       = null!;
    private ComboBox         _cboRepo     = null!;
    private Label            _lblBranch   = null!;
    private Panel            _filterPanel = null!;
    private TextBox          _txtFilter   = null!;
    private Button           _btnRefresh  = null!;
    private Panel            _warnPanel   = null!;
    private Label            _warnLabel   = null!;
    private Button           _btnGitFlow          = null!;
    private Panel            _gitFlowInitPanel    = null!;
    private Button           _btnGitFlowInit      = null!;
    private Button           _btnGitFlowDedicated = null!;
    private Button           _btnRestore           = null!;
    private Panel            _gitFlowButtonPanel  = null!;
    private Button           _btnPull             = null!;
    private Button           _btnPush             = null!;
    private Button           _btnCommitDedicated  = null!;
    private Button           _btnExcluir          = null!;
    private TreeView         _tree        = null!;
    private StatusStrip      _status      = null!;
    private ToolStripStatusLabel _statusLbl = null!;

    // ── Bottom panel ──────────────────────────────────────────────────────────
    private Panel    _bottomPanel  = null!;
    private Button   _btnClose    = null!;
    private CheckBox _chkShowDebug = null!;
    private CheckBox _chkDeveloperMode = null!;
    private Label    _lblLanguage  = null!;
    private ComboBox _cboLanguage  = null!;
    private bool     _suppressLangEvent;
    private LinkLabel _lnkAbout   = null!;

    // ── Localization ────────────────────────────────────────────────────────────
    // Loaded for the active language; reassigned by ApplyLanguage when the user switches
    // languages via the bottom-panel dropdown (so the open window re-localizes live).
    private Translator _t = I18n.Load("ZimerfeldTree");

    // ── Loading overlay ───────────────────────────────────────────────────────
    private Panel       _loadingOverlay   = null!;
    private ProgressBar _progressBar      = null!;
    private Label       _loadingTitle     = null!;
    private ListBox     _stepsList        = null!;
    private Button      _btnCancelRefresh = null!;
    private bool        _isRefreshing;
    private CancellationTokenSource? _refreshCts;

    // ── Tooltip engine ────────────────────────────────────────────────────────
    private readonly ToolTip _mainTooltip = new ToolTip();

    // ── Tree expand/collapse state persistence ────────────────────────────────
    /// <summary>Per-repo set of expanded node paths (key = workingDir, value = stable path strings).</summary>
    private Dictionary<string, HashSet<string>> _treeStateByRepo = [];
    /// <summary>True while we are restoring saved state — suppresses AfterExpand/AfterCollapse saves.</summary>
    private bool _restoringState;
    /// <summary>True between a left double-click MouseDown and its NodeMouseDoubleClick — cancels the default expand/collapse toggle so double-click only does checkout.</summary>
    private bool _suppressDoubleClickToggle;
    /// <summary>Debounce timer that delays disk writes when many nodes expand/collapse rapidly.</summary>
    private System.Windows.Forms.Timer? _saveDebounce;
    private static readonly string StateFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GitExtensions", "ZimerfeldTree.treestate.json");

    private static readonly string UiSettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GitExtensions", "ZimerfeldTree.uisettings.json");

    private static bool LoadShowControlIds()
    {
        try
        {
            if (!File.Exists(UiSettingsPath)) return false;
            return File.ReadAllText(UiSettingsPath).Contains("\"showControlIds\":true");
        }
        catch { return false; }
    }

    private static bool LoadDeveloperMode()
    {
        try
        {
            if (!File.Exists(UiSettingsPath)) return false;
            return File.ReadAllText(UiSettingsPath).Contains("\"developerMode\":true");
        }
        catch { return false; }
    }

    /// <summary>Persists both UI toggles (Show Debug + Modo Developer) to the settings file.</summary>
    private void SaveUiSettings()
    {
        try
        {
            string dir = Path.GetDirectoryName(UiSettingsPath)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(UiSettingsPath,
                $"{{\"showControlIds\":{(_chkShowDebug.Checked ? "true" : "false")}," +
                $"\"developerMode\":{(_chkDeveloperMode.Checked ? "true" : "false")}}}");
        }
        catch { }
    }

    // ── Tree section roots ────────────────────────────────────────────────────
    private TreeNode _localRoot   = null!;
    private TreeNode _remotesRoot = null!;
    private TreeNode _tagsRoot    = null!;

    // ── Context menu ──────────────────────────────────────────────────────────
    private ContextMenuStrip   _ctxMenu     = null!;
    private ToolStripMenuItem  _miPull      = null!;
    private ToolStripMenuItem  _miPush      = null!;
    private ToolStripMenuItem  _miCommit    = null!;
    private ToolStripMenuItem  _miCheckout  = null!;
    private ToolStripMenuItem  _miNewBranch = null!;
    private ToolStripMenuItem  _miMerge     = null!;
    private ToolStripMenuItem  _miRebase    = null!;
    private ToolStripMenuItem  _miRename    = null!;
    private ToolStripMenuItem  _miDelete    = null!;
    private ToolStripMenuItem  _miGitFlow      = null!;
    private ToolStripMenuItem  _miVoltarVersao = null!;
    private ToolStripMenuItem  _miExpand    = null!;
    private ToolStripMenuItem  _miCollapse  = null!;
    private ToolStripMenuItem  _miRefresh   = null!;

    // ─────────────────────────────────────────────────────────────────────────
    public BranchHierarchyForm(string workingDir, Action? notifyRepoChanged = null,
        Func<IWin32Window, string, bool?>? openCommitDialog = null,
        Func<IWin32Window, bool>? openPushDialog = null)
    {
        _svc = new BranchHierarchyService(workingDir);
        _notifyRepoChanged  = notifyRepoChanged;
        _openCommitDialog   = openCommitDialog;
        _openPushDialog     = openPushDialog;
        _treeStateByRepo    = LoadTreeState();
        InitializeComponent();
        LoadRepositories();   // combo population only — reads the settings XML, no git subprocess
        // FIRST LOAD: verify the repository against the GitFlow hierarchy rule and pre-fetch all
        // data BEFORE the window is shown — synchronously, with NO overlay. The tree is fully
        // built here, so the plugin's _form.Show() reveals an already-populated window. Subsequent
        // refreshes (button/menu/mutations) still use the async overlay path (RefreshTreeAsync).
        InitialLoadSync();
        FormClosed += (_, _) => { _saveDebounce?.Dispose(); SaveTreeState(); };
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Called by the plugin when GitExtensions switches the active repository.</summary>
    public void UpdateWorkingDir(string newDir)
    {
        // When the GitExtensions "Change Working Directory" dropdown actually switches repos, drop any
        // open GitFlow/Restore child window — it belongs to the old repo. The cboRepo dropdown and the
        // lblBranch label below are updated to the new repo here and by the subsequent RefreshTree.
        bool repoChanged = !string.Equals(newDir, _svc.WorkingDir, StringComparison.OrdinalIgnoreCase);
        if (repoChanged) CloseChildDialogs();

        _svc.WorkingDir = newDir;
        _gitFlowUserToggled = false; // re-enable auto-organization for the new repo
        if (!_cboRepo.Items.Contains(newDir))
            _cboRepo.Items.Add(newDir);
        _cboRepo.SelectedItem = newDir;
    }

    /// <summary>
    /// Closes the modal GitFlow / Restore child dialogs if either is open. Calling <see cref="Form.Close"/>
    /// on a modal form ends its ShowDialog loop, after which DoGitFlow/DoRestore resume and clear the field.
    /// Safe to call when nothing is open (no-op).
    /// </summary>
    private void CloseChildDialogs()
    {
        if (_gitFlowForm is { IsDisposed: false } gf) gf.Close();
        if (_restoreForm is { IsDisposed: false } rf) rf.Close();
    }

    /// <summary>
    /// Re-reads branches from git asynchronously and rebuilds the tree.
    /// Shows the "Carregando…" overlay while reading. Concurrent calls are collapsed into one.
    /// </summary>
    public void RefreshTree() => _ = RefreshTreeAsync(showOverlay: true);

    /// <summary>
    /// Loads all branch/tag data on a background thread, optionally showing a centered
    /// progress overlay, then rebuilds the tree on the UI thread.
    /// </summary>
    private async Task RefreshTreeAsync(bool showOverlay)
    {
        if (_isRefreshing) return;
        _isRefreshing = true;

        // Cancel any previous refresh that might still be in-flight, then create a fresh token.
        _refreshCts?.Cancel();
        _refreshCts = new CancellationTokenSource();
        var token = _refreshCts.Token;

        if (showOverlay)
        {
            _progressBar.Value        = 0;
            _stepsList.Items.Clear();
            _stepsList.Items.Add($"• {_t["progStarting"]}");
            _btnCancelRefresh.Enabled = true;
            _btnCancelRefresh.Text    = _t["abortOperation"];
            _loadingOverlay.Location  = new Point(
                (ClientSize.Width  - _loadingOverlay.Width)  / 2,
                (ClientSize.Height - _loadingOverlay.Height) / 2);
            _loadingOverlay.Visible = true;
            _loadingOverlay.BringToFront();
            SetFormEnabled(false);
        }

        IProgress<(int pct, string msg)>? ip = showOverlay
            ? new Progress<(int pct, string msg)>(p =>
              {
                  _progressBar.Value = p.pct;
                  _stepsList.Items.Add($"• {p.msg}");
                  int last = _stepsList.Items.Count - 1;
                  if (last >= 0) _stepsList.TopIndex = last;
              })
            : null;

        RepoData data;
        try
        {
            data = await Task.Run(() => FetchRepoData(ip, token), token);
        }
        catch (OperationCanceledException)
        {
            // User cancelled — silently restore UI without touching the existing tree data.
            _isRefreshing = false;
            if (!IsDisposed && showOverlay)
            {
                _loadingOverlay.Visible = false;
                SetFormEnabled(true);
            }
            return;
        }
        catch (Exception ex)
        {
            _isRefreshing = false;
            if (!IsDisposed)
            {
                if (showOverlay)
                {
                    _loadingOverlay.Visible = false;
                    SetFormEnabled(true);
                }
                MessageBox.Show(_t.F("errLoadRepo", ex.Message),
                    _t["appTitle"], MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return;
        }

        if (IsDisposed) { _isRefreshing = false; return; }

        ApplyRepoData(data);

        var postAction = _postRefreshAction;
        _postRefreshAction = null;
        if (postAction != null)
            postAction.Invoke();            // reveal a specific branch/tag (scrolls to it)
        else
            ScrollTreeToTop();              // normal reload: vertical scrollbar starts at the top

        if (showOverlay)
        {
            // Let the user see the final "Concluído." step for a moment before the overlay closes.
            await Task.Delay(1000);
            _loadingOverlay.Visible = false;
            SetFormEnabled(true);
        }
        _isRefreshing = false;
    }

    /// <summary>Immutable snapshot of everything read from the repository in a single pass.</summary>
    private sealed record RepoData(
        List<BranchInfo>            Local,
        List<BranchInfo>            Remote,
        List<BranchInfo>            Tags,
        Dictionary<string, string?> LMap,
        Dictionary<string, string?> RMap,
        int                         Pending);

    /// <summary>
    /// Reads all branch/tag data and computes the parent maps. Pure git work — safe to run on a
    /// background thread (RefreshTreeAsync) or synchronously before the window is shown
    /// (<see cref="InitialLoadSync"/>). <paramref name="ip"/> is null when no overlay is shown.
    /// </summary>
    private RepoData FetchRepoData(IProgress<(int pct, string msg)>? ip, CancellationToken token)
    {
        ip?.Report((10, _t["progLocal"]));
        var local = _svc.GetLocalBranches();
        token.ThrowIfCancellationRequested();
        // Drop stale remote-tracking refs (branches deleted on the remote) so the tree reflects
        // reality. Gated on ip != null: only the async/overlay refresh prunes — the synchronous
        // window-open load (ip == null) must stay fast and offline-safe, never touching the network.
        if (ip != null) _svc.PruneRemotes();
        token.ThrowIfCancellationRequested();
        ip?.Report((30, _t["progRemote"]));
        var remote = _svc.GetRemoteBranches();
        token.ThrowIfCancellationRequested();
        ip?.Report((50, _t["progTags"]));
        var tags = _svc.GetTags();
        token.ThrowIfCancellationRequested();
        ip?.Report((65, _t["progLocalHierarchy"]));
        var lMap = _svc.BuildParentMap(local);
        token.ThrowIfCancellationRequested();
        ip?.Report((80, _t["progRemoteHierarchy"]));
        var rMap = _svc.BuildRemoteParentMap(remote);
        token.ThrowIfCancellationRequested();
        ip?.Report((92, _t["progSync"]));
        var tracking = _svc.GetBranchTrackingInfo();
        foreach (var b in local)
            if (tracking.TryGetValue(b.FullName, out var ti))
            {
                b.HasUpstream = ti.hasUpstream;
                b.AheadCount  = ti.ahead;
                b.BehindCount = ti.behind;
            }
        token.ThrowIfCancellationRequested();
        ip?.Report((96, _t["progPending"]));
        int pending = _svc.GetPendingChangesCount();
        ip?.Report((100, _t["progDone"]));
        return new RepoData(local, remote, tags, lMap, rMap, pending);
    }

    /// <summary>
    /// Applies a <see cref="RepoData"/> snapshot to the UI: verifies whether the hierarchy matches
    /// the GitFlow rule (<see cref="UpdateGitFlowWarning"/>), rebuilds the tree, and refreshes
    /// labels/counters. UI-thread only. The caller handles scroll/reveal.
    /// </summary>
    private void ApplyRepoData(RepoData data)
    {
        _localBranches   = data.Local;
        _remoteBranches  = data.Remote;
        _tags            = data.Tags;
        _localParentMap  = data.LMap;
        _remoteParentMap = data.RMap;

        _tree.BeginUpdate();
        try
        {
            UpdateGitFlowWarning();   // verifies whether the real hierarchy follows the GitFlow rule
            // Even in forced-GitFlow mode, based-on links override the rigid map so the
            // visual hierarchy is honored in every mode.
            var localMap  = _gitFlowForced ? _svc.OverlayBasedOn(BuildGitFlowParentMap(_localBranches)) : _localParentMap;
            var remoteMap = _gitFlowForced ? BuildGitFlowRemoteParentMap(_remoteBranches)               : _remoteParentMap;
            RebuildAllSections(_txtFilter?.Text.Trim() ?? string.Empty, localMap, remoteMap);
            // Restore the saved expand/collapse state ONLY when the native handle exists. During the
            // first load (InitialLoadSync, in the constructor) there is no handle yet, so node.Expand()
            // /CollapseAll() would not stick; the Shown handler restores it once instead. This avoids a
            // double, partial restore (constructor without handle + Shown) that lost the saved state.
            if (_tree.IsHandleCreated) ExpandRoots();
            UpdateStatus();
            UpdateBranchLabel();
            UpdatePullPushButtons();
        }
        finally { _tree.EndUpdate(); }
        ApplyCheckBoxVisibility();   // hide checkboxes on section/folder nodes (no-op until the handle exists)
        UpdateDeleteButtonText();    // rebuilt tree → no checks → reset the Excluir label
        UpdateCommitActionTexts(data.Pending);
    }

    /// <summary>
    /// First-time load, run synchronously in the constructor BEFORE the window is shown. Verifies
    /// whether GitFlow is initialized and whether the hierarchy follows the GitFlow rule, pre-fetches
    /// all repository data, and builds the populated tree — with NO overlay. Per-node checkbox hiding
    /// and the initial scroll run later from the Shown event (they need the native tree handle).
    /// </summary>
    private void InitialLoadSync()
    {
        UpdateGitFlowInitButton();   // btnGitFlowInit — is GitFlow initialized as defined?
        try
        {
            var data = FetchRepoData(null, CancellationToken.None);
            ApplyRepoData(data);     // UpdateGitFlowWarning inside verifies the hierarchy rule
        }
        catch (Exception ex)
        {
            MessageBox.Show(_t.F("errLoadRepo", ex.Message),
                _t["appTitle"], MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ── Initialization ────────────────────────────────────────────────────────

    private void InitializeComponent()
    {
        SuspendLayout();

        Text            = _t["title"];
        // Height includes the sponsor banner (PanelHeight) docked above the working-directory panel.
        // Width 700 (client ~684) gives the bottom bar room for the longer Portuguese "Modo
        // Desenvolvedor" checkbox so it does not collide with the centered Fechar button.
        Size            = new Size(700, 760 + SponsorBanner.PanelHeight);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;   // não redimensionável pelo usuário
        MaximizeBox     = false;                          // maximizar redimensionaria a janela
        MinimizeBox     = true;
        KeyPreview      = true;
        Font            = new Font("Segoe UI", 9f);
        Icon            = PluginIcon.ForForm();

        BuildTopPanel();
        BuildAboutLink();
        BuildFilterPanel();
        BuildWarnPanel();
        BuildGitFlowInitPanel();
        BuildGitFlowButtonPanel();
        BuildTreeView();
        BuildContextMenu();
        BuildStatusStrip();
        BuildBottomPanel();
        BuildLoadingOverlay();
        SetTabOrder();
        ApplyLanguage();   // sets every localizable text from the active-language dictionary

        // Layout order (Dock fills from bottom and top inward, Fill takes the remainder).
        // Added last = topmost for DockStyle.Top; visual order top→bottom:
        //   _topPanel, _gitFlowInitPanel, _filterPanel, _warnPanel, _gitFlowButtonPanel, _tree (Fill), _bottomPanel, _status
        Controls.Add(_tree);                // Fill
        Controls.Add(_gitFlowButtonPanel);  // Top — just above the tree
        Controls.Add(_warnPanel);           // Top
        Controls.Add(_filterPanel);         // Top
        Controls.Add(_gitFlowInitPanel);    // Top — GitFlow Initialize button (above filter)
        Controls.Add(_topPanel);            // Top
        Controls.Add(SponsorBanner.Create(_lnkAbout)); // Top (topmost) — Sponsors banner hosts lnkAbout
        Controls.Add(_status);         // Bottom
        Controls.Add(_bottomPanel);    // Bottom (above status)
        Controls.Add(_loadingOverlay); // Floats above everything (BringToFront when shown)

        CancelButton = _btnClose;

        // Restore debug state and lay out the GitFlow buttons.
        // NOTE: the GitFlow verification (UpdateGitFlowInitButton -> IsGitFlowConfigured)
        // is intentionally NOT done here. It fires ~10 synchronous git subprocesses
        // (git config --get ×8 + 2× rev-parse) which, running on the UI thread before the
        // first WM_PAINT, freeze the window and paint the top controls as empty skeletons.
        // It now runs later, AFTER the whole screen is drawn (Refresh() in RefreshTreeAsync)
        // and right before the tree loads — via SetFormEnabled(false) and, after the data
        // pass, UpdateGitFlowWarning -> UpdateGitFlowInitButton.
        Load += (_, _) =>
        {
            ApplyControlTooltips(_chkShowDebug.Checked);
            LayoutGitFlowButtons();
        };

        // The tree is already populated synchronously in the constructor (InitialLoadSync), before
        // this window is shown — so there is no first-time overlay. The remaining first-show work
        // needs the native tree handle (created when the window is shown), and Shown fires once,
        // before the first paint (so no flicker):
        //   • ExpandRoots() re-applies the persisted expand/collapse state — node.Expand()/
        //     CollapseAll() do NOT stick on the native control before the handle exists, so the
        //     saved states must be restored here for them to be remembered across sessions.
        //   • ApplyCheckBoxVisibility() hides section/folder checkboxes (native message).
        //   • ScrollTreeToTop() resets the vertical scrollbar.
        Shown += (_, _) =>
        {
            _tree.BeginUpdate();
            try { ExpandRoots(); }
            finally { _tree.EndUpdate(); }
            ApplyCheckBoxVisibility();
            ScrollTreeToTop();
            // The synchronous open path is offline-safe and shows the LAST-KNOWN ahead/behind counts.
            // Now that the window is visible, contact the remote off the UI thread to refresh the
            // current branch's counts and correct the Pull/Push controls + branch label once.
            _ = RefreshRemoteStatusAsync();
        };

        ResumeLayout(false);
        PerformLayout();
    }

    private void BuildTopPanel()
    {
        _topPanel = new Panel { Name = "topPanel", Dock = DockStyle.Top, Height = 82 };

        var table = new TableLayoutPanel
        {
            Name        = "tblTop",
            Dock        = DockStyle.Fill,
            ColumnCount = 1,
            RowCount    = 3,
            Padding     = new Padding(6, 15, 6, 4),
            Margin      = Padding.Empty
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _lblWD = new Label
        {
            Name     = "lblWD",
            Text     = _t["workingDirectory"],
            AutoSize = true,
            Font     = new Font(Font, FontStyle.Bold),
            Margin   = new Padding(0, 0, 0, 2)
        };

        _cboRepo = new ComboBox
        {
            Name          = "cboRepo",
            DropDownStyle = ComboBoxStyle.DropDownList,
            Sorted        = true,
            Dock          = DockStyle.Fill,
            Margin        = new Padding(0, 0, 0, 2)
        };
        _cboRepo.SelectedIndexChanged += CboRepo_SelectedIndexChanged;

        _lblBranch = new Label
        {
            Name     = "lblBranch",
            AutoSize = true,
            Text     = "Branch: ",
            Margin   = Padding.Empty
        };

        table.Controls.Add(_lblWD,     0, 0);
        table.Controls.Add(_cboRepo,   0, 1);
        table.Controls.Add(_lblBranch, 0, 2);

        _topPanel.Controls.Add(table);
    }

    private void BuildAboutLink()
    {
        // Hosted inside the sponsor banner (see SponsorBanner.Create) so it sits at the same height
        // as picSponsor; the banner handles its right-edge, vertically-centered positioning.
        _lnkAbout = new LinkLabel
        {
            Name     = "lnkAbout",
            Text     = _t["aboutTree"],
            AutoSize = true
        };
        _lnkAbout.LinkClicked += (_, _) => ShowAboutTree();
    }

    private void BuildFilterPanel()
    {
        _filterPanel = new Panel { Name = "filterPanel", Dock = DockStyle.Top, Height = 28, Padding = new Padding(4, 2, 4, 2) };

        _txtFilter = new TextBox
        {
            Name            = "txtFilter",
            Dock            = DockStyle.Fill,
            PlaceholderText = _t["filterPlaceholder"]
        };
        _txtFilter.TextChanged += (_, _) => ApplyFilter(_txtFilter.Text.Trim());

        _btnRefresh = new Button
        {
            Name   = "btnRefresh",
            Text   = "↺",
            Dock   = DockStyle.Right,
            Width  = 64,   // wider Refresh button; txtFilter (Dock=Fill) narrows to match
            Height = 24,
            Font   = new Font(Font, FontStyle.Bold)
        };
        _btnRefresh.Click += (_, _) => RefreshTree();
        // Use the same icon as the context-menu "Atualizar" action; drop the ↺ glyph so the
        // compact button shows just the icon (falls back to ↺ if the resource is missing).
        if (LoadMenuIcon("ctx-refresh.png") is { } refreshImg)
        {
            _btnRefresh.Image = refreshImg;
            _btnRefresh.Text  = string.Empty;
        }

        _filterPanel.Controls.Add(_txtFilter);
        _filterPanel.Controls.Add(_btnRefresh);
    }

    private void BuildWarnPanel()
    {
        _warnLabel = new Label
        {
            Name      = "warnLabel",
            Text      = string.Empty,
            ForeColor = Color.DarkRed,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize  = false
        };

        _btnGitFlow = new Button
        {
            Name  = "btnGitFlow",
            Width = 160,
            Height = 24,
            Text  = _t["organizeAsGitFlow"]
        };
        _btnGitFlow.Click += BtnGitFlow_Click;
        ApplyButtonIcon(_btnGitFlow, "ctx-gitflow.png");

        _warnPanel = new Panel
        {
            Name    = "warnPanel",
            Dock    = DockStyle.Top,
            Height  = 28,
            Visible = false,
            Padding = new Padding(4, 2, 4, 2)
        };
        _warnPanel.Controls.Add(_warnLabel);
        _warnPanel.Controls.Add(_btnGitFlow);

        // btnGitFlow pinned to the right edge (vertically centered); warnLabel fills the space to its left.
        _warnPanel.Layout += (_, _) =>
        {
            int h = _warnPanel.ClientSize.Height;
            _btnGitFlow.Location = new Point(
                _warnPanel.ClientSize.Width - 4 - _btnGitFlow.Width,
                (h - _btnGitFlow.Height) / 2);
            _warnLabel.Bounds = new Rectangle(4, 0, Math.Max(0, _btnGitFlow.Left - 8), h);
        };
    }

    private void BuildGitFlowInitPanel()
    {
        _btnGitFlowInit = new Button
        {
            Name   = "btnGitFlowInit",
            Width  = 160,
            Height = 24,
            Text   = _t["gitFlowInitialize"]
        };
        _btnGitFlowInit.Click += (_, _) => DoGitFlowInit();
        ApplyButtonIcon(_btnGitFlowInit, "ctx-gitflow.png");

        _gitFlowInitPanel = new Panel
        {
            Name    = "gitFlowInitPanel",
            Dock    = DockStyle.Top,
            Height  = 28,
            Padding = new Padding(4, 2, 4, 2)
        };
        _gitFlowInitPanel.Controls.Add(_btnGitFlowInit);

        // Keep button centred horizontally whenever the panel is laid out.
        _gitFlowInitPanel.Layout += (_, _) =>
            _btnGitFlowInit.Location = new Point(
                (_gitFlowInitPanel.Width  - _btnGitFlowInit.Width)  / 2,
                (_gitFlowInitPanel.Height - _btnGitFlowInit.Height) / 2);
    }

    private void BuildGitFlowButtonPanel()
    {
        _btnPull = new Button { Name = "btnPull", Text = _t["pull"], Width = 80, Height = 24 };
        _btnPull.Click += (_, _) => DoPull();

        _btnPush = new Button { Name = "btnPush", Text = _t["push"], Width = 80, Height = 24 };
        _btnPush.Click += (_, _) => DoPush();

        _btnCommitDedicated = new Button { Name = "btnCommitDedicated", Text = _t["commit"], Width = 100, Height = 24 };
        _btnCommitDedicated.Click += (_, _) => DoCommit();

        // Deletes the checked branch/tag leaves (or the selected node when none are checked),
        // mirroring the context-menu "Excluir". Its text tracks the number of checked checkboxes.
        _btnExcluir = new Button { Name = "btnExcluir", Text = _t["delete"], Width = 100, Height = 24 };
        _btnExcluir.Click += (_, _) => DoDelete();

        _btnGitFlowDedicated = new Button
        {
            Name   = "btnGitFlowDedicated",
            Text   = _t["gitFlow"],
            Width  = 100,   // placeholder — LayoutGitFlowButtons sets equal widths to fill the panel
            Height = 24
        };
        _btnGitFlowDedicated.Click += (_, _) => DoGitFlow();

        _btnRestore = new Button
        {
            Name   = "btnRestore",
            Text   = _t["restore"],
            Width  = 100,   // placeholder — LayoutGitFlowButtons sets equal widths to fill the panel
            Height = 24
        };
        _btnRestore.Click += (_, _) => DoRestore();

        // Each button shows the same icon as its matching right-click action. Pull/Push have no
        // context-menu counterpart, so they stay text-only.
        ApplyButtonIcon(_btnCommitDedicated, "ctx-commit.png");
        ApplyButtonIcon(_btnExcluir,         "ctx-delete.png");
        ApplyButtonIcon(_btnGitFlowDedicated, "ctx-gitflow.png");
        ApplyButtonIcon(_btnRestore,         "ctx-restore.png");

        _gitFlowButtonPanel = new Panel { Name = "gitFlowButtonPanel", Dock = DockStyle.Top, Height = 32 };
        _gitFlowButtonPanel.Controls.AddRange([_btnPull, _btnPush, _btnCommitDedicated, _btnExcluir, _btnGitFlowDedicated, _btnRestore]);
        // Positions are set explicitly by LayoutGitFlowButtons() — no Layout event so resize doesn't move buttons.
    }

    private void BuildTreeView()
    {
        _tree = new GatedTreeView
        {
            Name          = "tree",
            Dock          = DockStyle.Fill,
            ShowLines     = true,
            ShowPlusMinus = true,
            ShowRootLines = true,
            HideSelection = false,
            CheckBoxes    = true,   // multi-select; checkboxes are hidden on non-leaf nodes (see ApplyCheckBoxVisibility)
            DrawMode      = TreeViewDrawMode.OwnerDrawText,
            Font          = new Font("Segoe UI", 9f),
            ImageList     = NodeIcons.GetList()
        };

        _tree.DrawNode              += Tree_DrawNode;
        _tree.NodeMouseDoubleClick  += Tree_NodeMouseDoubleClick;
        _tree.KeyDown               += Tree_KeyDown;
        _tree.MouseDown             += Tree_MouseDown;
        _tree.BeforeExpand          += Tree_BeforeExpandCollapse;
        _tree.BeforeCollapse        += Tree_BeforeExpandCollapse;
        _tree.AfterExpand           += Tree_AfterExpand;
        _tree.AfterCollapse         += Tree_AfterCollapse;
        _tree.BeforeCheck           += Tree_BeforeCheck;   // only branch/tag leaves are checkable
        _tree.AfterCheck            += Tree_AfterCheck;    // refresh the Excluir button count

        _localRoot = new TreeNode("LOCAL (0)")
        {
            Tag = SectionTag.Local,
            ImageIndex = NodeIcons.SectionLocal, SelectedImageIndex = NodeIcons.SectionLocal
        };
        _remotesRoot = new TreeNode("REMOTES (0)")
        {
            Tag = SectionTag.Remotes,
            ImageIndex = NodeIcons.SectionRemotes, SelectedImageIndex = NodeIcons.SectionRemotes
        };
        _tagsRoot = new TreeNode("TAGS (0)")
        {
            Tag = SectionTag.Tags,
            ImageIndex = NodeIcons.SectionTags, SelectedImageIndex = NodeIcons.SectionTags
        };

        _tree.Nodes.AddRange([_localRoot, _remotesRoot, _tagsRoot]);
    }

    private static Image? LoadMenuIcon(string fileName)
    {
        try
        {
            using var stream = typeof(BranchHierarchyForm).Assembly
                .GetManifestResourceStream($"GitExtensions.ZimerfeldTree.Resources.{fileName}");
            return stream is null ? null : new Bitmap(stream);
        }
        catch { return null; }
    }

    /// <summary>
    /// Places the given context-menu icon at the left of a button, before its text, so the
    /// button mirrors the icon shown for the same action in the right-click menu. No-op when
    /// the resource is missing (the button keeps its text only). Image+text are centred as a
    /// group, so callers should leave enough width for both.
    /// </summary>
    private static void ApplyButtonIcon(Button btn, string iconFile)
    {
        if (LoadMenuIcon(iconFile) is { } img)
        {
            btn.Image             = img;
            btn.TextImageRelation = TextImageRelation.ImageBeforeText;
        }
    }

    private void BuildContextMenu()
    {
        _miPull      = new ToolStripMenuItem(_t.F("ctxPull", 0));
        _miPush      = new ToolStripMenuItem(_t.F("ctxPush", 0));
        _miCommit    = new ToolStripMenuItem(_t["commit"]);
        _miCheckout  = new ToolStripMenuItem(_t["ctxCheckout"]);
        _miNewBranch = new ToolStripMenuItem(_t["ctxNewBranch"]);
        _miMerge     = new ToolStripMenuItem(_t["ctxMerge"]);
        _miRebase    = new ToolStripMenuItem(_t["ctxRebase"]);
        _miRename    = new ToolStripMenuItem(_t["ctxRename"]);
        _miDelete    = new ToolStripMenuItem(_t["ctxDelete"]);
        _miGitFlow      = new ToolStripMenuItem(_t["ctxGitFlow"]);
        _miVoltarVersao = new ToolStripMenuItem(_t["ctxRestore"]);
        _miExpand    = new ToolStripMenuItem(_t["ctxExpand"]);
        _miCollapse  = new ToolStripMenuItem(_t["ctxCollapse"]);
        _miRefresh   = new ToolStripMenuItem(_t["ctxRefresh"]);

        _miPull     .Image = LoadMenuIcon("ctx-pull.png");      // optional — null when absent
        _miPush     .Image = LoadMenuIcon("ctx-push.png");      // optional — null when absent
        _miCommit   .Image = LoadMenuIcon("ctx-commit.png");
        _miCheckout .Image = LoadMenuIcon("ctx-checkout.png");
        _miNewBranch.Image = LoadMenuIcon("ctx-new-branch.png");
        _miMerge    .Image = LoadMenuIcon("ctx-merge.png");
        _miRebase   .Image = LoadMenuIcon("ctx-rebase.png");
        _miRename   .Image = LoadMenuIcon("ctx-rename.png");
        _miDelete   .Image = LoadMenuIcon("ctx-delete.png");
        _miGitFlow     .Image = LoadMenuIcon("ctx-gitflow.png");
        _miVoltarVersao.Image = LoadMenuIcon("ctx-restore.png");
        _miExpand   .Image = LoadMenuIcon("ctx-expand.png");
        _miCollapse .Image = LoadMenuIcon("ctx-collapse.png");
        _miRefresh  .Image = LoadMenuIcon("ctx-refresh.png");

        _miPull     .Click += (_, _) => DoPull();
        _miPush     .Click += (_, _) => DoPush();
        _miCommit   .Click += (_, _) => DoCommit();
        _miCheckout .Click += (_, _) => DoCheckout();
        _miNewBranch.Click += (_, _) => DoNewBranch();
        _miMerge    .Click += (_, _) => DoMerge();
        _miRebase   .Click += (_, _) => DoRebase();
        _miRename   .Click += (_, _) => DoRename();
        _miDelete   .Click += (_, _) => DoDelete();
        _miGitFlow     .Click += (_, _) => DoGitFlow();
        _miVoltarVersao.Click += (_, _) => DoRestore();
        _miExpand  .Click += (_, _) => _tree.SelectedNode?.ExpandAll();
        _miCollapse.Click += (_, _) => { if (_tree.SelectedNode is { } n) CollapseRecursive(n); };
        _miRefresh  .Click += (_, _) => RefreshTree();

        _ctxMenu = new ContextMenuStrip();
        _ctxMenu.Opening += CtxMenu_Opening;
        _ctxMenu.Items.AddRange(
        [
            _miPull, _miPush,
            _miCommit,
            new ToolStripSeparator(),
            _miCheckout, _miNewBranch,
            new ToolStripSeparator(),
            _miMerge, _miRebase,
            new ToolStripSeparator(),
            _miRename, _miDelete,
            new ToolStripSeparator(),
            _miVoltarVersao,                 // GitFlow item removed from the context menu (use the GitFlow button)
            new ToolStripSeparator(),
            _miExpand, _miCollapse, _miRefresh
        ]);

        _tree.ContextMenuStrip = _ctxMenu;
    }

    private void BuildStatusStrip()
    {
        _status    = new StatusStrip { SizingGrip = false, Renderer = new NoGripRenderer() };
        _statusLbl = new ToolStripStatusLabel
        {
            Text      = _t.F("statusCounts", 0, 0, 0),
            Spring    = true,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _status.Items.Add(_statusLbl);
    }

    // Suppresses the StatusStrip sizing-grip image that SizingGrip=false alone does not remove.
    private sealed class NoGripRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderStatusStripSizingGrip(ToolStripRenderEventArgs e) { }
    }

    private void BuildBottomPanel()
    {
        _btnClose = new Button
        {
            Name         = "btnClose",
            Text         = _t["close"],
            Width        = 80,
            Height       = 26,
            DialogResult = DialogResult.Cancel
        };
        _btnClose.Click += (_, _) => Close();

        _chkShowDebug = new CheckBox
        {
            Name     = "chkShowDebug",
            Text     = _t["showDebug"],
            AutoSize = true,
            Checked  = LoadShowControlIds()
        };
        _chkDeveloperMode = new CheckBox
        {
            Name     = "chkDeveloperMode",
            Text     = _t["modoDeveloper"],
            AutoSize = true,
            Checked  = LoadDeveloperMode()
        };

        // Handlers wired after BOTH checkboxes exist (SaveUiSettings reads both).
        _chkShowDebug.CheckedChanged += (_, _) =>
        {
            SaveUiSettings();
            ApplyControlTooltips(_chkShowDebug.Checked);
        };
        _chkDeveloperMode.CheckedChanged += (_, _) =>
        {
            SaveUiSettings();
            // Turning Developer mode OFF drops any checked main/develop so they cannot be deleted.
            if (!_chkDeveloperMode.Checked) UncheckProtectedBranches();
        };

        // Language selector (right-aligned). Items + selection are populated by ApplyLanguage so
        // they stay localized; index order matches the AppLanguage enum (0=Automatic, 1=English,
        // 2=Portuguese). Changing it persists the choice and re-localizes this window live.
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

        _bottomPanel = new Panel { Name = "bottomPanel", Dock = DockStyle.Bottom, Height = 36 };
        _bottomPanel.Controls.Add(_btnClose);
        _bottomPanel.Controls.Add(_chkShowDebug);
        _bottomPanel.Controls.Add(_chkDeveloperMode);
        _bottomPanel.Controls.Add(_lblLanguage);
        _bottomPanel.Controls.Add(_cboLanguage);

        // Centre Fechar; pin chkShowDebug to the left, Modo Developer just to its right; pin the
        // Language label + dropdown to the right edge.
        _bottomPanel.Layout += (_, _) =>
        {
            int cy = (_bottomPanel.Height - _btnClose.Height) / 2;
            _btnClose.Location = new Point(
                (_bottomPanel.Width - _btnClose.Width) / 2, cy);
            _chkShowDebug.Location = new Point(
                8, (_bottomPanel.Height - _chkShowDebug.Height) / 2);
            _chkDeveloperMode.Location = new Point(
                _chkShowDebug.Right + 12, (_bottomPanel.Height - _chkDeveloperMode.Height) / 2);
            _cboLanguage.Location = new Point(
                _bottomPanel.Width - _cboLanguage.Width - 8,
                (_bottomPanel.Height - _cboLanguage.Height) / 2);
            _lblLanguage.Location = new Point(
                _cboLanguage.Left - _lblLanguage.Width - 6,
                (_bottomPanel.Height - _lblLanguage.Height) / 2);
        };
    }

    // ── Language selection ────────────────────────────────────────────────────────

    /// <summary>
    /// Persists the dropdown choice and re-localizes this window in place. The two modal child
    /// windows (GitFlow / Restore) read the choice when they next open, so no live push is needed.
    /// </summary>
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

    /// <summary>
    /// Reloads the active-language dictionary and reapplies every localizable text on this window.
    /// Called once during construction and again whenever the Language dropdown changes. Avoids the
    /// expensive git-config probe (UpdateGitFlowInitButton) so it is safe to run before first paint.
    /// </summary>
    private void ApplyLanguage()
    {
        _t = I18n.Load("ZimerfeldTree");

        // Window + top panel
        Text                       = _t["title"];
        _lblWD.Text                = _t["workingDirectory"];
        _lnkAbout.Text             = _t["aboutTree"];
        _txtFilter.PlaceholderText = _t["filterPlaceholder"];

        // Action buttons (static labels; the count-bearing ones are refreshed below)
        _btnGitFlowInit.Text      = _t["gitFlowInitialize"];
        _btnGitFlowDedicated.Text = _t["gitFlow"];
        _btnRestore.Text          = _t["restore"];

        // Bottom panel
        _btnClose.Text          = _t["close"];
        _chkShowDebug.Text      = _t["showDebug"];
        _chkDeveloperMode.Text  = _t["modoDeveloper"];
        _lblLanguage.Text       = _t["language"];

        // Context menu (the Commit/Delete items carry counts and are set on menu open)
        _miCheckout.Text     = _t["ctxCheckout"];
        _miNewBranch.Text    = _t["ctxNewBranch"];
        _miMerge.Text        = _t["ctxMerge"];
        _miRebase.Text       = _t["ctxRebase"];
        _miRename.Text       = _t["ctxRename"];
        _miDelete.Text       = _t["ctxDelete"];
        _miGitFlow.Text      = _t["ctxGitFlow"];
        _miVoltarVersao.Text = _t["ctxRestore"];
        _miExpand.Text       = _t["ctxExpand"];
        _miCollapse.Text     = _t["ctxCollapse"];
        _miRefresh.Text      = _t["ctxRefresh"];

        // Loading overlay (do not stomp the live abort-button text mid-refresh)
        _loadingTitle.Text = _t["loadingTitle"];
        if (!_isRefreshing) _btnCancelRefresh.Text = _t["abortOperation"];

        // Re-localize the GitFlow warning text + toggle button WITHOUT the expensive git-config
        // probe. GetGitFlowViolations reads only cached branch data (no subprocess); the warn panel
        // is hidden before the first data load, so this block is skipped during construction.
        if (_warnPanel.Visible)
        {
            var violations = GetGitFlowViolations();
            if (_gitFlowForced)
                _warnLabel.Text = violations.Count > 0
                    ? _t.F("gitFlowOutOrganizing", violations.Count)
                    : _t["gitFlowForcedView"];
            else
                _warnLabel.Text = _t.F("warnPrefix", violations.Count == 1
                    ? violations[0]
                    : _t.F("gitFlowViolationsCount", violations.Count));
        }
        _btnGitFlow.Text = _gitFlowForced ? _t["restoreRealHierarchy"] : _t["organizeAsGitFlow"];

        // Dynamic, count-bearing texts (cheap: cached counts + one git ref/pending probe, matching
        // the original construction-time call to UpdateCommitActionTexts).
        UpdateStatus();
        UpdateBranchLabel();
        UpdateCommitActionTexts();
        UpdatePullPushButtons();
        UpdateDeleteButtonText();

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

    private void BuildLoadingOverlay()
    {
        _loadingTitle = new Label
        {
            Name      = "loadingTitle",
            Text      = _t["loadingTitle"],
            AutoSize  = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(10, 10, 340, 20),
            Font      = new Font(Font, FontStyle.Bold)
        };

        _progressBar = new ProgressBar
        {
            Name    = "progressBar",
            Bounds  = new Rectangle(10, 38, 340, 20),
            Minimum = 0,
            Maximum = 100,
            Value   = 0,
            Style   = ProgressBarStyle.Continuous
        };

        _stepsList = new ListBox
        {
            Name           = "stepsList",
            // Tall enough to show all 8 progress steps without a vertical scrollbar.
            Bounds         = new Rectangle(10, 62, 340, 140),
            SelectionMode  = SelectionMode.None,
            BorderStyle    = BorderStyle.Fixed3D,
            IntegralHeight = false,
            TabStop        = false
        };

        _btnCancelRefresh = new Button
        {
            Name   = "btnCancelRefresh",
            Text   = _t["abortOperation"],
            Bounds = new Rectangle(110, 212, 140, 26)
        };
        _btnCancelRefresh.Click += (_, _) =>
        {
            _btnCancelRefresh.Enabled = false;
            _btnCancelRefresh.Text    = _t["aborting"];
            _refreshCts?.Cancel();
        };

        _loadingOverlay = new Panel
        {
            Name        = "loadingOverlay",
            Size        = new Size(360, 248),
            BackColor   = SystemColors.Window,
            BorderStyle = BorderStyle.FixedSingle,
            Visible     = false
        };
        _loadingOverlay.Controls.AddRange([_loadingTitle, _progressBar, _stepsList, _btnCancelRefresh]);
    }

    // ── GitFlow enforcement ───────────────────────────────────────────────────

    private void BtnGitFlow_Click(object? sender, EventArgs e)
    {
        _gitFlowUserToggled = true; // manual choice overrides auto-organization
        _gitFlowForced = !_gitFlowForced;
        RefreshTree();
    }

    private void UpdateGitFlowWarning()
    {
        var violations = GetGitFlowViolations();

        // Auto-organize: when the real hierarchy violates GitFlow, switch to the GitFlow
        // view automatically. Skipped once the user has explicitly chosen a view.
        if (!_gitFlowUserToggled && violations.Count > 0)
            _gitFlowForced = true;

        if (violations.Count == 0 && !_gitFlowForced)
        {
            _warnPanel.Visible = false;
            return;
        }

        _warnPanel.Visible = true;
        if (_gitFlowForced)
        {
            _warnLabel.Text     = violations.Count > 0
                ? _t.F("gitFlowOutOrganizing", violations.Count)
                : _t["gitFlowForcedView"];
            _warnLabel.ForeColor = Color.DarkBlue;
            _btnGitFlow.Text    = _t["restoreRealHierarchy"];
        }
        else
        {
            string msg = violations.Count == 1
                ? violations[0]
                : _t.F("gitFlowViolationsCount", violations.Count);
            _warnLabel.Text     = _t.F("warnPrefix", msg);
            _warnLabel.ForeColor = Color.DarkRed;
            _btnGitFlow.Text    = _t["organizeAsGitFlow"];
        }

        UpdateGitFlowInitButton();
    }

    private List<string> GetGitFlowViolations()
    {
        var violations = new List<string>();

        // ── Local ────────────────────────────────────────────────────────────
        string? master  = _localBranches.FirstOrDefault(b => b.FullName is "master" or "main")?.FullName;
        string? develop = _localBranches.FirstOrDefault(b => b.FullName == "develop")?.FullName;

        if (master != null && _localParentMap.TryGetValue(master, out var mp) && mp != null)
            violations.Add(_t.F("violLocalMasterRoot", master, mp));

        if (develop != null)
        {
            _localParentMap.TryGetValue(develop, out var dp);
            if (dp != master)
                violations.Add(_t.F("violLocalDevelopChild", master ?? _t["labelMasterMain"], dp ?? _t["labelRoot"]));
        }

        foreach (var b in _localBranches)
        {
            if (!b.FullName.StartsWith("feature/")) continue;
            _localParentMap.TryGetValue(b.FullName, out var fp);
            if (fp != develop)
                violations.Add(_t.F("violLocalFeature", b.FullName));
        }

        // ── Remotes (por grupo) ───────────────────────────────────────────────
        foreach (var grp in _remoteBranches.GroupBy(b => b.RemoteName ?? "origin"))
        {
            string r        = grp.Key;
            var    branches = grp.ToList();
            string? rmaster  = branches.FirstOrDefault(b => b.DisplayName is "master" or "main")?.FullName;
            string? rdevelop = branches.FirstOrDefault(b => b.DisplayName == "develop")?.FullName;

            if (rmaster != null && _remoteParentMap.TryGetValue(rmaster, out var rmp) && rmp != null)
                violations.Add(_t.F("violRemoteMasterRoot", r, rmp));

            if (rdevelop != null)
            {
                _remoteParentMap.TryGetValue(rdevelop, out var rdp);
                if (rdp != rmaster)
                    violations.Add(_t.F("violRemoteDevelopChild", r, r, master ?? _t["labelMasterMain"], rdp ?? _t["labelRoot"]));
            }

            foreach (var b in branches)
            {
                if (!b.DisplayName.StartsWith("feature/")) continue;
                _remoteParentMap.TryGetValue(b.FullName, out var rfp);
                if (rfp != rdevelop)
                    violations.Add(_t.F("violRemoteFeature", b.FullName, r));
            }
        }

        return violations;
    }

    private static Dictionary<string, string?> BuildGitFlowRemoteParentMap(List<BranchInfo> branches)
    {
        var result = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var grp in branches.GroupBy(b => b.RemoteName ?? "origin"))
        {
            var groupList = grp.ToList();
            string? master  = groupList.FirstOrDefault(b => b.DisplayName is "master" or "main")?.FullName;
            string? develop = groupList.FirstOrDefault(b => b.DisplayName == "develop")?.FullName;

            foreach (var b in groupList)
            {
                string? parent;
                if      (b.FullName == master)                   parent = null;
                else if (b.FullName == develop)                  parent = master;
                else if (b.DisplayName.StartsWith("feature/"))   parent = develop;
                else if (b.DisplayName.StartsWith("release/"))   parent = develop;
                else if (b.DisplayName.StartsWith("hotfix/"))    parent = master;
                else                                             parent = null;
                result[b.FullName] = parent;
            }
        }
        return result;
    }

    private static Dictionary<string, string?> BuildGitFlowParentMap(List<BranchInfo> branches)
    {
        string? master  = branches.FirstOrDefault(b => b.FullName is "master" or "main")?.FullName;
        string? develop = branches.FirstOrDefault(b => b.FullName == "develop")?.FullName;

        var result = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var b in branches)
        {
            string name = b.FullName;
            string? parent;
            if      (name == master)                 parent = null;
            else if (name == develop)                parent = master;
            else if (name.StartsWith("feature/"))   parent = develop;
            else if (name.StartsWith("release/"))   parent = develop;
            else if (name.StartsWith("hotfix/"))    parent = master;
            else                                     parent = null;
            result[name] = parent;
        }
        return result;
    }

    // ── Repository combo ──────────────────────────────────────────────────────

    private void LoadRepositories()
    {
        _cboRepo.Items.Clear();
        var repos = BranchHierarchyService.GetRepositoriesFromSettings();

        if (!string.IsNullOrEmpty(_svc.WorkingDir) &&
            !repos.Contains(_svc.WorkingDir, StringComparer.OrdinalIgnoreCase))
        {
            repos.Insert(0, _svc.WorkingDir);
        }

        foreach (var r in repos) _cboRepo.Items.Add(r);

        _cboRepo.SelectedItem = _svc.WorkingDir.Length > 0 ? _svc.WorkingDir : _cboRepo.Items[0];
    }

    private void CboRepo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cboRepo.SelectedItem is string dir && dir != _svc.WorkingDir)
        {
            _svc.WorkingDir = dir;
            _gitFlowUserToggled = false; // re-enable auto-organization for the new repo
            RefreshTree();
        }
    }

    // ── Tree building ─────────────────────────────────────────────────────────

    private void RebuildAllSections(string filter, Dictionary<string, string?> localMap, Dictionary<string, string?> remoteMap)
    {
        BuildLocalSection(filter, localMap);
        BuildRemotesSection(filter, remoteMap);
        BuildTagsSection(filter);
        // ApplyCheckBoxVisibility() runs AFTER EndUpdate (the native TVM_SETITEM hide does not
        // reliably stick while BeginUpdate suppresses the redraw).
    }

    private void BuildLocalSection(string filter, Dictionary<string, string?> localMap)
    {
        var list = Filter(_localBranches, filter);
        _localRoot.Text = _t.F("sectionLocal", list.Count);
        _localRoot.Nodes.Clear();

        if (list.Count == 0)
        { _localRoot.Nodes.Add(EmptyNode(_t["emptyLocal"])); return; }

        foreach (var n in BuildAncestryTree(list, localMap, b => b.FullName))
            _localRoot.Nodes.Add(n);
    }

    private void BuildRemotesSection(string filter, Dictionary<string, string?> remoteMap)
    {
        var list = Filter(_remoteBranches, filter);
        _remotesRoot.Text = _t.F("sectionRemotes", list.Count);
        _remotesRoot.Nodes.Clear();

        if (list.Count == 0)
        { _remotesRoot.Nodes.Add(EmptyNode(_t["emptyRemote"])); return; }

        foreach (var n in BuildAncestryTree(list, remoteMap, b => b.FullName))
            _remotesRoot.Nodes.Add(n);
    }

    private void BuildTagsSection(string filter)
    {
        var list = Filter(_tags, filter);
        _tagsRoot.Text = _t.F("sectionTags", list.Count);
        _tagsRoot.Nodes.Clear();

        if (list.Count == 0)
        { _tagsRoot.Nodes.Add(EmptyNode(_t["emptyTags"])); return; }

        var noChildren = new Dictionary<string, List<BranchInfo>>(StringComparer.Ordinal);
        foreach (var n in PathGroup(list, noChildren, t => t.FullName))
            _tagsRoot.Nodes.Add(n);
    }

    // ── Combined ancestry + path tree builder ─────────────────────────────────

    /// <summary>
    /// Builds the section tree combining two relationships:
    /// <list type="bullet">
    /// <item>vertical nesting by git ancestry (<paramref name="parentMap"/>): a branch is nested
    /// under its parent branch when that parent is also displayed;</item>
    /// <item>horizontal grouping by '/' in the name: among the children of a given parent, names
    /// that share a path prefix are grouped under folder nodes
    /// (e.g. <c>feature/teste</c> → folder "feature" containing leaf "teste").</item>
    /// </list>
    /// <paramref name="getPath"/> returns the name used for '/' splitting and leaf labels
    /// (full name for locals, remote-stripped DisplayName for remotes).
    /// </summary>
    private List<TreeNode> BuildAncestryTree(
        List<BranchInfo> branches,
        Dictionary<string, string?> parentMap,
        Func<BranchInfo, string> getPath)
    {
        var present    = new HashSet<string>(branches.Select(b => b.FullName), StringComparer.Ordinal);
        var childrenOf = new Dictionary<string, List<BranchInfo>>(StringComparer.Ordinal);
        var roots      = new List<BranchInfo>();

        foreach (var b in branches)
        {
            string? parent = parentMap.TryGetValue(b.FullName, out var p) ? p : null;
            if (parent != null && present.Contains(parent))
            {
                if (!childrenOf.TryGetValue(parent, out var lst)) { lst = []; childrenOf[parent] = lst; }
                lst.Add(b);
            }
            else
            {
                roots.Add(b);
            }
        }

        return PathGroup(roots, childrenOf, getPath);
    }

    /// <summary>
    /// Groups a set of sibling branches by '/' path segments into folder nodes, then nests each
    /// branch's ancestry children (from <paramref name="childrenOf"/>) recursively under its leaf.
    /// <paramref name="stripPrefix"/> is removed from each branch's path before splitting, so an
    /// ancestry child in the same folder as its parent (e.g. <c>feature/teste1</c> under
    /// <c>feature/f3</c>) nests as a bare leaf instead of re-creating a redundant folder.
    /// </summary>
    private List<TreeNode> PathGroup(
        List<BranchInfo> siblings,
        Dictionary<string, List<BranchInfo>> childrenOf,
        Func<BranchInfo, string> getPath,
        string stripPrefix = "")
    {
        var root = new SortedDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var b in siblings.OrderBy(getPath, StringComparer.OrdinalIgnoreCase))
        {
            string path = getPath(b);
            if (stripPrefix.Length > 0 && path.StartsWith(stripPrefix, StringComparison.Ordinal))
                path = path[stripPrefix.Length..];
            var parts  = path.Split('/');
            var cursor = root;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (!cursor.TryGetValue(parts[i], out var child) ||
                    child is not SortedDictionary<string, object> childDict)
                {
                    childDict = new SortedDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    cursor[parts[i]] = childDict;
                }
                cursor = (SortedDictionary<string, object>)cursor[parts[i]];
            }
            cursor[parts[^1]] = b; // leaf
        }
        return WalkPathDict(root, childrenOf, getPath);
    }

    private List<TreeNode> WalkPathDict(
        SortedDictionary<string, object> dict,
        Dictionary<string, List<BranchInfo>> childrenOf,
        Func<BranchInfo, string> getPath)
    {
        var nodes = new List<TreeNode>();
        foreach (var kvp in dict)
        {
            if (kvp.Value is BranchInfo b)
            {
                var node = CreateLeafNode(b, kvp.Key);
                if (childrenOf.TryGetValue(b.FullName, out var kids))
                    // Nest children under this branch, stripping its folder prefix so a same-folder
                    // child (feature/teste1 under feature/f3) shows as a bare leaf, not feature/teste1.
                    foreach (var n in PathGroup(kids, childrenOf, getPath, DirPrefix(getPath(b))))
                        node.Nodes.Add(n);
                nodes.Add(node);
            }
            else if (kvp.Value is SortedDictionary<string, object> sub)
            {
                int fi = GetFolderIconIndex(kvp.Key);
                var folder = new TreeNode(kvp.Key)
                {
                    Tag                = SectionTag.Folder,
                    ImageIndex         = fi,
                    SelectedImageIndex = fi
                };
                foreach (var n in WalkPathDict(sub, childrenOf, getPath))
                    folder.Nodes.Add(n);
                nodes.Add(folder);
            }
        }
        return nodes;
    }

    // Returns the folder portion of a branch path (up to and including the last '/'),
    // or "" when the branch sits at the root (no '/').
    private static string DirPrefix(string path)
    {
        int i = path.LastIndexOf('/');
        return i >= 0 ? path[..(i + 1)] : string.Empty;
    }

    // ── Node factories ────────────────────────────────────────────────────────

    /// <summary>Creates a leaf branch/tag node showing the last path segment as its label.</summary>
    private TreeNode CreateLeafNode(BranchInfo info, string label)
    {
        // Tracking indicators — shown only when there is actual divergence:
        //   ↑N = commits ahead (to push)   ↓M = commits behind (to pull)
        //   Both omitted when the branch is in sync with its upstream.
        string tracking = string.Empty;
        if (info.Type == BranchType.Local && info.HasUpstream &&
            (info.AheadCount > 0 || info.BehindCount > 0))
        {
            var sb = new System.Text.StringBuilder(" (");
            if (info.BehindCount > 0) sb.Append($"↓{info.BehindCount}");
            if (info.AheadCount  > 0) sb.Append($"↑{info.AheadCount}");
            sb.Append(')');
            tracking = sb.ToString();
        }

        string displayLabel = info.IsCurrent ? $"[{label}]" : label;
        string text         = displayLabel + tracking;
        if (info.IsCurrent) text += "  "; // win32 measures with tree font (regular); extra room for bold rendering

        int imgIdx = GetBranchIconIndex(info);

        return new TreeNode(text)
        {
            Tag                = info,
            NodeFont           = info.IsCurrent ? new Font(_tree.Font, FontStyle.Bold) : null,
            ForeColor          = info.IsCurrent ? SystemColors.Highlight : _tree.ForeColor,
            ImageIndex         = imgIdx,
            SelectedImageIndex = imgIdx
        };
    }

    /// <summary>
    /// Selects the <see cref="NodeIcons"/> index for a branch or tag leaf node based on
    /// the branch name conventions (master, develop, feature/*, etc.).
    /// </summary>
    private static int GetBranchIconIndex(BranchInfo info)
    {
        // For remotes, compare against the display name (strips the remote prefix).
        string name = (info.Type == BranchType.Remote
            ? info.DisplayName : info.FullName).ToLowerInvariant();

        if (name is "master" or "main")         return NodeIcons.BranchMaster;
        if (name is "develop" or "development") return NodeIcons.BranchDevelop;
        if (name.StartsWith("feature/"))        return NodeIcons.BranchFeatureLeaf;
        if (name.StartsWith("bugfix/")  ||
            name.StartsWith("bug/"))            return NodeIcons.BranchBugfix;
        if (name.StartsWith("release/"))        return NodeIcons.BranchRelease;
        if (name.StartsWith("hotfix/"))         return NodeIcons.BranchHotfix;
        if (name.StartsWith("support/"))        return NodeIcons.BranchSupport;

        return info.Type switch
        {
            BranchType.Remote => NodeIcons.RemoteBranch,
            BranchType.Tag    => NodeIcons.Tag,
            _                 => NodeIcons.Branch,
        };
    }

    /// <summary>
    /// Selects the <see cref="NodeIcons"/> index for a path-segment folder node based on
    /// the folder name (e.g. "feature" → leaf icon, "hotfix" → warning icon).
    /// </summary>
    private static int GetFolderIconIndex(string folderName)
    {
        return folderName.ToLowerInvariant() switch
        {
            // Remote-group folder (the remote name, e.g. "origin") keeps the rocket icon
            // (origin.png). The dedicated RemoteGroup node that used to carry NodeIcons.Remote
            // was replaced by generic path folders, so restore the icon by name here.
            "origin"   or "upstream"             => NodeIcons.Remote,
            "feature"  or "features"             => NodeIcons.BranchFeature,
            "bugfix"   or "bug"   or "bugs"       => NodeIcons.BranchBugfix,
            "release"  or "releases"             => NodeIcons.BranchRelease,
            "hotfix"   or "hotfixes"             => NodeIcons.BranchHotfix,
            "support"                            => NodeIcons.BranchSupport,
            _                                    => NodeIcons.Folder,
        };
    }

    private static TreeNode EmptyNode(string text) =>
        new(text) { Tag = SectionTag.Empty, ForeColor = Color.Gray };

    // ── Filter ────────────────────────────────────────────────────────────────

    private void ApplyFilter(string filter)
    {
        _tree.BeginUpdate();
        try
        {
            var localMap  = _gitFlowForced ? _svc.OverlayBasedOn(BuildGitFlowParentMap(_localBranches)) : _localParentMap;
            var remoteMap = _gitFlowForced ? BuildGitFlowRemoteParentMap(_remoteBranches)               : _remoteParentMap;
            RebuildAllSections(filter, localMap, remoteMap);
            ExpandRoots();
        }
        finally { _tree.EndUpdate(); }
        ApplyCheckBoxVisibility();   // hide checkboxes on section/folder nodes (after EndUpdate so it sticks)
        UpdateDeleteButtonText();    // rebuilt tree → no checks → reset the Excluir label
        ScrollTreeToTop();           // filtering is a normal reload: keep the scrollbar at the top
    }

    private static List<BranchInfo> Filter(List<BranchInfo> source, string filter) =>
        string.IsNullOrEmpty(filter)
            ? source
            : source.Where(b => b.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void ExpandRoots()
    {
        // While filtering, always expand everything so results are visible.
        string filter = _txtFilter?.Text.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(filter))
        {
            _localRoot.ExpandAll();
            _remotesRoot.ExpandAll();
            _tagsRoot.ExpandAll();
            return;
        }
        _treeStateByRepo.TryGetValue(_svc.WorkingDir, out var saved);
        RestoreTreeState(saved);
    }

    private void RestoreTreeState(HashSet<string>? expandedPaths)
    {
        if (expandedPaths is null || expandedPaths.Count == 0)
        {
            // Default first-time behaviour
            _localRoot.ExpandAll();
            _remotesRoot.Expand();
            _tagsRoot.Expand();
            return;
        }
        _restoringState = true;
        try
        {
            _tree.CollapseAll();
            RestoreNodeExpansion(_tree.Nodes, expandedPaths);
        }
        finally { _restoringState = false; }
    }

    private void RestoreNodeExpansion(TreeNodeCollection nodes, HashSet<string> paths)
    {
        foreach (TreeNode node in nodes)
        {
            string? path = GetNodeStablePath(node);
            if (path != null && paths.Contains(path))
            {
                node.Expand();
                RestoreNodeExpansion(node.Nodes, paths);
            }
        }
    }

    /// <summary>
    /// Computes a stable string key for a tree node that survives tree rebuilds.
    /// Uses the section tag for root nodes, the remote name for remote-group nodes,
    /// the folder text for folder nodes, and BranchInfo.FullName for leaf nodes.
    /// Returns null for nodes that should not be tracked (empty placeholders).
    /// </summary>
    private static string? GetNodeStablePath(TreeNode node)
    {
        var parts = new List<string>();
        TreeNode? cur = node;
        while (cur != null)
        {
            string? seg;
            if (cur.Tag is BranchInfo bi)
            {
                seg = bi.FullName;
            }
            else if (cur.Tag is string s)
            {
                seg = s switch
                {
                    SectionTag.Local       => "LOCAL",
                    SectionTag.Remotes     => "REMOTES",
                    SectionTag.Tags        => "TAGS",
                    SectionTag.RemoteGroup => cur.Text,
                    SectionTag.Folder      => cur.Text,
                    _                      => null   // Empty or unknown
                };
            }
            else
            {
                return null;
            }
            if (seg is null) return null;
            parts.Add(seg);
            cur = cur.Parent;
        }
        parts.Reverse();
        return string.Join("|", parts);
    }

    private void ScheduleSaveDebounce()
    {
        if (_saveDebounce is null)
        {
            _saveDebounce = new System.Windows.Forms.Timer { Interval = 500 };
            _saveDebounce.Tick += (_, _) => { _saveDebounce.Stop(); SaveTreeState(); };
        }
        _saveDebounce.Stop();
        _saveDebounce.Start();
    }

    private static Dictionary<string, HashSet<string>> LoadTreeState()
    {
        try
        {
            if (!File.Exists(StateFilePath)) return [];
            string json = File.ReadAllText(StateFilePath);
            var raw = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
            if (raw is null) return [];
            return raw.ToDictionary(
                kv => kv.Key,
                kv => new HashSet<string>(kv.Value, StringComparer.Ordinal),
                StringComparer.OrdinalIgnoreCase);
        }
        catch { return []; }
    }

    private void SaveTreeState()
    {
        try
        {
            var raw = _treeStateByRepo.ToDictionary(kv => kv.Key, kv => kv.Value.ToList());
            string dir = Path.GetDirectoryName(StateFilePath)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(StateFilePath, JsonSerializer.Serialize(raw));
        }
        catch { }
    }

    /// <summary>
    /// Recursively collapses <paramref name="node"/> and all of its descendants
    /// (depth-first so child state is set before parent collapse).
    /// </summary>
    private static void CollapseRecursive(TreeNode node)
    {
        foreach (TreeNode child in node.Nodes)
            CollapseRecursive(child);
        node.Collapse();
    }

    /// <summary>
    /// Sets TabIndex on every interactive control: top→bottom visually,
    /// right→left within each row.
    /// </summary>
    private void SetTabOrder()
    {
        // Panels on the form — visual order top→bottom
        _topPanel           .TabIndex = 0;
        _gitFlowInitPanel   .TabIndex = 1;
        _filterPanel        .TabIndex = 2;
        _warnPanel          .TabIndex = 3;
        _gitFlowButtonPanel .TabIndex = 4;
        _tree               .TabIndex = 5;
        _bottomPanel        .TabIndex = 6;

        // Top panel (only interactive control)
        _cboRepo.TabIndex = 0;

        // Filter panel — left→right
        _txtFilter .TabIndex = 0;
        _btnRefresh.TabIndex = 1;

        // Warn panel — right→left
        _btnGitFlow.TabIndex = 0;

        // GitFlow init panel
        _btnGitFlowInit.TabIndex = 0;

        // GitFlow button panel — left→right
        _btnPull            .TabIndex = 0;
        _btnPush            .TabIndex = 1;
        _btnCommitDedicated .TabIndex = 2;
        _btnExcluir         .TabIndex = 3;
        _btnGitFlowDedicated.TabIndex = 4;
        _btnRestore         .TabIndex = 5;

        // Bottom panel
        _btnClose        .TabIndex = 0;
        _chkShowDebug    .TabIndex = 1;
        _chkDeveloperMode.TabIndex = 2;
    }

    /// <summary>Enables or disables all interactive controls while the loading overlay is active.</summary>
    private void SetFormEnabled(bool enabled)
    {
        _cboRepo            .Enabled = enabled;
        _txtFilter          .Enabled = enabled;
        _btnRefresh         .Enabled = enabled;
        _btnGitFlow         .Enabled = enabled;
        _btnGitFlowInit     .Enabled = enabled && !IsGitFlowConfigured();
        _btnCommitDedicated .Enabled = enabled;
        _btnExcluir         .Enabled = enabled;
        _btnGitFlowDedicated.Enabled = enabled;
        _btnRestore          .Enabled = enabled;
        _tree               .Enabled = enabled;
        _btnClose           .Enabled = enabled;
        _chkShowDebug       .Enabled = enabled;
        _chkDeveloperMode   .Enabled = enabled;
    }

    private void UpdateStatus()
        => _statusLbl.Text =
            _t.F("statusCounts", _localBranches.Count, _remoteBranches.Count, _tags.Count);

    private void UpdateBranchLabel()
    {
        string text = _t.F("branchLabel", _svc.GetCurrentBranch());
        // When the remote is ahead of the checked-out branch, append the pull count (↓N) so the
        // user sees there is something to pull straight from the branch label. Hidden when in sync.
        int behind = _localBranches.FirstOrDefault(b => b.IsCurrent)?.BehindCount ?? 0;
        if (behind > 0) text += $"  ↓{behind}";
        _lblBranch.Text = text;
    }

    private void UpdateCommitActionTexts()
        => UpdateCommitActionTexts(_svc.GetPendingChangesCount());

    private void UpdateCommitActionTexts(int pendingChangesCount)
    {
        int pending = Math.Max(0, pendingChangesCount);
        _miCommit.Text = _t.F("commitCount", pending);
        _btnCommitDedicated.Text = _miCommit.Text;
    }

    private void UpdatePullPushButtons()
    {
        var current = _localBranches.FirstOrDefault(b => b.IsCurrent);
        if (current == null) return;

        int behind = current.BehindCount;
        int ahead  = current.AheadCount;
        _btnPull.Text = _t.F("pullCount", behind);
        _btnPush.Text = _t.F("pushCount", ahead);
        _miPull.Text  = _t.F("ctxPull", behind);
        _miPush.Text  = _t.F("ctxPush", ahead);
    }

    /// <summary>
    /// After the window is shown, contacts the remote (off the UI thread) to refresh the current
    /// branch's ahead/behind counts, then updates the Pull/Push controls and branch label. Best-effort
    /// and non-blocking: the window already shows last-known counts; this corrects them once the network
    /// round-trip completes. Kept off the synchronous open path, which must stay fast and offline-safe.
    /// </summary>
    private async Task RefreshRemoteStatusAsync()
    {
        bool fetched = await Task.Run(() => _svc.FetchCurrentBranchUpstream());
        if (!fetched || IsDisposed) return;

        var tracking = await Task.Run(() => _svc.GetBranchTrackingInfo());
        if (IsDisposed) return;

        foreach (var b in _localBranches)
            if (tracking.TryGetValue(b.FullName, out var ti))
            {
                b.HasUpstream = ti.hasUpstream;
                b.AheadCount  = ti.ahead;
                b.BehindCount = ti.behind;
            }

        UpdatePullPushButtons();
        UpdateBranchLabel();
    }

    private BranchInfo? SelectedBranch()
        => _tree.SelectedNode?.Tag as BranchInfo;

    /// <summary>Scrolls the tree so the first node is at the top (vertical scrollbar at the start).</summary>
    private void ScrollTreeToTop()
    {
        if (_tree.Nodes.Count > 0)
            _tree.TopNode = _tree.Nodes[0];
    }

    /// <summary>All checked branch/tag leaf nodes across the whole tree (multi-selection set).</summary>
    private List<TreeNode> CheckedBranchNodes()
    {
        var result = new List<TreeNode>();
        void Walk(TreeNodeCollection nodes)
        {
            foreach (TreeNode n in nodes)
            {
                if (n.Checked && n.Tag is BranchInfo) result.Add(n);
                Walk(n.Nodes);
            }
        }
        Walk(_tree.Nodes);
        return result;
    }

    // ── Per-node checkbox visibility ──────────────────────────────────────────
    // TreeView.CheckBoxes is all-or-nothing, so checkboxes are hidden on every node that is
    // NOT a branch/tag leaf (section roots, path folders, placeholders) via the native
    // TVM_SETITEM message, which clears the node's state-image index (0 = no checkbox).

    [StructLayout(LayoutKind.Sequential)]
    private struct TVITEM
    {
        public int    mask;
        public IntPtr hItem;
        public int    state;
        public int    stateMask;
        public IntPtr pszText;
        public int    cchTextMax;
        public int    iImage;
        public int    iSelectedImage;
        public int    cChildren;
        public IntPtr lParam;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref TVITEM lParam);

    private const int TVM_SETITEM         = 0x1100 + 63;   // TVM_SETITEMW
    private const int TVIF_STATE          = 0x0008;
    private const int TVIS_STATEIMAGEMASK = 0xF000;

    private void HideCheckBox(TreeNode node)
    {
        var tvi = new TVITEM
        {
            hItem     = node.Handle,
            mask      = TVIF_STATE,
            stateMask = TVIS_STATEIMAGEMASK,
            state     = 0,
        };
        SendMessage(_tree.Handle, TVM_SETITEM, IntPtr.Zero, ref tvi);
    }

    /// <summary>
    /// TreeView that swallows a left double-click landing on the checkbox glyph. WinForms otherwise
    /// desyncs the checkbox visual from <see cref="TreeNode.Checked"/> when the checkbox is
    /// double-clicked — the second click toggles it without raising <c>BeforeCheck</c>, so a protected
    /// branch could *appear* checked even though the single-click gate (<c>Tree_BeforeCheck</c>)
    /// blocked it. Eating the double-click leaves the gated single-click path as the only way to
    /// toggle a checkbox; double-clicking the label still checks out (it never reaches here).
    /// </summary>
    private sealed class GatedTreeView : TreeView
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct TVHITTESTINFO
        {
            public int    pt_x;
            public int    pt_y;
            public int    flags;
            public IntPtr hItem;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref TVHITTESTINFO lParam);

        private const int WM_LBUTTONDBLCLK     = 0x0203;
        private const int TVM_HITTEST          = 0x1100 + 17;
        private const int TVHT_ONITEMSTATEICON = 0x0008;   // hit landed on the checkbox/state image

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDBLCLK && IsOnCheckBox(m.LParam))
            {
                m.Result = IntPtr.Zero;   // eat it: no checkbox toggle, no visual/Checked desync
                return;
            }
            base.WndProc(ref m);
        }

        private bool IsOnCheckBox(IntPtr lParam)
        {
            var hit = new TVHITTESTINFO
            {
                pt_x = unchecked((short)(long)lParam),          // client x = low word of lParam
                pt_y = unchecked((short)((long)lParam >> 16)),  // client y = high word of lParam
            };
            SendMessage(Handle, TVM_HITTEST, IntPtr.Zero, ref hit);
            return (hit.flags & TVHT_ONITEMSTATEICON) != 0;
        }
    }

    /// <summary>Hides the checkbox on every non-leaf node; branch/tag leaves keep theirs.</summary>
    private void ApplyCheckBoxVisibility()
    {
        if (!_tree.IsHandleCreated) return;
        void Walk(TreeNodeCollection nodes)
        {
            foreach (TreeNode n in nodes)
            {
                if (n.Tag is not BranchInfo) HideCheckBox(n);
                Walk(n.Nodes);
            }
        }
        Walk(_tree.Nodes);
    }

    // ── Tree drawing (bold + highlight for current branch) ────────────────────

    private void Tree_DrawNode(object? sender, DrawTreeNodeEventArgs e)
    {
        if (e.Node is null) return;

        bool selected = (e.State & TreeNodeStates.Selected) != 0;
        bool current  = e.Node.Tag is BranchInfo bi && bi.IsCurrent;

        Font   font  = current ? new Font(_tree.Font, FontStyle.Bold) : _tree.Font;
        Color  fore  = selected ? SystemColors.HighlightText : (current ? SystemColors.Highlight : _tree.ForeColor);
        Color  back  = selected ? SystemColors.Highlight : _tree.BackColor;

        using var bg = new SolidBrush(back);
        e.Graphics.FillRectangle(bg, e.Bounds);
        TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, fore,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.SingleLine);

        if (current) font.Dispose();
    }

    // ── Tree events ───────────────────────────────────────────────────────────

    private void Tree_AfterExpand(object? sender, TreeViewEventArgs e)
    {
        if (_restoringState || e.Node is null) return;
        string? path = GetNodeStablePath(e.Node);
        if (path is null) return;
        if (!_treeStateByRepo.TryGetValue(_svc.WorkingDir, out var set))
        { set = []; _treeStateByRepo[_svc.WorkingDir] = set; }
        set.Add(path);
        ScheduleSaveDebounce();
    }

    private void Tree_AfterCollapse(object? sender, TreeViewEventArgs e)
    {
        if (_restoringState || e.Node is null) return;
        string? path = GetNodeStablePath(e.Node);
        if (path is null) return;
        if (_treeStateByRepo.TryGetValue(_svc.WorkingDir, out var set))
            set.Remove(path);
        ScheduleSaveDebounce();
    }

    private void Tree_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
    {
        // Fires after Tree_BeforeExpandCollapse has consumed the guard for folder nodes; clearing it
        // here also covers leaf branches (no toggle fires) so the flag never leaks to a later toggle.
        _suppressDoubleClickToggle = false;
        if (e.Node?.Tag is BranchInfo) DoCheckout();
    }

    /// <summary>
    /// Gates the tree checkboxes. Section roots and path folders are never checkable. The protected
    /// branches (main/master/develop, local and remote) cannot be CHECKED for deletion unless
    /// "Modo Developer" is on — unchecking is always allowed.
    /// </summary>
    private void Tree_BeforeCheck(object? sender, TreeViewCancelEventArgs e)
    {
        if (e.Node?.Tag is not BranchInfo info) { e.Cancel = true; return; }
        // e.Node.Checked is the state BEFORE the toggle: false → about to be checked.
        if (!e.Node.Checked && !_chkDeveloperMode.Checked && IsProtectedBranch(info))
            e.Cancel = true;
    }

    /// <summary>main / master / develop (local or remote) — protected from deletion by default.</summary>
    private static bool IsProtectedBranch(BranchInfo info)
    {
        string name = (info.Type == BranchType.Remote ? info.DisplayName : info.FullName).ToLowerInvariant();
        return name is "main" or "master" or "develop";
    }

    /// <summary>Unchecks any checked protected branch (used when Developer mode is turned off).</summary>
    private void UncheckProtectedBranches()
    {
        void Walk(TreeNodeCollection nodes)
        {
            foreach (TreeNode n in nodes)
            {
                if (n.Checked && n.Tag is BranchInfo info && IsProtectedBranch(info))
                    n.Checked = false;   // fires AfterCheck → UpdateDeleteButtonText
                Walk(n.Nodes);
            }
        }
        Walk(_tree.Nodes);
    }

    private void Tree_AfterCheck(object? sender, TreeViewEventArgs e) => UpdateDeleteButtonText();

    /// <summary>Updates the Excluir button label with the count of checked branch/tag leaves.</summary>
    private void UpdateDeleteButtonText()
    {
        int n = CheckedBranchNodes().Count;
        _btnExcluir.Text = n >= 1 ? _t.F("deleteCount", n) : _t["delete"];
    }

    private void Tree_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && _tree.SelectedNode?.Tag is BranchInfo) DoCheckout();
    }

    private void Tree_MouseDown(object? sender, MouseEventArgs e)
    {
        // A left double-click does checkout (see Tree_NodeMouseDoubleClick) and must NOT toggle
        // expand/collapse. This MouseDown fires before the toggle, so we raise the guard here and
        // cancel the pending expand/collapse in Tree_BeforeExpandCollapse.
        if (e.Button == MouseButtons.Left && e.Clicks == 2)
            _suppressDoubleClickToggle = true;

        if (e.Button == MouseButtons.Right)
        {
            var node = _tree.GetNodeAt(e.X, e.Y);
            if (node != null) _tree.SelectedNode = node;
        }
    }

    /// <summary>
    /// Suppresses the default expand/collapse toggle that a left double-click would otherwise
    /// trigger. The guard is set in <see cref="Tree_MouseDown"/> and cleared in
    /// <see cref="Tree_NodeMouseDoubleClick"/>, so single-click toggles and the +/- glyph keep working.
    /// </summary>
    private void Tree_BeforeExpandCollapse(object? sender, TreeViewCancelEventArgs e)
    {
        if (_suppressDoubleClickToggle) e.Cancel = true;
    }

    private void CtxMenu_Opening(object? sender, CancelEventArgs e)
    {
        // Multi-selection (2+ checked branch/tag leaves): only bulk Excluir + Atualizar apply.
        var checkedNodes = CheckedBranchNodes();
        if (checkedNodes.Count >= 2)
        {
            foreach (ToolStripItem it in _ctxMenu.Items)
                it.Visible = it == _miDelete || it == _miRefresh;
            _miDelete.Text = _t.F("ctxDeleteCount", checkedNodes.Count);
            return;   // no separators shown — just the two commands
        }

        _miDelete.Text = _t["ctxDelete"];

        var info     = SelectedBranch();
        bool branch  = info != null;
        bool local   = info?.Type == BranchType.Local;
        bool remote  = info?.Type == BranchType.Remote;
        bool tag     = info?.Type == BranchType.Tag;

        int miPending = _svc.GetPendingChangesCount();
        UpdateCommitActionTexts(miPending);
        UpdatePullPushButtons();   // refresh the ↓N / ↑N counts on the Pull/Push items from cache

        _miPull        .Visible = true;
        _miPush        .Visible = true;
        _miCommit      .Visible = true;
        _miCheckout    .Visible = branch;
        _miNewBranch   .Visible = local || tag;
        _miMerge       .Visible = local;
        _miRebase      .Visible = local;
        _miRename      .Visible = local;
        _miDelete      .Visible = local || remote || tag;
        _miExpand      .Visible = true;
        _miCollapse    .Visible = true;
        _miRefresh     .Visible = true;

        string currentBranch = _svc.GetCurrentBranch();
        _miVoltarVersao.Visible = !string.IsNullOrEmpty(currentBranch)
                               && !string.Equals(currentBranch, "develop", StringComparison.OrdinalIgnoreCase);

        FixContextMenuSeparators();
    }

    /// <summary>
    /// Hides any separator that has no visible non-separator items on one or both sides.
    /// Prevents orphan separator lines when menu groups are entirely hidden.
    /// </summary>
    private void FixContextMenuSeparators()
    {
        var items = _ctxMenu.Items;
        foreach (ToolStripItem item in items)
        {
            if (item is not ToolStripSeparator sep) continue;
            int idx = items.IndexOf(sep);
            bool beforeOk = false, afterOk = false;
            for (int i = idx - 1; i >= 0; i--)
                if (items[i] is not ToolStripSeparator && items[i].Visible) { beforeOk = true; break; }
            for (int i = idx + 1; i < items.Count; i++)
                if (items[i] is not ToolStripSeparator && items[i].Visible) { afterOk = true; break; }
            sep.Visible = beforeOk && afterOk;
        }
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    private void DoPull()
    {
        _btnPull.Enabled = false;
        _ = Task.Run(() => _svc.Pull()).ContinueWith(t =>
        {
            var (ok, err) = t.Result;
            BeginInvoke(() =>
            {
                _btnPull.Enabled = true;
                RefreshTree();
                NotifyRepoChanged();
                if (!ok && !string.IsNullOrEmpty(err))
                    ShowError("Pull falhou", err);
            });
        });
    }

    private void DoPush()
    {
        if (_openPushDialog != null)
        {
            _openPushDialog(this);
            RefreshTree();
            NotifyRepoChanged();
            return;
        }
        var (ok, err) = _svc.OpenPushWindow();
        if (!ok) ShowError("Erro ao abrir a janela de Push", err);
    }

    private void DoCommit()
    {
        // Prefer the in-process native commit dialog: it has the full plugin system loaded,
        // so Commit Template plugins (e.g. Zimerfeld: Auto-resumo) are visible.
        if (_openCommitDialog != null)
        {
            // Pass _svc.WorkingDir (the repo selected in cboRepo) so the native dialog commits against
            // that repository and its checked-out branch (shown in lblBranch), not the host's repo.
            bool? result = _openCommitDialog(this, _svc.WorkingDir);
            if (result.HasValue)
            {
                if (result.Value) { RefreshTree(); NotifyRepoChanged(); }
                else RestoreFocus();
                return;
            }
        }
        // Fallback: spawn a new GitExtensions process (plugins won't load in that mode).
        var (ok, err) = _svc.OpenCommitWindow();
        if (!ok) ShowError("Erro ao abrir a janela de Commit", err);
    }

    private void DoCheckout()
    {
        var info = SelectedBranch();
        if (info is null) return;

        (bool ok, string err) result = (false, string.Empty);

        if (info.Type == BranchType.Remote)
        {
            result = _svc.CheckoutRemoteAsLocal(info.FullName);

            if (!result.ok && result.err.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                string localName = info.FullName.Contains('/')
                    ? info.FullName[(info.FullName.IndexOf('/') + 1)..]
                    : info.FullName;

                using var dlg = new CheckoutBranchExistsDialog(localName, info.FullName, _t);
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                result = dlg.Choice switch
                {
                    CheckoutExistsChoice.ResetLocal   => _svc.Checkout(localName),
                    CheckoutExistsChoice.CreateCustom => _svc.CheckoutRemoteAsLocal(info.FullName, dlg.CustomBranchName),
                    CheckoutExistsChoice.Detached     => _svc.CheckoutDetached(info.FullName),
                    _                                 => (false, _t["invalidOption"])
                };
            }
        }
        else
        {
            result = _svc.Checkout(info.FullName);
        }

        if (result.ok)
        {
            RefreshTree();
            NotifyRepoChanged();
        }
        else if (!string.IsNullOrEmpty(result.err))
        {
            if (result.err.Contains("not fully merged", StringComparison.OrdinalIgnoreCase))
                MessageBox.Show(result.err, _t["checkoutFailedTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                MessageBox.Show(result.err, _t["checkoutFailedTitle"], MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DoNewBranch()
    {
        var info = SelectedBranch();
        if (info is null) return;

        using var dlg = new InputDialog(_t["newBranchTitle"],
            _t.F("newBranchPrompt", info.FullName), okText: _t["okBtn"], cancelText: _t["cancelBtn"]);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        var (ok, err) = _svc.CreateAndCheckoutBranch(dlg.Value.Trim(), info.FullName);
        if (ok)
        {
            RefreshTree();
            NotifyRepoChanged();
        }
        else ShowError(_t["errCreateBranchTitle"], err);
    }

    // ── GitFlow init helpers ─────────────────────────────────────────────────

    /// <summary>
    /// Returns true when all 8 standard git-flow config keys are set to their default values
    /// AND both <c>main</c> and <c>develop</c> branches exist locally.
    /// Used to decide whether to enable <see cref="_btnGitFlowInit"/>.
    /// </summary>
    private bool IsGitFlowConfigured()
    {
        (string key, string expected)[] checks =
        [
            ("gitflow.branch.main",     "main"),
            ("gitflow.branch.develop",    "develop"),
            ("gitflow.prefix.feature",    "feature/"),
            ("gitflow.prefix.bugfix",     "bugfix/"),
            ("gitflow.prefix.release",    "release/"),
            ("gitflow.prefix.hotfix",     "hotfix/"),
            ("gitflow.prefix.support",    "support/"),
            ("gitflow.prefix.versiontag", ""),
        ];

        foreach (var (key, expected) in checks)
        {
            var (output, code) = _svc.RunGitFlow($"config --get {key}");
            if (code != 0 || output.Trim() != expected)
                return false;
        }

        // Also require that both branches actually exist.
        var (_, masterCode)  = _svc.RunGitFlow("rev-parse --verify refs/heads/main");
        var (_, developCode) = _svc.RunGitFlow("rev-parse --verify refs/heads/develop");
        return masterCode == 0 && developCode == 0;
    }

    /// <summary>
    /// Refreshes the enabled state of <see cref="_btnGitFlowInit"/>:
    /// enabled when the working directory does not follow the GitFlow pattern, disabled when it does.
    /// </summary>
    private void UpdateGitFlowInitButton()
    {
        _btnGitFlowInit.Enabled = !IsGitFlowConfigured();
    }

    // ── Tooltip debug ────────────────────────────────────────────────────────

    private void ApplyControlTooltips(bool show)
    {
        _mainTooltip.RemoveAll();
        if (!show) return;
        SetTooltipsRecursive(this, _mainTooltip);
        // Also show the window's own TYPE and Handle (HWND) — visible on hover over any uncovered area.
        _mainTooltip.SetToolTip(this, $"TYPE: {GetType().Name}\nHandle: 0x{Handle.ToInt64():X}");
    }

    /// <summary>
    /// Repositions all buttons in <see cref="_gitFlowButtonPanel"/> left-to-right with a 4 px gap.
    /// Called explicitly on load and whenever button visibility changes so that horizontal
    /// resize of the window does NOT trigger automatic repositioning.
    /// </summary>
    private void LayoutGitFlowButtons()
    {
        // Give all six buttons an equal width that fills the panel edge-to-edge with uniform gaps,
        // so the row stays evenly spaced regardless of the window width.
        Button[] buttons = [_btnPull, _btnPush, _btnCommitDedicated, _btnExcluir, _btnGitFlowDedicated, _btnRestore];
        const int margin = 8, gap = 6;
        int avail = _gitFlowButtonPanel.Width - margin * 2 - gap * (buttons.Length - 1);
        int w = avail / buttons.Length;
        int y = (_gitFlowButtonPanel.Height - 24) / 2;

        int x = margin;
        foreach (var b in buttons)
        {
            b.Width    = w;
            b.Location = new Point(x, y);
            x += w + gap;
        }
        // Absorb integer-division rounding into the last button so the row ends flush at the right edge.
        // Use a 4 px right margin (not `margin`) so btnRestore's right edge lines up with
        // btnRefresh and btnGitFlow, both of which sit 4 px from the panel's right edge.
        const int rightMargin = 4;
        _btnRestore.Width = _gitFlowButtonPanel.Width - rightMargin - _btnRestore.Left;
    }

    private static void SetTooltipsRecursive(Control parent, ToolTip tip)
    {
        foreach (Control c in parent.Controls)
        {
            tip.SetToolTip(c, $"TYPE: {c.GetType().Name}\nID: {c.Name}");
            SetTooltipsRecursive(c, tip);
        }
    }

    private void DoGitFlowInit()
    {
        const string masterBranch  = "main";
        const string developBranch = "develop";

        // ── 1. Apply all standard git-flow config keys ────────────────────────
        (string key, string value)[] configs =
        [
            ("gitflow.branch.main",     masterBranch),
            ("gitflow.branch.develop",    developBranch),
            ("gitflow.prefix.feature",    "feature/"),
            ("gitflow.prefix.bugfix",     "bugfix/"),
            ("gitflow.prefix.release",    "release/"),
            ("gitflow.prefix.hotfix",     "hotfix/"),
            ("gitflow.prefix.support",    "support/"),
            ("gitflow.prefix.versiontag", ""),
        ];

        var errors = new System.Text.StringBuilder();
        foreach (var (key, value) in configs)
        {
            var (output, code) = _svc.RunGitFlow($"config {key} \"{value}\"");
            if (code != 0)
                errors.AppendLine($"  {key}: {output.Trim()}");
        }

        // ── 2. Create missing branches ────────────────────────────────────────
        // Detect whether the repository has any commits at all.
        // An empty repo has an unborn HEAD; git branch <name> would fail because there
        // is no valid object to point the new ref at.
        var (_, headCode) = _svc.RunGitFlow("rev-parse HEAD");
        if (headCode != 0)
        {
            // Empty repo: redirect the unborn HEAD to main and create an empty initial
            // commit so that branches can be created normally afterwards.
            _svc.RunGitFlow($"symbolic-ref HEAD refs/heads/{masterBranch}");
            var (commitOut, commitCode) = _svc.RunGitFlow(
                "commit --allow-empty -m \"chore: Initial commit\"");
            if (commitCode != 0)
            {
                errors.AppendLine($"  Commit inicial em '{masterBranch}': {commitOut.Trim()}");
            }
            else
            {
                // main now exists; create develop from it
                var (out3, code3) = _svc.RunGitFlow($"branch {developBranch}");
                if (code3 != 0)
                    errors.AppendLine($"  branch {developBranch}: {out3.Trim()}");
            }
        }
        else
        {
            // main — create from HEAD if absent
            var (_, masterExists) = _svc.RunGitFlow($"rev-parse --verify refs/heads/{masterBranch}");
            if (masterExists != 0)
            {
                var (out2, code2) = _svc.RunGitFlow($"branch {masterBranch}");
                if (code2 != 0)
                    errors.AppendLine($"  branch {masterBranch}: {out2.Trim()}");
            }

            // develop — create from main if absent
            var (_, developExists) = _svc.RunGitFlow($"rev-parse --verify refs/heads/{developBranch}");
            if (developExists != 0)
            {
                var (out3, code3) = _svc.RunGitFlow($"branch {developBranch} {masterBranch}");
                if (code3 != 0)
                    errors.AppendLine($"  branch {developBranch}: {out3.Trim()}");
            }
        }

        // ── 3. Report result ──────────────────────────────────────────────────
        if (errors.Length == 0)
        {
            _svc.Checkout(developBranch);
            MessageBox.Show(_t["gitFlowInitOk"], _t["gitFlowInitTitle"],
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show(_t.F("gitFlowInitErrors", errors), _t["gitFlowInitTitle"],
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        RefreshTree();
        UpdateGitFlowInitButton();
    }

    private void DoGitFlow()
    {
        using var dlg = new GitFlowForm(_svc, _chkShowDebug.Checked);
        _gitFlowForm = dlg;   // tracked so a Change-Working-Directory switch can force-close it

        // Refresh the tree live when GitFlow mutates the repo (any button) while still modal, and
        // reveal/select the affected branch. RefreshTree() runs behind the modal dialog and does
        // not steal its focus; the reveal runs as a post-refresh action once the tree is rebuilt.
        dlg.RepoMutated += branch =>
        {
            if (!string.IsNullOrEmpty(branch))
                _postRefreshAction = () => FocusBranchNode(branch);
            RefreshTree();
        };

        // Place the two windows side by side, both centered on the current screen.
        var wa     = Screen.FromControl(this).WorkingArea;
        int gap    = 8;
        int totalW = Width + gap + dlg.Width;

        if (wa.Width >= totalW)
        {
            int leftX = wa.Left + (wa.Width  - totalW) / 2;
            int topY  = wa.Top  + Math.Max(0, (wa.Height - Math.Max(Height, dlg.Height)) / 2);
            Location     = new Point(leftX, topY);
            dlg.Location = new Point(leftX + Width + gap, topY);
        }
        else
        {
            // Screen too narrow — centre GitFlow over this window instead
            dlg.Location = new Point(
                Math.Max(wa.Left, Location.X + (Width  - dlg.Width)  / 2),
                Math.Max(wa.Top,  Location.Y + (Height - dlg.Height) / 2));
        }

        dlg.ShowDialog(this);
        _gitFlowForm = null;   // dialog closed — no longer force-closable

        // Recentre this window on the screen after the GitFlow dialog closes.
        Location = new Point(
            wa.Left + (wa.Width  - Width)  / 2,
            wa.Top  + (wa.Height - Height) / 2);

        if (dlg.LastFinishedReleaseTag is string tag)
            _postRefreshAction = () => FocusTagNode(tag);

        // Always refresh (showing the loading overlay) when the GitFlow dialog closes, so the tree
        // reflects any repo state the dialog left behind; a freshly finished release tag is focused.
        RefreshTree();
        // No NotifyRepoChanged on close: the refresh already keeps the tree current, and notifying
        // GitExtensions would only pull its (minimized) window forward.
    }

    private void DoRestore()
    {
        using var dlg = new RestoreForm(_svc, _chkShowDebug.Checked);
        _restoreForm = dlg;   // tracked so a Change-Working-Directory switch can force-close it

        dlg.RepoMutated += branch =>
        {
            if (!string.IsNullOrEmpty(branch))
                _postRefreshAction = () => FocusBranchNode(branch);
            RefreshTree();
        };

        // Place the two windows side by side, both centered on the current screen.
        var wa     = Screen.FromControl(this).WorkingArea;
        int gap    = 8;
        int totalW = Width + gap + dlg.Width;

        if (wa.Width >= totalW)
        {
            int leftX = wa.Left + (wa.Width  - totalW) / 2;
            int topY  = wa.Top  + Math.Max(0, (wa.Height - Math.Max(Height, dlg.Height)) / 2);
            Location     = new Point(leftX, topY);
            dlg.Location = new Point(leftX + Width + gap, topY);
        }
        else
        {
            dlg.Location = new Point(
                Math.Max(wa.Left, Location.X + (Width  - dlg.Width)  / 2),
                Math.Max(wa.Top,  Location.Y + (Height - dlg.Height) / 2));
        }

        dlg.ShowDialog(this);
        _restoreForm = null;   // dialog closed — no longer force-closable

        // Recentre this window on the screen after the Restore dialog closes.
        Location = new Point(
            wa.Left + (wa.Width  - Width)  / 2,
            wa.Top  + (wa.Height - Height) / 2);

        // Always refresh (showing the loading overlay) when the Restore dialog closes, so the tree
        // reflects any repo state the dialog left behind. GitExtensions is not notified here.
        RefreshTree();
    }

    private void FocusTagNode(string tagName)
    {
        _tagsRoot.Expand();
        var node = FindTagNode(_tagsRoot.Nodes, tagName);
        if (node is null) return;
        _tree.SelectedNode = node;
        node.EnsureVisible();
        _tree.Focus();
    }

    private static TreeNode? FindTagNode(TreeNodeCollection nodes, string tagName)
    {
        foreach (TreeNode node in nodes)
        {
            if (node.Tag is BranchInfo bi && bi.FullName == tagName) return node;
            var found = FindTagNode(node.Nodes, tagName);
            if (found != null) return found;
        }
        return null;
    }

    /// <summary>
    /// Expands the LOCAL section and ancestor folders to reveal and select the local branch with
    /// the given full name (e.g. "feature/x", "develop"). No-op when the branch is not found.
    /// </summary>
    private void FocusBranchNode(string fullName)
    {
        _localRoot.Expand();
        var node = FindBranchNode(_localRoot.Nodes, fullName);
        if (node is null) return;
        _tree.SelectedNode = node;
        node.EnsureVisible();   // expands the ancestor path folders automatically
    }

    private static TreeNode? FindBranchNode(TreeNodeCollection nodes, string fullName)
    {
        foreach (TreeNode node in nodes)
        {
            if (node.Tag is BranchInfo bi && bi.Type == BranchType.Local && bi.FullName == fullName)
                return node;
            var found = FindBranchNode(node.Nodes, fullName);
            if (found != null) return found;
        }
        return null;
    }

    private void DoMerge()
    {
        var info = SelectedBranch();
        if (info?.Type != BranchType.Local) return;

        if (Confirm(_t.F("confirmMerge", info.FullName, _svc.GetCurrentBranch()), _t["confirmMergeTitle"]))
        {
            var (ok, err) = _svc.MergeBranch(info.FullName);
            if (ok) RefreshTree();
            else ShowError(_t["errMergeTitle"], err);
        }
        RestoreFocus();
    }

    private void DoRebase()
    {
        var info = SelectedBranch();
        if (info?.Type != BranchType.Local) return;

        if (Confirm(_t.F("confirmRebase", info.FullName, _svc.GetCurrentBranch()),
                _t["confirmRebaseTitle"]))
        {
            var (ok, err) = _svc.RebaseBranch(info.FullName);
            if (ok) RefreshTree();
            else ShowError(_t["errRebaseTitle"], err);
        }
        RestoreFocus();
    }

    private void DoRename()
    {
        var info = SelectedBranch();
        if (info?.Type != BranchType.Local) return;

        using var dlg = new InputDialog(_t["renameTitle"],
            _t.F("renamePrompt", info.FullName), info.FullName, _t["okBtn"], _t["cancelBtn"]);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        var (ok, err) = _svc.RenameBranch(info.FullName, dlg.Value.Trim());
        if (ok) RefreshTree();
        else ShowError(_t["errRenameTitle"], err);
        RestoreFocus();
    }

    private void DoDelete()
    {
        // Target the checked branch/tag leaves when any are checked; otherwise the selected node.
        var checkedNodes = CheckedBranchNodes();
        var targets = checkedNodes.Count > 0
            ? checkedNodes.Select(n => (BranchInfo)n.Tag!).ToList()
            : SelectedBranch() is { } sel ? new List<BranchInfo> { sel } : new List<BranchInfo>();

        // Protect main/master/develop unless Developer mode is on. Checkboxes already block marking
        // them, so this guards the single-delete path (a protected branch selected, none checked).
        if (!_chkDeveloperMode.Checked && targets.Any(IsProtectedBranch))
        {
            targets = targets.Where(t => !IsProtectedBranch(t)).ToList();
            if (targets.Count == 0)
            {
                MessageBox.Show(
                    _t["protectedBranchMsg"],
                    _t["protectedBranchTitle"], MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        if (targets.Count == 0) return;

        // Confirmation dialog listing the items, with an unchecked "Excluir Remotamente ?" option.
        var (confirmed, deleteRemote) = ConfirmDelete(targets);
        if (!confirmed) return;

        _ = DoDeleteAsync(targets, deleteRemote);
    }

    /// <summary>
    /// Modal confirmation for deletion. Asks "Deseja realmente excluir os itens selecionados ?",
    /// lists every target branch/tag, and offers an unchecked "Excluir Remotamente ?" checkbox.
    /// Returns whether the user confirmed and whether remote deletion was requested.
    /// </summary>
    private (bool confirmed, bool deleteRemote) ConfirmDelete(List<BranchInfo> targets)
    {
        using var dlg = new Form
        {
            Text            = _t["confirmDeleteTitle"],
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition   = FormStartPosition.CenterParent,
            MinimizeBox     = false,
            MaximizeBox     = false,
            ShowInTaskbar   = false,
            ClientSize      = new Size(440, 340),
            Font            = Font
        };

        var lblPrompt = new Label
        {
            Text      = _t["confirmDeletePrompt"],
            AutoSize  = false,
            Bounds    = new Rectangle(12, 12, 416, 20),
            Font      = new Font(Font, FontStyle.Bold)
        };

        var list = new ListBox
        {
            Bounds         = new Rectangle(12, 40, 416, 200),
            SelectionMode  = SelectionMode.None,
            IntegralHeight = false,
            TabStop        = false
        };
        foreach (var t in targets)
            list.Items.Add(_t.F("deleteListItem", t.FullName, DescribeType(t)));

        var chkRemote = new CheckBox
        {
            Text     = _t["deleteRemoteQuestion"],
            Checked  = false,
            AutoSize = true,
            Location = new Point(12, 250)
        };

        var btnOk = new Button
        {
            Text         = _t["deleteConfirmBtn"],
            DialogResult = DialogResult.OK,
            Bounds       = new Rectangle(252, 298, 84, 30)
        };
        var btnCancel = new Button
        {
            Text         = _t["cancelBtn"],
            DialogResult = DialogResult.Cancel,
            Bounds       = new Rectangle(344, 298, 84, 30)
        };

        dlg.Controls.AddRange([lblPrompt, list, chkRemote, btnOk, btnCancel]);
        dlg.AcceptButton = btnOk;
        dlg.CancelButton = btnCancel;

        bool ok = dlg.ShowDialog(this) == DialogResult.OK;
        return (ok, ok && chkRemote.Checked);
    }

    /// <summary>Human-readable kind of a target, used in confirmation and progress messages.</summary>
    private string DescribeType(BranchInfo info) => info.Type switch
    {
        BranchType.Tag    => _t["typeTag"],
        BranchType.Remote => _t["typeRemote"],
        _                 => _t["typeLocal"],
    };

    /// <summary>
    /// Deletes every target with the loading overlay blocking the form. The steps list shows each
    /// branch/tag name as it is removed and the progress bar advances by one slice per deletion
    /// (increment = 100 / number of deletions). When <paramref name="deleteRemote"/> is true, each
    /// local branch / tag is also removed from the default remote. The tree is reloaded within the
    /// same overlay once all deletions complete.
    /// </summary>
    private async Task DoDeleteAsync(List<BranchInfo> targets, bool deleteRemote)
    {
        if (_isRefreshing) return;
        _isRefreshing = true;

        _refreshCts?.Cancel();
        _refreshCts = new CancellationTokenSource();
        var token = _refreshCts.Token;

        // Block the form and show the overlay with the list of items being deleted. The Abortar
        // button stays enabled so the user can stop the pass and revert what was already deleted.
        _progressBar.Value = 0;
        _stepsList.Items.Clear();
        _stepsList.Items.Add($"• {_t["startingDelete"]}");
        _btnCancelRefresh.Enabled = true;
        _btnCancelRefresh.Text    = _t["abortOperation"];
        _loadingTitle.Text        = _t["deletingTitle"];
        _loadingOverlay.Location  = new Point(
            (ClientSize.Width  - _loadingOverlay.Width)  / 2,
            (ClientSize.Height - _loadingOverlay.Height) / 2);
        _loadingOverlay.Visible = true;
        _loadingOverlay.BringToFront();
        SetFormEnabled(false);

        var errors  = new List<string>();
        int total   = targets.Count;
        bool aborted = false;

        // Items deleted so far, with the SHA captured before deletion — enough to recreate them.
        var deleted = new List<(BranchInfo info, string sha, bool remoteDeleted)>();

        IProgress<(int pct, string msg)> prog = new Progress<(int pct, string msg)>(p =>
        {
            _progressBar.Value = Math.Max(0, Math.Min(100, p.pct));
            _stepsList.Items.Add($"• {p.msg}");
            int last = _stepsList.Items.Count - 1;
            if (last >= 0) _stepsList.TopIndex = last;
        });

        try
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < total; i++)
                {
                    if (token.IsCancellationRequested) { aborted = true; break; }

                    var info = targets[i];
                    prog.Report((i * 100 / total, _t.F("deletingItem", DescribeType(info), info.FullName)));

                    string sha = CaptureSha(info);                       // before deletion, to allow undo
                    var (ok, err, remoteDeleted) = DeleteSingle(info, deleteRemote);
                    if (ok) deleted.Add((info, sha, remoteDeleted));
                    else    errors.Add($"{info.FullName}: {err.Trim()}");

                    prog.Report(((i + 1) * 100 / total,
                        ok ? _t.F("deletedItem", info.FullName) : _t.F("failedItem", info.FullName)));
                }
            });

            // Aborted mid-pass: revert everything already deleted, restoring local refs and (when the
            // remote copy was removed) pushing them back to the remote.
            if (aborted)
            {
                _btnCancelRefresh.Enabled = false;
                _loadingTitle.Text        = _t["revertingTitle"];
                _progressBar.Value        = 0;
                _stepsList.Items.Add($"• {_t["abortReverting"]}");

                if (deleted.Count > 0)
                {
                    var toRevert = deleted.ToList();
                    await Task.Run(() =>
                    {
                        for (int i = 0; i < toRevert.Count; i++)
                        {
                            var d = toRevert[i];
                            prog.Report((i * 100 / toRevert.Count, _t.F("restoringItem", d.info.FullName)));
                            var (rok, rerr) = RestoreSingle(d.info, d.sha, d.remoteDeleted);
                            if (!rok) errors.Add(_t.F("restoreErrorEntry", d.info.FullName, rerr.Trim()));
                            prog.Report(((i + 1) * 100 / toRevert.Count,
                                rok ? _t.F("restoredItem", d.info.FullName) : _t.F("failedRestoreItem", d.info.FullName)));
                        }
                    });
                }
            }
            else
            {
                _btnCancelRefresh.Enabled = false;
            }

            // Reload the tree within the same overlay so the result is reflected immediately. Use a
            // fresh (non-cancelled) token — the original token may have been cancelled by the abort.
            prog.Report((100, _t["updatingTree"]));
            var data = await Task.Run(() => FetchRepoData(null, CancellationToken.None), CancellationToken.None);
            if (!IsDisposed)
            {
                ApplyRepoData(data);
                ScrollTreeToTop();
            }
            await Task.Delay(700);
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
                MessageBox.Show(_t.F("errDuringDelete", ex.Message),
                    _t["appTitle"], MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (!IsDisposed)
            {
                _loadingOverlay.Visible = false;
                _loadingTitle.Text      = _t["loadingTitle"];   // restore default title
                SetFormEnabled(true);
            }
            _isRefreshing = false;
        }

        if (!IsDisposed && errors.Count > 0)
            ShowError(aborted ? _t["abortedWarningsTitle"] : _t["errDeleteTitle"],
                      string.Join("\n", errors));
        RestoreFocus();
    }

    /// <summary>Captures the commit SHA of a target before deletion, so the deletion can be undone.</summary>
    private string CaptureSha(BranchInfo info) => info.Type switch
    {
        BranchType.Tag    => _svc.ResolveTagSha(info.FullName),
        BranchType.Remote => _svc.ResolveRemoteBranchSha(info.FullName),
        _                 => _svc.ResolveLocalBranchSha(info.FullName),
    };

    /// <summary>
    /// Deletes one branch/tag locally and, when <paramref name="deleteRemote"/> is set, from the
    /// default remote too. Runs on a background thread; the not-fully-merged force prompt is
    /// marshalled back to the UI thread. Returns (ok, error, remoteDeleted) — <c>remoteDeleted</c>
    /// indicates the remote copy was removed and must be pushed back if the operation is reverted.
    /// </summary>
    private (bool ok, string err, bool remoteDeleted) DeleteSingle(BranchInfo info, bool deleteRemote)
    {
        switch (info.Type)
        {
            case BranchType.Tag:
            {
                var (ok, err) = _svc.DeleteLocalTag(info.FullName);
                if (!ok) return (false, err, false);
                if (deleteRemote)
                {
                    var (rok, rerr) = _svc.DeleteRemoteTag(info.FullName);
                    if (!rok) return (false, rerr, false);
                    return (true, string.Empty, true);
                }
                return (true, string.Empty, false);
            }

            case BranchType.Remote:
            {
                // A remote-tracking branch only exists on the remote — deletion is inherently remote.
                // DeleteRemoteBranch treats "remote ref does not exist" as success, so a branch already
                // gone on the remote (a stale tracking ref) isn't reported as an error. On success we
                // prune the local tracking ref so the now-orphaned entry disappears from the tree.
                var (ok, err) = _svc.DeleteRemoteBranch(info.DisplayName);
                if (ok) _svc.PruneRemotes();
                return (ok, err, ok);
            }

            default:
            {
                var (ok, err) = _svc.DeleteBranch(info.FullName, isRemote: false);
                if (!ok && err.Contains("not fully merged", StringComparison.OrdinalIgnoreCase))
                {
                    bool force = Invoke(() => Confirm(
                        _t.F("forceDeleteConfirm", info.FullName),
                        _t["forceDeleteTitle"]));
                    if (!force) return (false, _t["deleteCancelledUnmerged"], false);
                    (ok, err) = _svc.DeleteBranchForce(info.FullName);
                }
                if (!ok) return (false, err, false);

                if (deleteRemote)
                {
                    var (rok, rerr) = _svc.DeleteRemoteBranch(info.DisplayName);
                    if (!rok) return (false, rerr, false);
                    return (true, string.Empty, true);
                }
                return (true, string.Empty, false);
            }
        }
    }

    /// <summary>
    /// Undoes a single deletion captured during an aborted pass: recreates the local ref at its
    /// original SHA and, when the remote copy was removed, pushes it back to the remote.
    /// </summary>
    private (bool ok, string err) RestoreSingle(BranchInfo info, string sha, bool remoteDeleted)
    {
        if (string.IsNullOrEmpty(sha))
            return (false, _t["shaNotCaptured"]);

        switch (info.Type)
        {
            case BranchType.Tag:
            {
                var (ok, err) = _svc.CreateTag(info.FullName, sha);
                if (!ok) return (false, err);
                if (remoteDeleted)
                {
                    var (rok, rerr) = _svc.RestoreRemoteTag(info.FullName, sha);
                    if (!rok) return (false, rerr);
                }
                return (true, string.Empty);
            }

            case BranchType.Remote:
                // Only ever existed on the remote — push it back; no local ref to recreate.
                return _svc.RestoreRemoteBranch(info.RemoteName ?? _svc.GetDefaultRemote(), info.DisplayName, sha);

            default:
            {
                var (ok, err) = _svc.CreateLocalBranch(info.FullName, sha);
                if (!ok) return (false, err);
                if (remoteDeleted)
                {
                    var (rok, rerr) = _svc.RestoreRemoteBranch(_svc.GetDefaultRemote(), info.DisplayName, sha);
                    if (!rok) return (false, rerr);
                }
                return (true, string.Empty);
            }
        }
    }

    // ── UI helpers ────────────────────────────────────────────────────────────

    /// <summary>
    /// Re-activates this window so it keeps focus after an action.
    /// The window is owner-less, so notifying GitExtensions of a repo change brings the
    /// GitExtensions main window to the foreground; this puts ZimerfeldTree back on top.
    /// BeginInvoke ensures Activate() runs after any pending window-activation messages
    /// (including the GitExtensions window that _notifyRepoChanged may raise asynchronously).
    /// (The GitFlow window is modal, so it keeps its own focus while open — unaffected.)
    /// </summary>
    private void RestoreFocus()
    {
        if (!IsDisposed && Visible)
            BeginInvoke(() => { if (!IsDisposed && Visible) Activate(); });
    }

    /// <summary>Notifies GitExtensions to refresh its UI, then restores focus to this window.</summary>
    private void NotifyRepoChanged()
    {
        // Tells GitExtensions to refresh its own main window. We no longer subscribe to
        // PostRepositoryChanged, so the resulting echo is simply not received here — the tree was
        // already refreshed live by the operation that called this method.
        _notifyRepoChanged?.Invoke();
        RestoreFocus();
    }

    private bool Confirm(string text, string caption) =>
        MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    private void ShowError(string caption, string message) =>
        MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

    private void ShowAboutTree()
    {
        MessageBox.Show(_t["aboutTreeBody"], _t["aboutTreeTitle"],
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tag sentinel values for non-branch tree nodes
    // ─────────────────────────────────────────────────────────────────────────
    private static class SectionTag
    {
        public const string Local       = "section:local";
        public const string Remotes     = "section:remotes";
        public const string Tags        = "section:tags";
        public const string RemoteGroup = "section:remote-group";
        public const string Folder      = "section:folder";
        public const string Empty       = "section:empty";
    }
}

// ── Checkout branch-exists dialog ────────────────────────────────────────────

internal enum CheckoutExistsChoice { ResetLocal, CreateCustom, Detached }

/// <summary>
/// Shown when checking out an origin branch whose local counterpart already exists.
/// Mirrors the GitExtensions "Checkout branch" dialog options.
/// </summary>
internal sealed class CheckoutBranchExistsDialog : Form
{
    private readonly RadioButton _rbReset;
    private readonly RadioButton _rbCustom;
    private readonly RadioButton _rbDetached;
    private readonly TextBox     _customName;

    public CheckoutExistsChoice Choice { get; private set; }
    public string CustomBranchName => _customName.Text.Trim();

    public CheckoutBranchExistsDialog(string localName, string remoteName, Translator t)
    {
        Text            = t["checkoutBranchTitle"];
        Size            = new Size(480, 190);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        Font            = new Font("Segoe UI", 9f);
        Icon            = PluginIcon.ForForm();

        string defaultCustom = remoteName.Replace('/', '_');

        _rbReset = new RadioButton
        {
            Text    = t["resetLocalBranch"],
            Bounds  = new Rectangle(10, 16, 236, 22),
            Checked = true
        };
        var lblLocal = new Label
        {
            Text      = $"'{localName}'",
            Bounds    = new Rectangle(248, 18, 210, 16),
            AutoSize  = false,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _rbCustom = new RadioButton
        {
            Text   = t["createCustomBranch"],
            Bounds = new Rectangle(10, 48, 250, 22)
        };
        _customName = new TextBox
        {
            Text    = defaultCustom,
            Bounds  = new Rectangle(262, 46, 196, 22),
            Enabled = false
        };

        _rbDetached = new RadioButton
        {
            Text   = t["checkoutDetached"],
            Bounds = new Rectangle(10, 80, 450, 22)
        };

        var btnCheckout = new Button
        {
            Text         = t["checkoutBtn"],
            Bounds       = new Rectangle(376, 120, 82, 28),
            DialogResult = DialogResult.OK
        };

        _rbCustom.CheckedChanged += (_, _) => _customName.Enabled = _rbCustom.Checked;

        btnCheckout.Click += (_, _) =>
        {
            Choice = _rbReset.Checked   ? CheckoutExistsChoice.ResetLocal
                   : _rbCustom.Checked  ? CheckoutExistsChoice.CreateCustom
                   : CheckoutExistsChoice.Detached;
        };

        Controls.AddRange([_rbReset, lblLocal, _rbCustom, _customName, _rbDetached, btnCheckout]);
        AcceptButton = btnCheckout;
    }
}

// ── Simple single-line input dialog ──────────────────────────────────────────

/// <summary>Minimal modal dialog that asks the user for a text value.</summary>
internal sealed class InputDialog : Form
{
    private readonly Label   _label;
    private readonly TextBox _input;

    public string Value => _input.Text;

    public InputDialog(string title, string prompt, string defaultValue = "",
        string okText = "OK", string cancelText = "Cancel")
    {
        Text            = title;
        Size            = new Size(420, 148);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        Font            = new Font("Segoe UI", 9f);
        Icon            = PluginIcon.ForForm();

        _label = new Label  { Text = prompt, Bounds = new Rectangle(12, 12, 388, 20) };
        _input = new TextBox { Text = defaultValue, Bounds = new Rectangle(12, 36, 388, 22) };

        var ok     = new Button { Text = okText,     Bounds = new Rectangle(228, 70, 82, 28), DialogResult = DialogResult.OK };
        var cancel = new Button { Text = cancelText, Bounds = new Rectangle(318, 70, 82, 28), DialogResult = DialogResult.Cancel };

        Controls.AddRange([_label, _input, ok, cancel]);
        AcceptButton = ok;
        CancelButton = cancel;
    }
}
