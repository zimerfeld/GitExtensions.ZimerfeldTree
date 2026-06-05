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

    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GitExtensions", "ZimerfeldRestore.settings.json");

    // ── Header ──
    private Label     _lblHead  = null!;
    private LinkLabel _lnkAbout = null!;

    // ── Restore File ──
    private GroupBox _grpRestoreFile = null!;
    private ComboBox _cboRestoreHash = null!;
    private TextBox  _txtRestoreFile = null!;
    private Button   _btnRestoreFile = null!;

    // ── Cherry-Pick ──
    private GroupBox _grpCherryPick = null!;
    private ComboBox _cboCherryHash = null!;
    private Button   _btnCherryPick = null!;

    // ── Reset Branch ──
    private GroupBox    _grpReset    = null!;
    private ComboBox    _cboBranch   = null!;
    private ComboBox    _cboResetHash = null!;
    private RadioButton _rdMixed     = null!;
    private RadioButton _rdSoft      = null!;
    private RadioButton _rdHard      = null!;
    private Button      _btnReset    = null!;

    // ── Result ──
    private GroupBox _grpResult = null!;
    private TextBox  _txtResult = null!;

    // ── Close ──
    private Button _btnClose = null!;

    /// <summary>
    /// Raised after a restore operation mutates the repository so the owning ZimerfeldTree window
    /// can refresh its tree. The argument is the branch to reveal, or null to only refresh.
    /// </summary>
    public event Action<string?>? RepoMutated;

    public RestoreForm(BranchHierarchyService svc, bool showControlIds = false)
    {
        _svc            = svc;
        _showControlIds = showControlIds;

        Text            = "ZimerfeldTree - Restore";
        Size            = new Size(560, 720);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        StartPosition   = FormStartPosition.Manual;
        Font            = new Font("Segoe UI", 9f);
        Icon            = TreeOfLifeIcon.ForForm();

        BuildHeader();
        BuildRestoreFileGroup();
        BuildCherryPickGroup();
        BuildResetGroup();
        BuildResultGroup();
        BuildCloseButton();

        CancelButton  = _btnClose;
        Load         += (_, _) =>
        {
            InitData();
            if (_showControlIds) ApplyControlTooltips();
        };
        FormClosing  += (_, _) => SaveSettings();
    }

    // ── Build UI ────────────────────────────────────────────────────────────

    private void BuildHeader()
    {
        _lblHead = new Label
        {
            Name      = "lblHead",
            TextAlign = ContentAlignment.MiddleLeft,
            Bounds    = new Rectangle(12, 10, 380, 20),
            Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        _lnkAbout = new LinkLabel
        {
            Name     = "lnkAbout",
            Text     = "About Restore",
            AutoSize = true,
            Anchor   = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(ClientSize.Width - 116, 12)
        };
        _lnkAbout.LinkClicked += (_, _) => ShowAbout();
        Controls.AddRange([_lblHead, _lnkAbout]);
    }

    private void BuildRestoreFileGroup()
    {
        _grpRestoreFile = new GroupBox
        {
            Name   = "grpRestoreFile",
            Text   = "Restaurar Arquivo",
            Bounds = new Rectangle(8, 36, 536, 112),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var lblHash = new Label
        {
            Text      = "Commit hash:",
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
            Text      = "Arquivo (caminho relativo):",
            AutoSize  = false,
            Bounds    = new Rectangle(12, 54, 172, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _txtRestoreFile = new TextBox
        {
            Name            = "txtRestoreFile",
            PlaceholderText = "ex.: src/Foo/Bar.cs",
            Bounds          = new Rectangle(188, 52, 218, 22)
        };

        _btnRestoreFile = new Button
        {
            Name   = "btnRestoreFile",
            Text   = "Restaurar Arquivo",
            Bounds = new Rectangle(384, 82, 144, 24)
        };
        _btnRestoreFile.Click += BtnRestoreFile_Click;

        _grpRestoreFile.Controls.AddRange([lblHash, _cboRestoreHash, lblFile, _txtRestoreFile, _btnRestoreFile]);
        Controls.Add(_grpRestoreFile);
    }

    private void BuildCherryPickGroup()
    {
        _grpCherryPick = new GroupBox
        {
            Name   = "grpCherryPick",
            Text   = "Cherry-Pick",
            Bounds = new Rectangle(8, 156, 536, 58),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var lblHash = new Label
        {
            Text      = "Commit(s):",
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
            Text   = "Aplicar Cherry-Pick",
            Bounds = new Rectangle(384, 20, 144, 24)
        };
        _btnCherryPick.Click += BtnCherryPick_Click;

        _grpCherryPick.Controls.AddRange([lblHash, _cboCherryHash, _btnCherryPick]);
        Controls.Add(_grpCherryPick);
    }

    private void BuildResetGroup()
    {
        _grpReset = new GroupBox
        {
            Name   = "grpReset",
            Text   = "Reset Branch",
            Bounds = new Rectangle(8, 222, 536, 152),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var lblBranch = new Label
        {
            Text      = "Branch:",
            AutoSize  = false,
            Bounds    = new Rectangle(12, 26, 54, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboBranch = new ComboBox
        {
            Name          = "cboBranch",
            DropDownStyle = ComboBoxStyle.DropDownList,
            Bounds        = new Rectangle(70, 24, 210, 22)
        };

        var lblHash = new Label
        {
            Text      = "Commit hash:",
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
            Text    = "--mixed  (mantém mudanças como unstaged — padrão)",
            Bounds  = new Rectangle(12, 82, 350, 20),
            Checked = true
        };
        _rdSoft = new RadioButton
        {
            Text   = "--soft   (mantém mudanças como staged)",
            Bounds = new Rectangle(12, 104, 310, 20)
        };
        _rdHard = new RadioButton
        {
            Text      = "--hard   (DESCARTA TUDO — irreversível)",
            Bounds    = new Rectangle(12, 126, 310, 20),
            ForeColor = Color.DarkRed
        };

        _btnReset = new Button
        {
            Name   = "btnReset",
            Text   = "Resetar Branch",
            Bounds = new Rectangle(384, 122, 144, 24)
        };
        _btnReset.Click += BtnReset_Click;

        _grpReset.Controls.AddRange([lblBranch, _cboBranch, lblHash, _cboResetHash, _rdMixed, _rdSoft, _rdHard, _btnReset]);
        Controls.Add(_grpReset);
    }

    private void BuildResultGroup()
    {
        _grpResult = new GroupBox
        {
            Name   = "grpResult",
            Text   = "Resultado:",
            Bounds = new Rectangle(8, 382, 536, 248),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };

        _txtResult = new TextBox
        {
            Name       = "txtResult",
            Multiline  = true,
            ReadOnly   = true,
            ScrollBars = ScrollBars.Both,
            WordWrap   = false,
            BackColor  = SystemColors.Window,
            Font       = new Font("Consolas", 9f),
            Bounds     = new Rectangle(10, 22, 516, 218),
            Anchor     = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
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
            Bounds       = new Rectangle(235, 648, 90, 28),
            Anchor       = AnchorStyles.Bottom,
            DialogResult = DialogResult.Cancel
        };
        _btnClose.Click += (_, _) => Close();
        Controls.Add(_btnClose);
    }

    // ── Initialization ───────────────────────────────────────────────────────

    private void InitData()
    {
        _lblHead.Text = "HEAD:  " + _svc.GetHeadRef();

        var (branchOutput, _) = _svc.RunGitFlow("branch");
        var branches = branchOutput
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(b => b.TrimStart('*', ' ').Trim())
            .Where(b => b.Length > 0)
            .ToList();

        _cboBranch.Items.AddRange(branches.Cast<object>().ToArray());

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
        return code == 0;
    }

    // ── Button handlers ──────────────────────────────────────────────────────

    private void BtnRestoreFile_Click(object? sender, EventArgs e)
    {
        string hash = HashOf(_cboRestoreHash);
        string file = _txtRestoreFile.Text.Trim();
        if (hash.Length == 0 || file.Length == 0)
        {
            MessageBox.Show("Informe o commit hash e o caminho do arquivo.",
                "Restaurar Arquivo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        bool ok = RunGit($"checkout {Clean(hash)} -- \"{Clean(file)}\"");
        if (ok) RepoMutated?.Invoke(null);
    }

    private void BtnCherryPick_Click(object? sender, EventArgs e)
    {
        string hash = HashOf(_cboCherryHash);
        if (hash.Length == 0)
        {
            MessageBox.Show("Informe o commit hash ou intervalo.",
                "Cherry-Pick", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        bool ok = RunGit($"cherry-pick {hash.Replace("\"", "")}");
        if (ok) RepoMutated?.Invoke(null);
    }

    private void BtnReset_Click(object? sender, EventArgs e)
    {
        if (_cboBranch.SelectedItem is not string branch || branch.Length == 0)
        {
            MessageBox.Show("Selecione uma branch para resetar.",
                "Reset", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        string hash = HashOf(_cboResetHash);
        if (hash.Length == 0)
        {
            MessageBox.Show("Informe o commit hash de destino.",
                "Reset", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string mode = _rdHard.Checked ? "--hard" : _rdSoft.Checked ? "--soft" : "--mixed";

        if (_rdHard.Checked)
        {
            var dr = MessageBox.Show(
                $"ATENÇÃO: git reset --hard descartará TODOS os arquivos não commitados em '{branch}'.\n\n" +
                "Esta ação é IRREVERSÍVEL. Deseja continuar?",
                "Confirmar Reset --hard", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
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
        if (ok) RepoMutated?.Invoke(branch);

        if (needSwitch)
            RunGit($"checkout {current}", append: true);
    }

    // ── About ────────────────────────────────────────────────────────────────

    private void ShowAbout()
    {
        MessageBox.Show(
            "Botões:\n\n" +
            "  Restaurar Arquivo\n" +
            "    Recupera um arquivo específico do estado de um commit antigo.\n" +
            "    Equivale a: git checkout <hash> -- <arquivo>\n" +
            "    As mudanças ficam staged, prontas para commit.\n\n" +
            "  Cherry-Pick\n" +
            "    Aplica um ou mais commits sobre a branch atual.\n" +
            "    Equivale a: git cherry-pick <hash>\n" +
            "    Para um intervalo use: <hash-antigo>..<hash-recente>\n\n" +
            "  Reset Branch\n" +
            "    Move o ponteiro da branch para um commit anterior.\n" +
            "    --mixed  Desfaz commits, mantém mudanças como unstaged (padrão).\n" +
            "    --soft   Desfaz commits, mantém mudanças como staged.\n" +
            "    --hard   Desfaz commits e DESCARTA todas as mudanças locais.\n\n" +
            "Como localizar um commit hash:\n\n" +
            "  git log --oneline             lista todos os commits do HEAD\n" +
            "  git log --oneline -20         últimos 20 commits\n" +
            "  git log --oneline <branch>    commits de uma branch específica\n" +
            "  git log --oneline A..B        commits em B que não estão em A\n" +
            "  git log --oneline --all       commits de todas as branches\n\n" +
            "Os dropdowns de hash exibem o HEAD de cada branch local.\n" +
            "Você pode selecionar uma branch ou digitar qualquer hash manualmente.",
            "About Restore", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
