// BranchHierarchyForm.cs — Main WinForms window for ZimerfeldTree plugin
// MIT License — Copyright (c) 2026 Zimerfeld

using System.ComponentModel;
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
    /// Returns true = commits were made, false = dialog closed without committing, null = unavailable (fall back).
    /// </summary>
    private readonly Func<IWin32Window, bool?>? _openCommitDialog;
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
    private int                          _pendingChangesCount;        // cached from background task; used by UpdatePullPushButtons

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
    private Button           _btnVoltar           = null!;
    private Panel            _gitFlowButtonPanel  = null!;
    private Button           _btnPull             = null!;
    private Button           _btnPush             = null!;
    private Button           _btnCommitDedicated  = null!;
    private TreeView         _tree        = null!;
    private StatusStrip      _status      = null!;
    private ToolStripStatusLabel _statusLbl = null!;

    // ── Bottom panel ──────────────────────────────────────────────────────────
    private Panel    _bottomPanel  = null!;
    private Button   _btnClose    = null!;
    private CheckBox _chkShowDebug = null!;
    private LinkLabel _lnkAbout   = null!;

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

    private static void SaveShowControlIds(bool value)
    {
        try
        {
            string dir = Path.GetDirectoryName(UiSettingsPath)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(UiSettingsPath,
                $"{{\"showControlIds\":{(value ? "true" : "false")}}}");
        }
        catch { }
    }

    // ── Tree section roots ────────────────────────────────────────────────────
    private TreeNode _localRoot   = null!;
    private TreeNode _remotesRoot = null!;
    private TreeNode _tagsRoot    = null!;

    // ── Context menu ──────────────────────────────────────────────────────────
    private ContextMenuStrip   _ctxMenu     = null!;
    private ToolStripMenuItem  _miCommit    = null!;
    private ToolStripMenuItem  _miCheckout  = null!;
    private ToolStripMenuItem  _miNewBranch = null!;
    private ToolStripMenuItem  _miMerge     = null!;
    private ToolStripMenuItem  _miRebase    = null!;
    private ToolStripMenuItem  _miRename    = null!;
    private ToolStripMenuItem  _miDelete    = null!;
    private ToolStripMenuItem  _miGitFlow   = null!;
    private ToolStripMenuItem  _miExpand    = null!;
    private ToolStripMenuItem  _miCollapse  = null!;
    private ToolStripMenuItem  _miRefresh   = null!;

    // ─────────────────────────────────────────────────────────────────────────
    public BranchHierarchyForm(string workingDir, Action? notifyRepoChanged = null,
        Func<IWin32Window, bool?>? openCommitDialog = null,
        Func<IWin32Window, bool>? openPushDialog = null)
    {
        _svc = new BranchHierarchyService(workingDir);
        _notifyRepoChanged  = notifyRepoChanged;
        _openCommitDialog   = openCommitDialog;
        _openPushDialog     = openPushDialog;
        _treeStateByRepo    = LoadTreeState();
        InitializeComponent();
        LoadRepositories();
        FormClosed += (_, _) => { _saveDebounce?.Dispose(); SaveTreeState(); };
        // Initial tree load is triggered by the Shown event so the window skeleton
        // is visible to the user before we start reading the repository.
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Called by the plugin when GitExtensions switches the active repository.</summary>
    public void UpdateWorkingDir(string newDir)
    {
        _svc.WorkingDir = newDir;
        _gitFlowUserToggled = false; // re-enable auto-organization for the new repo
        if (!_cboRepo.Items.Contains(newDir))
            _cboRepo.Items.Add(newDir);
        _cboRepo.SelectedItem = newDir;
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
            _stepsList.Items.Add("• Iniciando...");
            _btnCancelRefresh.Enabled = true;
            _btnCancelRefresh.Text    = "Cancelar";
            _loadingOverlay.Location  = new Point(
                (ClientSize.Width  - _loadingOverlay.Width)  / 2,
                (ClientSize.Height - _loadingOverlay.Height) / 2);
            _loadingOverlay.Visible = true;
            _loadingOverlay.BringToFront();
            SetFormEnabled(false);
        }

        List<BranchInfo>            local  = [];
        List<BranchInfo>            remote = [];
        List<BranchInfo>            tags   = [];
        Dictionary<string, string?> lMap   = [];
        Dictionary<string, string?> rMap   = [];

        IProgress<(int pct, string msg)>? ip = showOverlay
            ? new Progress<(int pct, string msg)>(p =>
              {
                  _progressBar.Value = p.pct;
                  _stepsList.Items.Add($"• {p.msg}");
                  int last = _stepsList.Items.Count - 1;
                  if (last >= 0) _stepsList.TopIndex = last;
              })
            : null;

        try
        {
            await Task.Run(() =>
            {
                ip?.Report((10, "Carregando branches locais..."));
                local  = _svc.GetLocalBranches();
                token.ThrowIfCancellationRequested();
                ip?.Report((30, "Carregando branches remotas..."));
                remote = _svc.GetRemoteBranches();
                token.ThrowIfCancellationRequested();
                ip?.Report((50, "Carregando tags..."));
                tags   = _svc.GetTags();
                token.ThrowIfCancellationRequested();
                ip?.Report((65, "Calculando hierarquia local..."));
                lMap   = _svc.BuildParentMap(local);
                token.ThrowIfCancellationRequested();
                ip?.Report((80, "Calculando hierarquia remota..."));
                rMap   = _svc.BuildRemoteParentMap(remote);
                token.ThrowIfCancellationRequested();
                ip?.Report((92, "Obtendo informações de sincronização..."));
                var tracking = _svc.GetBranchTrackingInfo();
                foreach (var b in local)
                    if (tracking.TryGetValue(b.FullName, out var ti))
                    {
                        b.HasUpstream  = ti.hasUpstream;
                        b.AheadCount   = ti.ahead;
                        b.BehindCount  = ti.behind;
                    }
                _pendingChangesCount = _svc.GetPendingChangesCount();
                ip?.Report((100, "Concluído."));
            }, token);
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
                MessageBox.Show($"Erro ao carregar dados do repositório:\n{ex.Message}",
                    "ZimerfeldTree", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return;
        }

        if (IsDisposed) { _isRefreshing = false; return; }

        _localBranches   = local;
        _remoteBranches  = remote;
        _tags            = tags;
        _localParentMap  = lMap;
        _remoteParentMap = rMap;

        _tree.BeginUpdate();
        try
        {
            UpdateGitFlowWarning();
            var localMap  = _gitFlowForced ? BuildGitFlowParentMap(_localBranches)         : _localParentMap;
            var remoteMap = _gitFlowForced ? BuildGitFlowRemoteParentMap(_remoteBranches)   : _remoteParentMap;
            RebuildAllSections(_txtFilter?.Text.Trim() ?? string.Empty, localMap, remoteMap);
            ExpandRoots();
            UpdateStatus();
            UpdateBranchLabel();
            UpdatePullPushButtons();
        }
        finally { _tree.EndUpdate(); }

        var postAction = _postRefreshAction;
        _postRefreshAction = null;
        postAction?.Invoke();

        if (showOverlay)
        {
            // Let the user see the final "Concluído." step for a moment before the overlay closes.
            await Task.Delay(1000);
            _loadingOverlay.Visible = false;
            SetFormEnabled(true);
        }
        _isRefreshing = false;
    }

    // ── Initialization ────────────────────────────────────────────────────────

    private void InitializeComponent()
    {
        SuspendLayout();

        Text            = "ZimerfeldTree - Branch Hierarchy";
        Size            = new Size(580, 760);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox     = true;
        MinimizeBox     = true;
        KeyPreview      = true;
        Font            = new Font("Segoe UI", 9f);
        Icon            = TreeOfLifeIcon.ForForm();

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

        // Layout order (Dock fills from bottom and top inward, Fill takes the remainder).
        // Added last = topmost for DockStyle.Top; visual order top→bottom:
        //   _topPanel, _gitFlowInitPanel, _filterPanel, _warnPanel, _gitFlowButtonPanel, _tree (Fill), _bottomPanel, _status
        Controls.Add(_tree);                // Fill
        Controls.Add(_gitFlowButtonPanel);  // Top — just above the tree
        Controls.Add(_warnPanel);           // Top
        Controls.Add(_filterPanel);         // Top
        Controls.Add(_gitFlowInitPanel);    // Top — GitFlow Initialize button (above filter)
        Controls.Add(_topPanel);            // Top (topmost)
        Controls.Add(_status);         // Bottom
        Controls.Add(_bottomPanel);    // Bottom (above status)
        Controls.Add(_lnkAbout);       // Floats top-right over _topPanel
        Controls.Add(_loadingOverlay); // Floats above everything (BringToFront when shown)

        CancelButton = _btnClose;

        // Restore debug state and button enable state.
        Load += (_, _) =>
        {
            _lnkAbout.BringToFront();
            ApplyControlTooltips(_chkShowDebug.Checked);
            UpdateGitFlowInitButton();
            LayoutGitFlowButtons();
        };

        // Trigger the async initial load once the window is fully painted.
        Shown += (_, _) => _ = RefreshTreeAsync(showOverlay: true);

        ResumeLayout(false);
        PerformLayout();
    }

    private void BuildTopPanel()
    {
        _topPanel = new Panel { Name = "topPanel", Dock = DockStyle.Top, Height = 87 };

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
            Text     = "Working Directory:",
            AutoSize = true,
            Font     = new Font(Font, FontStyle.Bold),
            Margin   = new Padding(0, 0, 0, 2)
        };

        _cboRepo = new ComboBox
        {
            Name          = "cboRepo",
            DropDownStyle = ComboBoxStyle.DropDownList,
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
        _lnkAbout = new LinkLabel
        {
            Name     = "lnkAbout",
            Text     = "About Tree",
            AutoSize = true,
            Anchor   = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(ClientSize.Width - 100, 2)
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
            PlaceholderText = "Filtrar branches..."
        };
        _txtFilter.TextChanged += (_, _) => ApplyFilter(_txtFilter.Text.Trim());

        _btnRefresh = new Button
        {
            Name   = "btnRefresh",
            Text   = "↺",
            Dock   = DockStyle.Right,
            Width  = 32,
            Height = 24,
            Font   = new Font(Font, FontStyle.Bold)
        };
        _btnRefresh.Click += (_, _) => RefreshTree();

        _filterPanel.Controls.Add(_txtFilter);
        _filterPanel.Controls.Add(_btnRefresh);
    }

    private void BuildWarnPanel()
    {
        _warnLabel = new Label
        {
            Name      = "warnLabel",
            Dock      = DockStyle.Fill,
            Text      = string.Empty,
            ForeColor = Color.DarkRed,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(4, 0, 0, 0),
            AutoSize  = false
        };

        _btnGitFlow = new Button
        {
            Name  = "btnGitFlow",
            Dock  = DockStyle.Right,
            Width = 160,
            Text  = "Organizar como GitFlow"
        };
        _btnGitFlow.Click += BtnGitFlow_Click;

        _warnPanel = new Panel
        {
            Name    = "warnPanel",
            Dock    = DockStyle.Top,
            Height  = 26,
            Visible = false,
            Padding = new Padding(4, 2, 4, 2)
        };
        _warnPanel.Controls.Add(_warnLabel);
        _warnPanel.Controls.Add(_btnGitFlow);
    }

    private void BuildGitFlowInitPanel()
    {
        _btnGitFlowInit = new Button
        {
            Name   = "btnGitFlowInit",
            Width  = 160,
            Height = 22,
            Text   = "GitFlow Initialize"
        };
        _btnGitFlowInit.Click += (_, _) => DoGitFlowInit();

        _gitFlowInitPanel = new Panel
        {
            Name    = "gitFlowInitPanel",
            Dock    = DockStyle.Top,
            Height  = 26,
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
        _btnPull = new Button { Name = "btnPull", Text = "Pull", Width = 80, Height = 24, Visible = false };
        _btnPull.Click += (_, _) => DoPull();

        _btnPush = new Button { Name = "btnPush", Text = "Push", Width = 80, Height = 24, Visible = false };
        _btnPush.Click += (_, _) => DoPush();

        _btnCommitDedicated = new Button { Name = "btnCommitDedicated", Text = "Commit", Width = 80, Height = 24, Visible = false };
        _btnCommitDedicated.Click += (_, _) => DoCommit();

        _btnGitFlowDedicated = new Button
        {
            Name    = "btnGitFlowDedicated",
            Text    = "GitFlow",
            Width   = 120,
            Height  = 24,
            Font    = new Font(Font, FontStyle.Bold),
            Visible = false
        };
        _btnGitFlowDedicated.Click += (_, _) => DoGitFlow();

        _btnVoltar = new Button
        {
            Name    = "btnVoltar",
            Text    = "Voltar Versão",
            Width   = 120,
            Height  = 24,
            Visible = false
        };
        _btnVoltar.Click += (_, _) => DoRestore();

        _gitFlowButtonPanel = new Panel { Name = "gitFlowButtonPanel", Dock = DockStyle.Top, Height = 32 };
        _gitFlowButtonPanel.Controls.AddRange([_btnPull, _btnPush, _btnCommitDedicated, _btnGitFlowDedicated, _btnVoltar]);
        // Positions are set explicitly by LayoutGitFlowButtons() — no Layout event so resize doesn't move buttons.
    }

    private void BuildTreeView()
    {
        _tree = new TreeView
        {
            Name          = "tree",
            Dock          = DockStyle.Fill,
            ShowLines     = true,
            ShowPlusMinus = true,
            ShowRootLines = true,
            HideSelection = false,
            DrawMode      = TreeViewDrawMode.OwnerDrawText,
            Font          = new Font("Segoe UI", 9f),
            ImageList     = NodeIcons.GetList()
        };

        _tree.DrawNode              += Tree_DrawNode;
        _tree.NodeMouseDoubleClick  += Tree_NodeMouseDoubleClick;
        _tree.KeyDown               += Tree_KeyDown;
        _tree.MouseDown             += Tree_MouseDown;
        _tree.AfterExpand           += Tree_AfterExpand;
        _tree.AfterCollapse         += Tree_AfterCollapse;

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

    private void BuildContextMenu()
    {
        _miCommit    = new ToolStripMenuItem("Commit");
        _miCheckout  = new ToolStripMenuItem("Checkout");
        _miNewBranch = new ToolStripMenuItem("Nova branch daqui…");
        _miMerge     = new ToolStripMenuItem("Mesclar na branch atual");
        _miRebase    = new ToolStripMenuItem("Rebase na branch atual");
        _miRename    = new ToolStripMenuItem("Renomear…");
        _miDelete    = new ToolStripMenuItem("Excluir…");
        _miGitFlow   = new ToolStripMenuItem("GitFlow…");
        _miExpand    = new ToolStripMenuItem("Expandir tudo");
        _miCollapse  = new ToolStripMenuItem("Recolher tudo");
        _miRefresh   = new ToolStripMenuItem("Atualizar");

        _miCommit   .Image = LoadMenuIcon("ctx-commit.png");
        _miCheckout .Image = LoadMenuIcon("ctx-checkout.png");
        _miNewBranch.Image = LoadMenuIcon("ctx-new-branch.png");
        _miMerge    .Image = LoadMenuIcon("ctx-merge.png");
        _miRebase   .Image = LoadMenuIcon("ctx-rebase.png");
        _miRename   .Image = LoadMenuIcon("ctx-rename.png");
        _miDelete   .Image = LoadMenuIcon("ctx-delete.png");
        _miGitFlow  .Image = LoadMenuIcon("ctx-gitflow.png");
        _miExpand   .Image = LoadMenuIcon("ctx-expand.png");
        _miCollapse .Image = LoadMenuIcon("ctx-collapse.png");
        _miRefresh  .Image = LoadMenuIcon("ctx-refresh.png");

        _miCommit   .Click += (_, _) => DoCommit();
        _miCheckout .Click += (_, _) => DoCheckout();
        _miNewBranch.Click += (_, _) => DoNewBranch();
        _miMerge    .Click += (_, _) => DoMerge();
        _miRebase   .Click += (_, _) => DoRebase();
        _miRename   .Click += (_, _) => DoRename();
        _miDelete   .Click += (_, _) => DoDelete();
        _miGitFlow  .Click += (_, _) => DoGitFlow();
        _miExpand  .Click += (_, _) => _tree.SelectedNode?.ExpandAll();
        _miCollapse.Click += (_, _) => { if (_tree.SelectedNode is { } n) CollapseRecursive(n); };
        _miRefresh  .Click += (_, _) => RefreshTree();

        _ctxMenu = new ContextMenuStrip();
        _ctxMenu.Opening += CtxMenu_Opening;
        _ctxMenu.Items.AddRange(
        [
            _miCommit,
            new ToolStripSeparator(),
            _miCheckout, _miNewBranch,
            new ToolStripSeparator(),
            _miMerge, _miRebase,
            new ToolStripSeparator(),
            _miRename, _miDelete,
            new ToolStripSeparator(),
            _miGitFlow,
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
            Text      = "Local: 0  |  Remoto: 0  |  Tags: 0",
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
            Text         = "Fechar",
            Width        = 80,
            Height       = 26,
            DialogResult = DialogResult.Cancel
        };
        _btnClose.Click += (_, _) => Close();

        _chkShowDebug = new CheckBox
        {
            Name     = "chkShowDebug",
            Text     = "Show Debug",
            AutoSize = true,
            Checked  = LoadShowControlIds()
        };
        _chkShowDebug.CheckedChanged += (_, _) =>
        {
            SaveShowControlIds(_chkShowDebug.Checked);
            ApplyControlTooltips(_chkShowDebug.Checked);
        };

        _bottomPanel = new Panel { Name = "bottomPanel", Dock = DockStyle.Bottom, Height = 36 };
        _bottomPanel.Controls.Add(_btnClose);
        _bottomPanel.Controls.Add(_chkShowDebug);

        // Centre Fechar; pin chkShowDebug to the left.
        _bottomPanel.Layout += (_, _) =>
        {
            int cy = (_bottomPanel.Height - _btnClose.Height) / 2;
            _btnClose.Location = new Point(
                (_bottomPanel.Width - _btnClose.Width) / 2, cy);
            _chkShowDebug.Location = new Point(
                8, (_bottomPanel.Height - _chkShowDebug.Height) / 2);
        };
    }

    private void BuildLoadingOverlay()
    {
        _loadingTitle = new Label
        {
            Name      = "loadingTitle",
            Text      = "Carregando dados do repositório",
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
            Text   = "Cancelar",
            Bounds = new Rectangle(130, 212, 100, 26)
        };
        _btnCancelRefresh.Click += (_, _) =>
        {
            _btnCancelRefresh.Enabled = false;
            _btnCancelRefresh.Text    = "Cancelando…";
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
                ? $"Hierarquia fora do GitFlow ({violations.Count}) — exibindo organização GitFlow."
                : "Exibindo hierarquia GitFlow forçada.";
            _warnLabel.ForeColor = Color.DarkBlue;
            _btnGitFlow.Text    = "Restaurar hierarquia real";
        }
        else
        {
            string msg = violations.Count == 1
                ? violations[0]
                : $"Hierarquia fora do GitFlow ({violations.Count} violações).";
            _warnLabel.Text     = $"⚠ {msg}";
            _warnLabel.ForeColor = Color.DarkRed;
            _btnGitFlow.Text    = "Organizar como GitFlow";
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
            violations.Add($"LOCAL '{master}' deveria ser raiz, mas tem pai '{mp}'.");

        if (develop != null)
        {
            _localParentMap.TryGetValue(develop, out var dp);
            if (dp != master)
                violations.Add($"LOCAL 'develop' deveria ser filho de '{master ?? "master/main"}', está em '{dp ?? "(raiz)"}'.");
        }

        foreach (var b in _localBranches)
        {
            if (!b.FullName.StartsWith("feature/")) continue;
            _localParentMap.TryGetValue(b.FullName, out var fp);
            if (fp != develop)
                violations.Add($"LOCAL '{b.FullName}' deveria ser filho de 'develop'.");
        }

        // ── Remotes (por grupo) ───────────────────────────────────────────────
        foreach (var grp in _remoteBranches.GroupBy(b => b.RemoteName ?? "origin"))
        {
            string r        = grp.Key;
            var    branches = grp.ToList();
            string? rmaster  = branches.FirstOrDefault(b => b.DisplayName is "master" or "main")?.FullName;
            string? rdevelop = branches.FirstOrDefault(b => b.DisplayName == "develop")?.FullName;

            if (rmaster != null && _remoteParentMap.TryGetValue(rmaster, out var rmp) && rmp != null)
                violations.Add($"REMOTE '{r}/master' deveria ser raiz, mas tem pai '{rmp}'.");

            if (rdevelop != null)
            {
                _remoteParentMap.TryGetValue(rdevelop, out var rdp);
                if (rdp != rmaster)
                    violations.Add($"REMOTE '{r}/develop' deveria ser filho de '{r}/{master ?? "master/main"}', está em '{rdp ?? "(raiz)"}'.");
            }

            foreach (var b in branches)
            {
                if (!b.DisplayName.StartsWith("feature/")) continue;
                _remoteParentMap.TryGetValue(b.FullName, out var rfp);
                if (rfp != rdevelop)
                    violations.Add($"REMOTE '{b.FullName}' deveria ser filho de '{r}/develop'.");
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
    }

    private void BuildLocalSection(string filter, Dictionary<string, string?> localMap)
    {
        var list = Filter(_localBranches, filter);
        _localRoot.Text = $"LOCAL ({list.Count})";
        _localRoot.Nodes.Clear();

        if (list.Count == 0)
        { _localRoot.Nodes.Add(EmptyNode("(nenhuma branch local encontrada)")); return; }

        foreach (var n in BuildAncestryTree(list, localMap, b => b.FullName))
            _localRoot.Nodes.Add(n);
    }

    private void BuildRemotesSection(string filter, Dictionary<string, string?> remoteMap)
    {
        var list = Filter(_remoteBranches, filter);
        _remotesRoot.Text = $"REMOTES ({list.Count})";
        _remotesRoot.Nodes.Clear();

        if (list.Count == 0)
        { _remotesRoot.Nodes.Add(EmptyNode("(nenhuma branch remota encontrada)")); return; }

        foreach (var group in list.GroupBy(b => b.RemoteName ?? "origin").OrderBy(g => g.Key))
        {
            var remoteNode = new TreeNode(group.Key)
            {
                Tag                = SectionTag.RemoteGroup,
                ImageIndex         = NodeIcons.Remote,
                SelectedImageIndex = NodeIcons.Remote
            };
            var groupList  = group.ToList();

            foreach (var n in BuildAncestryTree(groupList, remoteMap, b => b.DisplayName))
                remoteNode.Nodes.Add(n);

            _remotesRoot.Nodes.Add(remoteNode);
        }
    }

    private void BuildTagsSection(string filter)
    {
        var list = Filter(_tags, filter);
        _tagsRoot.Text = $"TAGS ({list.Count})";
        _tagsRoot.Nodes.Clear();

        if (list.Count == 0)
        { _tagsRoot.Nodes.Add(EmptyNode("(nenhuma tag encontrada)")); return; }

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
    /// </summary>
    private List<TreeNode> PathGroup(
        List<BranchInfo> siblings,
        Dictionary<string, List<BranchInfo>> childrenOf,
        Func<BranchInfo, string> getPath)
    {
        var root = new SortedDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var b in siblings.OrderBy(getPath, StringComparer.OrdinalIgnoreCase))
        {
            var parts  = getPath(b).Split('/');
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
                    foreach (var n in PathGroup(kids, childrenOf, getPath))
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
            var localMap  = _gitFlowForced ? BuildGitFlowParentMap(_localBranches)          : _localParentMap;
            var remoteMap = _gitFlowForced ? BuildGitFlowRemoteParentMap(_remoteBranches) : _remoteParentMap;
            RebuildAllSections(filter, localMap, remoteMap);
            ExpandRoots();
        }
        finally { _tree.EndUpdate(); }
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
        _btnGitFlowDedicated.TabIndex = 3;

        // Bottom panel
        _btnClose    .TabIndex = 0;
        _chkShowDebug.TabIndex = 1;
    }

    /// <summary>Enables or disables all interactive controls while the loading overlay is active.</summary>
    private void SetFormEnabled(bool enabled)
    {
        _cboRepo            .Enabled = enabled;
        _txtFilter          .Enabled = enabled;
        _btnRefresh         .Enabled = enabled;
        _btnGitFlow         .Enabled = enabled;
        _btnGitFlowInit     .Enabled = enabled;
        _btnGitFlowDedicated.Enabled = enabled;
        _btnVoltar          .Enabled = enabled;
        _tree               .Enabled = enabled;
        _btnClose           .Enabled = enabled;
        _chkShowDebug       .Enabled = enabled;
    }

    private void UpdateStatus()
        => _statusLbl.Text =
            $"Local: {_localBranches.Count}  |  Remoto: {_remoteBranches.Count}  |  Tags: {_tags.Count}";

    private void UpdateBranchLabel()
        => _lblBranch.Text = $"Branch: {_svc.GetCurrentBranch()}";

    private void UpdatePullPushButtons()
    {
        var current    = _localBranches.FirstOrDefault(b => b.IsCurrent);
        bool hasBranch = current != null;
        _btnPull            .Visible = hasBranch;
        _btnPush            .Visible = hasBranch;
        _btnCommitDedicated .Visible = hasBranch;
        _btnGitFlowDedicated.Visible = hasBranch;
        _btnVoltar          .Visible = hasBranch;
        LayoutGitFlowButtons(); // reposition buttons after visibility change
        if (!hasBranch) return;

        int behind = current!.BehindCount;
        int ahead  = current.AheadCount;
        _btnPull.Text = behind > 0 ? $"Pull (↓{behind})" : "Pull";
        _btnPush.Text = ahead  > 0 ? $"Push (↑{ahead})"  : "Push";
        _btnCommitDedicated.Text = _pendingChangesCount > 0 ? $"Commit ({_pendingChangesCount})" : "Commit";
    }

    private BranchInfo? SelectedBranch()
        => _tree.SelectedNode?.Tag as BranchInfo;

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
        if (e.Node?.Tag is BranchInfo) DoCheckout();
    }

    private void Tree_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && _tree.SelectedNode?.Tag is BranchInfo) DoCheckout();
    }

    private void Tree_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            var node = _tree.GetNodeAt(e.X, e.Y);
            if (node != null) _tree.SelectedNode = node;
        }
    }

    private void CtxMenu_Opening(object? sender, CancelEventArgs e)
    {
        var info     = SelectedBranch();
        bool branch  = info != null;
        bool local   = info?.Type == BranchType.Local;
        bool remote  = info?.Type == BranchType.Remote;
        bool tag     = info?.Type == BranchType.Tag;

        int miPending = _svc.GetPendingChangesCount();
        _miCommit.Text = miPending > 0 ? $"Commit ({miPending})" : "Commit";

        _miCheckout .Visible = branch;
        _miNewBranch.Visible = local || tag;
        _miMerge    .Visible = local;
        _miRebase   .Visible = local;
        _miRename   .Visible = local;
        _miDelete   .Visible = local || remote || tag;
        _miGitFlow  .Visible = branch;

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
            bool pushed = _openPushDialog(this);
            if (pushed) { RefreshTree(); NotifyRepoChanged(); }
            else RestoreFocus();
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
            bool? result = _openCommitDialog(this);
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

                using var dlg = new CheckoutBranchExistsDialog(localName, info.FullName);
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                result = dlg.Choice switch
                {
                    CheckoutExistsChoice.ResetLocal   => _svc.Checkout(localName),
                    CheckoutExistsChoice.CreateCustom => _svc.CheckoutRemoteAsLocal(info.FullName, dlg.CustomBranchName),
                    CheckoutExistsChoice.Detached     => _svc.CheckoutDetached(info.FullName),
                    _                                 => (false, "Opção inválida.")
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
                MessageBox.Show(result.err, "Checkout falhou", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                MessageBox.Show(result.err, "Checkout falhou", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DoNewBranch()
    {
        var info = SelectedBranch();
        if (info is null) return;

        using var dlg = new InputDialog("Nova branch",
            $"Nome da nova branch a partir de '{info.FullName}':");
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        var (ok, err) = _svc.CreateAndCheckoutBranch(dlg.Value.Trim(), info.FullName);
        if (ok)
        {
            RefreshTree();
            NotifyRepoChanged();
        }
        else ShowError("Erro ao criar branch", err);
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
        int y = (_gitFlowButtonPanel.Height - 24) / 2;
        int x = 8;
        if (_btnPull.Visible)
        {
            _btnPull.Location = new Point(x, y); x += _btnPull.Width + 4;
            _btnPush.Location = new Point(x, y); x += _btnPush.Width + 4;
        }
        if (_btnCommitDedicated.Visible)
        {
            _btnCommitDedicated.Location = new Point(x, y); x += _btnCommitDedicated.Width + 4;
        }
        _btnGitFlowDedicated.Location = new Point(x, y); x += _btnGitFlowDedicated.Width + 4;
        _btnVoltar.Location = new Point(x, y);
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
            MessageBox.Show("GitFlow inicializado com sucesso.", "GitFlow Initialize",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show($"Alguns comandos falharam:\n{errors}", "GitFlow Initialize",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        RefreshTree();
        UpdateGitFlowInitButton();
    }

    private void DoGitFlow()
    {
        using var dlg = new GitFlowForm(_svc, _chkShowDebug.Checked);

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

        // Recentre this window on the screen after the GitFlow dialog closes.
        Location = new Point(
            wa.Left + (wa.Width  - Width)  / 2,
            wa.Top  + (wa.Height - Height) / 2);

        if (dlg.LastFinishedReleaseTag is string tag)
            _postRefreshAction = () => FocusTagNode(tag);

        RefreshTree();
        // GitFlow dialog has already closed (modal) — refocusing ZimerfeldTree here is correct.
        NotifyRepoChanged();
    }

    private void DoRestore()
    {
        using var dlg = new RestoreForm(_svc, _chkShowDebug.Checked);

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

        // Recentre this window on the screen after the Restore dialog closes.
        Location = new Point(
            wa.Left + (wa.Width  - Width)  / 2,
            wa.Top  + (wa.Height - Height) / 2);

        RefreshTree();
        NotifyRepoChanged();
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

        if (Confirm($"Mesclar '{info.FullName}' na branch atual '{_svc.GetCurrentBranch()}'?", "Confirmar Merge"))
        {
            var (ok, err) = _svc.MergeBranch(info.FullName);
            if (ok) RefreshTree();
            else ShowError("Erro no merge", err);
        }
        RestoreFocus();
    }

    private void DoRebase()
    {
        var info = SelectedBranch();
        if (info?.Type != BranchType.Local) return;

        if (Confirm($"Rebase em cima de '{info.FullName}' (branch atual: '{_svc.GetCurrentBranch()}')?",
                "Confirmar Rebase"))
        {
            var (ok, err) = _svc.RebaseBranch(info.FullName);
            if (ok) RefreshTree();
            else ShowError("Erro no rebase", err);
        }
        RestoreFocus();
    }

    private void DoRename()
    {
        var info = SelectedBranch();
        if (info?.Type != BranchType.Local) return;

        using var dlg = new InputDialog("Renomear branch",
            $"Novo nome para '{info.FullName}':", info.FullName);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        var (ok, err) = _svc.RenameBranch(info.FullName, dlg.Value.Trim());
        if (ok) RefreshTree();
        else ShowError("Erro ao renomear", err);
        RestoreFocus();
    }

    private void DoDelete()
    {
        var info = SelectedBranch();
        if (info is null) return;

        if (!Confirm($"Excluir '{info.FullName}'?", "Confirmar exclusão")) return;

        (bool ok, string err) result = info.Type switch
        {
            BranchType.Tag    => _svc.DeleteTag(info.FullName),
            BranchType.Remote => _svc.DeleteBranch(info.FullName, isRemote: true),
            _                 => _svc.DeleteBranch(info.FullName, isRemote: false)
        };

        if (result.ok)
        {
            RefreshTree();
        }
        else if (result.err.Contains("not fully merged", StringComparison.OrdinalIgnoreCase))
        {
            if (Confirm($"A branch não está totalmente mesclada. Forçar exclusão de '{info.FullName}'?",
                        "Excluir forçado"))
            {
                var (ok2, err2) = _svc.DeleteBranchForce(info.FullName);
                if (ok2) RefreshTree();
                else ShowError("Erro ao excluir", err2);
            }
        }
        else
        {
            ShowError("Erro ao excluir", result.err);
        }
        RestoreFocus();
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

    /// <summary>
    /// Brings this window to the front after a commit completes in the GitExtensions Commit dialog.
    /// Called by the plugin's PostCommit event handler so the tree stays visible after committing.
    /// Uses BeginInvoke to ensure activation runs after the commit dialog has fully closed.
    /// </summary>
    public void FocusAfterCommit()
    {
        if (!IsDisposed && Visible)
            BeginInvoke(() => { if (!IsDisposed && Visible) { BringToFront(); Activate(); } });
    }

    /// <summary>Notifies GitExtensions to refresh its UI, then restores focus to this window.</summary>
    private void NotifyRepoChanged()
    {
        _notifyRepoChanged?.Invoke();
        RestoreFocus();
    }

    private bool Confirm(string text, string caption) =>
        MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    private void ShowError(string caption, string message) =>
        MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

    private void ShowAboutTree()
    {
        MessageBox.Show(
            "Botões:\n\n" +
            "  ↺ (Atualizar)\n" +
            "    Recarrega a árvore de branches lendo o repositório git atual.\n\n" +
            "  Organizar como GitFlow / Restaurar hierarquia geral\n" +
            "    Reorganiza a árvore seguindo a hierarquia GitFlow\n" +
            "    (main, develop, feature/*, release/*, hotfix/*, support/*).\n" +
            "    Ou exibe conforme estado real.\n\n" +
            "  GitFlow Initialize\n" +
            "    Inicializa o GitFlow no repositório atual (git flow init).\n" +
            "    Visível apenas no modo de depuração.\n\n" +
            "  Pull / Pull (↓N)\n" +
            "    Puxa commits do remoto para a branch atual (git pull).\n" +
            "    O número entre parênteses indica commits a receber.\n\n" +
            "  Push / Push (↑N)\n" +
            "    Envia commits locais para o remoto (git push).\n" +
            "    O número entre parênteses indica commits a enviar.\n\n" +
            "  Commit / Commit (N)\n" +
            "    Abre o diálogo de commit do GitExtensions.\n" +
            "    O número entre parênteses indica arquivos com mudanças pendentes.\n\n" +
            "  GitFlow\n" +
            "    Abre a janela GitFlow para a branch atual:\n" +
            "    Publish, Track, Update e Finish.\n\n" +
            "  Voltar Versão\n" +
            "    Abre a janela de restauração: restaurar arquivo de commit,\n" +
            "    cherry-pick e reset de branch.\n\n" +
            "Menu de contexto (clique com botão direito em uma branch):\n\n" +
            "  Checkout           — Faz checkout da branch selecionada.\n" +
            "  Nova branch daqui… — Cria nova branch a partir desta.\n" +
            "  Mesclar            — git merge da branch selecionada na atual.\n" +
            "  Rebase             — git rebase da atual na selecionada.\n" +
            "  Renomear…          — Renomeia a branch.\n" +
            "  Excluir…           — Exclui a branch.\n" +
            "  GitFlow…           — Abre janela GitFlow para esta branch.",
            "About Tree", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

    public CheckoutBranchExistsDialog(string localName, string remoteName)
    {
        Text            = "Checkout branch";
        Size            = new Size(480, 190);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        Font            = new Font("Segoe UI", 9f);
        Icon            = TreeOfLifeIcon.ForForm();

        string defaultCustom = remoteName.Replace('/', '_');

        _rbReset = new RadioButton
        {
            Text    = "Reset local branch with the name:",
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
            Text   = "Create local branch with custom name:",
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
            Text   = "Checkout the commit (in detached head)",
            Bounds = new Rectangle(10, 80, 450, 22)
        };

        var btnCheckout = new Button
        {
            Text         = "Checkout",
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

    public InputDialog(string title, string prompt, string defaultValue = "")
    {
        Text            = title;
        Size            = new Size(420, 148);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        Font            = new Font("Segoe UI", 9f);
        Icon            = TreeOfLifeIcon.ForForm();

        _label = new Label  { Text = prompt, Bounds = new Rectangle(12, 12, 388, 20) };
        _input = new TextBox { Text = defaultValue, Bounds = new Rectangle(12, 36, 388, 22) };

        var ok     = new Button { Text = "OK",       Bounds = new Rectangle(228, 70, 82, 28), DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Cancelar", Bounds = new Rectangle(318, 70, 82, 28), DialogResult = DialogResult.Cancel };

        Controls.AddRange([_label, _input, ok, cancel]);
        AcceptButton = ok;
        CancelButton = cancel;
    }
}
