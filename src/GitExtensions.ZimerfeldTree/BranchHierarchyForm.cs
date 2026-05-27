// BranchHierarchyForm.cs — Main WinForms window for ZimerfeldTree plugin
// MIT License — Copyright (c) 2026 Zimerfeld

using System.ComponentModel;

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

    // ── Cached data ───────────────────────────────────────────────────────────
    private List<BranchInfo>             _localBranches  = [];
    private List<BranchInfo>             _remoteBranches = [];
    private List<BranchInfo>             _tags           = [];
    private Dictionary<string, string?>  _localParentMap  = []; // real git ancestry
    private Dictionary<string, string?>  _remoteParentMap = [];
    private bool                         _gitFlowForced   = false;

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
    private Button           _btnGitFlow  = null!;
    private TreeView         _tree        = null!;
    private StatusStrip      _status      = null!;
    private ToolStripStatusLabel _statusLbl = null!;

    // ── Tree section roots ────────────────────────────────────────────────────
    private TreeNode _localRoot   = null!;
    private TreeNode _remotesRoot = null!;
    private TreeNode _tagsRoot    = null!;

    // ── Context menu ──────────────────────────────────────────────────────────
    private ContextMenuStrip   _ctxMenu     = null!;
    private ToolStripMenuItem  _miCheckout  = null!;
    private ToolStripMenuItem  _miNewBranch = null!;
    private ToolStripMenuItem  _miMerge     = null!;
    private ToolStripMenuItem  _miRebase    = null!;
    private ToolStripMenuItem  _miRename    = null!;
    private ToolStripMenuItem  _miDelete    = null!;
    private ToolStripMenuItem  _miExpand    = null!;
    private ToolStripMenuItem  _miCollapse  = null!;
    private ToolStripMenuItem  _miRefresh   = null!;

    // ─────────────────────────────────────────────────────────────────────────
    public BranchHierarchyForm(string workingDir, Action? notifyRepoChanged = null)
    {
        _svc = new BranchHierarchyService(workingDir);
        _notifyRepoChanged = notifyRepoChanged;
        InitializeComponent();
        LoadRepositories();
        RefreshTree();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Called by the plugin when GitExtensions switches the active repository.</summary>
    public void UpdateWorkingDir(string newDir)
    {
        _svc.WorkingDir = newDir;
        if (!_cboRepo.Items.Contains(newDir))
            _cboRepo.Items.Add(newDir);
        _cboRepo.SelectedItem = newDir;
    }

    /// <summary>Re-reads branches from git and rebuilds the tree.</summary>
    public void RefreshTree()
    {
        _tree.BeginUpdate();
        try
        {
            _localBranches   = _svc.GetLocalBranches();
            _remoteBranches  = _svc.GetRemoteBranches();
            _tags            = _svc.GetTags();
            _localParentMap  = _svc.BuildParentMap(_localBranches);
            _remoteParentMap = _svc.BuildRemoteParentMap(_remoteBranches);
            UpdateGitFlowWarning();
            var localMap  = _gitFlowForced ? BuildGitFlowParentMap(_localBranches)          : _localParentMap;
            var remoteMap = _gitFlowForced ? BuildGitFlowRemoteParentMap(_remoteBranches) : _remoteParentMap;
            RebuildAllSections(_txtFilter?.Text.Trim() ?? string.Empty, localMap, remoteMap);
            ExpandRoots();
            UpdateStatus();
            UpdateBranchLabel();
        }
        finally
        {
            _tree.EndUpdate();
        }
    }

    // ── Initialization ────────────────────────────────────────────────────────

    private void InitializeComponent()
    {
        SuspendLayout();

        Text            = "ZimerfeldTree — Branch Hierarchy";
        Size            = new Size(420, 720);
        MinimumSize     = new Size(300, 450);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.SizableToolWindow;
        Font            = new Font("Segoe UI", 9f);

        BuildTopPanel();
        BuildFilterPanel();
        BuildWarnPanel();
        BuildTreeView();
        BuildContextMenu();
        BuildStatusStrip();

        // Layout order (Dock fills from bottom and top inward, Fill takes remainder)
        // Added last = topmost for DockStyle.Top; so visual order top→bottom:
        //   _topPanel, _filterPanel, _warnPanel, _tree (Fill), _status
        Controls.Add(_tree);           // Fill
        Controls.Add(_warnPanel);      // Top (between filter and tree)
        Controls.Add(_filterPanel);    // Top (below topPanel)
        Controls.Add(_topPanel);       // Top (topmost)
        Controls.Add(_status);         // Bottom

        ResumeLayout(false);
        PerformLayout();
    }

    private void BuildTopPanel()
    {
        _topPanel = new Panel { Dock = DockStyle.Top, Height = 72 };

        var table = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 1,
            RowCount    = 3,
            Padding     = new Padding(6, 4, 6, 4),
            Margin      = Padding.Empty
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _lblWD = new Label
        {
            Text     = "Working Directory:",
            AutoSize = true,
            Font     = new Font(Font, FontStyle.Bold),
            Margin   = new Padding(0, 0, 0, 2)
        };

        _cboRepo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock          = DockStyle.Fill,
            Margin        = new Padding(0, 0, 0, 2)
        };
        _cboRepo.SelectedIndexChanged += CboRepo_SelectedIndexChanged;

        _lblBranch = new Label
        {
            AutoSize = true,
            Text     = "Branch: ",
            Margin   = Padding.Empty
        };

        table.Controls.Add(_lblWD,     0, 0);
        table.Controls.Add(_cboRepo,   0, 1);
        table.Controls.Add(_lblBranch, 0, 2);

        _topPanel.Controls.Add(table);
    }

    private void BuildFilterPanel()
    {
        _filterPanel = new Panel { Dock = DockStyle.Top, Height = 28, Padding = new Padding(4, 2, 4, 2) };

        _txtFilter = new TextBox
        {
            Dock            = DockStyle.Fill,
            PlaceholderText = "Filtrar branches..."
        };
        _txtFilter.TextChanged += (_, _) => ApplyFilter(_txtFilter.Text.Trim());

        _btnRefresh = new Button
        {
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
            Dock      = DockStyle.Fill,
            Text      = string.Empty,
            ForeColor = Color.DarkRed,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(4, 0, 0, 0),
            AutoSize  = false
        };

        _btnGitFlow = new Button
        {
            Dock  = DockStyle.Right,
            Width = 160,
            Text  = "Organizar como GitFlow"
        };
        _btnGitFlow.Click += BtnGitFlow_Click;

        _warnPanel = new Panel
        {
            Dock    = DockStyle.Top,
            Height  = 26,
            Visible = false,
            Padding = new Padding(4, 2, 4, 2)
        };
        _warnPanel.Controls.Add(_warnLabel);
        _warnPanel.Controls.Add(_btnGitFlow);
    }

    private void BuildTreeView()
    {
        _tree = new TreeView
        {
            Dock          = DockStyle.Fill,
            ShowLines     = true,
            ShowPlusMinus = true,
            ShowRootLines = true,
            HideSelection = false,
            DrawMode      = TreeViewDrawMode.OwnerDrawText,
            Font          = new Font("Segoe UI", 9f)
        };

        _tree.DrawNode              += Tree_DrawNode;
        _tree.NodeMouseDoubleClick  += Tree_NodeMouseDoubleClick;
        _tree.KeyDown               += Tree_KeyDown;
        _tree.MouseDown             += Tree_MouseDown;

        _localRoot   = new TreeNode("LOCAL (0)")   { Tag = SectionTag.Local };
        _remotesRoot = new TreeNode("REMOTES (0)") { Tag = SectionTag.Remotes };
        _tagsRoot    = new TreeNode("TAGS (0)")    { Tag = SectionTag.Tags };

        _tree.Nodes.AddRange([_localRoot, _remotesRoot, _tagsRoot]);
    }

    private void BuildContextMenu()
    {
        _miCheckout  = new ToolStripMenuItem("Checkout");
        _miNewBranch = new ToolStripMenuItem("Nova branch daqui…");
        _miMerge     = new ToolStripMenuItem("Mesclar na branch atual");
        _miRebase    = new ToolStripMenuItem("Rebase na branch atual");
        _miRename    = new ToolStripMenuItem("Renomear…");
        _miDelete    = new ToolStripMenuItem("Excluir…");
        _miExpand    = new ToolStripMenuItem("Expandir tudo");
        _miCollapse  = new ToolStripMenuItem("Recolher tudo");
        _miRefresh   = new ToolStripMenuItem("Atualizar");

        _miCheckout .Click += (_, _) => DoCheckout();
        _miNewBranch.Click += (_, _) => DoNewBranch();
        _miMerge    .Click += (_, _) => DoMerge();
        _miRebase   .Click += (_, _) => DoRebase();
        _miRename   .Click += (_, _) => DoRename();
        _miDelete   .Click += (_, _) => DoDelete();
        _miExpand   .Click += (_, _) => _tree.ExpandAll();
        _miCollapse .Click += (_, _) => { _tree.CollapseAll(); ExpandRoots(); };
        _miRefresh  .Click += (_, _) => RefreshTree();

        _ctxMenu = new ContextMenuStrip();
        _ctxMenu.Opening += CtxMenu_Opening;
        _ctxMenu.Items.AddRange(
        [
            _miCheckout, _miNewBranch,
            new ToolStripSeparator(),
            _miMerge, _miRebase,
            new ToolStripSeparator(),
            _miRename, _miDelete,
            new ToolStripSeparator(),
            _miExpand, _miCollapse, _miRefresh
        ]);

        _tree.ContextMenuStrip = _ctxMenu;
    }

    private void BuildStatusStrip()
    {
        _status    = new StatusStrip();
        _statusLbl = new ToolStripStatusLabel
        {
            Text      = "Local: 0  |  Remoto: 0  |  Tags: 0",
            Spring    = true,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _status.Items.Add(_statusLbl);
    }

    // ── GitFlow enforcement ───────────────────────────────────────────────────

    private void BtnGitFlow_Click(object? sender, EventArgs e)
    {
        _gitFlowForced = !_gitFlowForced;
        RefreshTree();
    }

    private void UpdateGitFlowWarning()
    {
        var violations = GetGitFlowViolations();

        if (violations.Count == 0 && !_gitFlowForced)
        {
            _warnPanel.Visible = false;
            return;
        }

        _warnPanel.Visible = true;
        if (_gitFlowForced)
        {
            _warnLabel.Text     = "Exibindo hierarquia GitFlow forçada.";
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

        foreach (var n in BuildAncestryTree(list, localMap, CreateBranchNodeFull))
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
            var remoteNode = new TreeNode(group.Key) { Tag = SectionTag.RemoteGroup };
            var groupList  = group.ToList();

            foreach (var n in BuildAncestryTree(groupList, remoteMap, CreateRemoteBranchNode))
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

        foreach (var tag in list.OrderBy(t => t.FullName))
            _tagsRoot.Nodes.Add(CreateBranchNode(tag));
    }

    // ── Generic path-tree builder ─────────────────────────────────────────────

    /// <summary>
    /// Converts a flat list of <typeparamref name="T"/> into a tree of <see cref="TreeNode"/>
    /// nodes using '/' as the path separator, creating intermediate folder nodes where needed.
    /// </summary>
    private static List<TreeNode> BuildPathTree<T>(
        IEnumerable<T> items,
        Func<T, string> getPath,
        Func<T, string> getLeafLabel,
        Func<T, TreeNode> createLeaf)
    {
        // Build an intermediate dictionary tree: key → dict or T
        var root = new SortedDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in items.OrderBy(getPath))
        {
            var parts  = getPath(item).Split('/');
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
            cursor[parts[^1]] = item;   // leaf
        }

        return WalkDict(root, createLeaf);
    }

    private static List<TreeNode> WalkDict<T>(
        SortedDictionary<string, object> dict,
        Func<T, TreeNode> createLeaf)
    {
        var nodes = new List<TreeNode>();
        foreach (var kvp in dict)
        {
            if (kvp.Value is T leaf)
            {
                nodes.Add(createLeaf(leaf));
            }
            else if (kvp.Value is SortedDictionary<string, object> sub)
            {
                var folder = new TreeNode(kvp.Key) { Tag = SectionTag.Folder };
                foreach (var child in WalkDict(sub, createLeaf))
                    folder.Nodes.Add(child);
                nodes.Add(folder);
            }
        }
        return nodes;
    }

    // ── Ancestry tree builder ─────────────────────────────────────────────────

    /// <summary>
    /// Converts a flat list of branches into a tree using git parent-child relationships
    /// from <paramref name="parentMap"/>.  Branches whose parent is not in the displayed set
    /// become roots.
    /// </summary>
    private static List<TreeNode> BuildAncestryTree(
        List<BranchInfo> branches,
        Dictionary<string, string?> parentMap,
        Func<BranchInfo, TreeNode> createLeaf)
    {
        var nodeMap = branches.ToDictionary(b => b.FullName, b => createLeaf(b), StringComparer.Ordinal);
        var roots   = new List<TreeNode>();

        foreach (var b in branches.OrderBy(b => b.FullName))
        {
            var node = nodeMap[b.FullName];
            string? parentName = parentMap.TryGetValue(b.FullName, out var p) ? p : null;

            if (parentName != null && nodeMap.TryGetValue(parentName, out var parentNode))
                parentNode.Nodes.Add(node);
            else
                roots.Add(node);
        }

        return roots;
    }

    // ── Node factories ────────────────────────────────────────────────────────

    private TreeNode CreateBranchNode(BranchInfo info)
    {
        string label = info.FullName.Contains('/')
            ? info.FullName.Split('/')[^1]
            : info.FullName;

        if (info.IsCurrent) label = $"[{label}]";

        return new TreeNode(label)
        {
            Tag      = info,
            NodeFont = info.IsCurrent ? new Font(_tree.Font, FontStyle.Bold) : null,
            ForeColor = info.IsCurrent ? SystemColors.Highlight : _tree.ForeColor
        };
    }

    /// <summary>Branch node for remote ancestry tree — shows DisplayName (strips remote prefix).</summary>
    private TreeNode CreateRemoteBranchNode(BranchInfo info)
    {
        string label = info.IsCurrent ? $"[{info.DisplayName}]" : info.DisplayName;
        return new TreeNode(label)
        {
            Tag       = info,
            NodeFont  = info.IsCurrent ? new Font(_tree.Font, FontStyle.Bold) : null,
            ForeColor = info.IsCurrent ? SystemColors.Highlight : _tree.ForeColor
        };
    }

    /// <summary>Branch node that shows the full branch name (used in the ancestry tree).</summary>
    private TreeNode CreateBranchNodeFull(BranchInfo info)
    {
        string label = info.IsCurrent ? $"[{info.FullName}]" : info.FullName;
        return new TreeNode(label)
        {
            Tag       = info,
            NodeFont  = info.IsCurrent ? new Font(_tree.Font, FontStyle.Bold) : null,
            ForeColor = info.IsCurrent ? SystemColors.Highlight : _tree.ForeColor
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
        _localRoot.ExpandAll();   // show full ancestry tree
        _remotesRoot.Expand();
        _tagsRoot.Expand();
    }

    private void UpdateStatus()
        => _statusLbl.Text =
            $"Local: {_localBranches.Count}  |  Remoto: {_remoteBranches.Count}  |  Tags: {_tags.Count}";

    private void UpdateBranchLabel()
        => _lblBranch.Text = $"Branch: {_svc.GetCurrentBranch()}";

    private BranchInfo? SelectedBranch()
        => _tree.SelectedNode?.Tag as BranchInfo;

    // ── Tree drawing (bold + highlight for current branch) ────────────────────

    private void Tree_DrawNode(object? sender, DrawTreeNodeEventArgs e)
    {
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

    private void Tree_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Node.Tag is BranchInfo) DoCheckout();
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

        _miCheckout .Visible = branch;
        _miNewBranch.Visible = local || tag;
        _miMerge    .Visible = local;
        _miRebase   .Visible = local;
        _miRename   .Visible = local;
        _miDelete   .Visible = local || remote || tag;
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    private void DoCheckout()
    {
        var info = SelectedBranch();
        if (info is null) return;

        var (ok, err) = info.Type == BranchType.Remote
            ? _svc.CheckoutRemoteAsLocal(info.FullName)
            : _svc.Checkout(info.FullName);

        if (ok)
        {
            RefreshTree();
            _notifyRepoChanged?.Invoke();
        }
        else if (!string.IsNullOrEmpty(err))
        {
            // If branch cannot be deleted due to uncommitted changes, offer force option
            if (err.Contains("not fully merged", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(err, "Checkout falhou", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show(err, "Checkout falhou", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void DoNewBranch()
    {
        var info = SelectedBranch();
        if (info is null) return;

        using var dlg = new InputDialog("Nova branch",
            $"Nome da nova branch a partir de '{info.FullName}':");
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        var (ok, err) = _svc.CreateBranch(dlg.Value.Trim(), info.FullName);
        if (ok) RefreshTree();
        else ShowError("Erro ao criar branch", err);
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
    }

    // ── UI helpers ────────────────────────────────────────────────────────────

    private bool Confirm(string text, string caption) =>
        MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    private void ShowError(string caption, string message) =>
        MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

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

        _label = new Label  { Text = prompt, Bounds = new Rectangle(12, 12, 388, 20) };
        _input = new TextBox { Text = defaultValue, Bounds = new Rectangle(12, 36, 388, 22) };

        var ok     = new Button { Text = "OK",       Bounds = new Rectangle(228, 70, 82, 28), DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Cancelar", Bounds = new Rectangle(318, 70, 82, 28), DialogResult = DialogResult.Cancel };

        Controls.AddRange([_label, _input, ok, cancel]);
        AcceptButton = ok;
        CancelButton = cancel;
    }
}
