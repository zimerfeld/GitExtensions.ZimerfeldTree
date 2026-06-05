// RestoreForm.cs — Git restore/cherry-pick/reset window for ZimerfeldTree plugin
// MIT License — Copyright (c) 2026 Zimerfeld

namespace GitExtensions.ZimerfeldTree;

/// <summary>
/// Modal window that drives git history-restoration operations: restoring a file from a commit,
/// cherry-picking commits onto the current branch, and resetting a branch to a prior commit.
/// </summary>
public sealed class RestoreForm : Form
{
    private readonly BranchHierarchyService _svc;

    // ── Header ──
    private Label     _lblHead  = null!;
    private LinkLabel _lnkAbout = null!;

    // ── Restore File ──
    private GroupBox _grpRestoreFile = null!;
    private TextBox  _txtRestoreHash = null!;
    private TextBox  _txtRestoreFile = null!;
    private Button   _btnRestoreFile = null!;

    // ── Cherry-Pick ──
    private GroupBox _grpCherryPick = null!;
    private TextBox  _txtCherryHash = null!;
    private Button   _btnCherryPick = null!;

    // ── Reset Branch ──
    private GroupBox    _grpReset     = null!;
    private ComboBox    _cboBranch    = null!;
    private TextBox     _txtResetHash = null!;
    private RadioButton _rdMixed      = null!;
    private RadioButton _rdSoft       = null!;
    private RadioButton _rdHard       = null!;
    private Button      _btnReset     = null!;

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

    public RestoreForm(BranchHierarchyService svc)
    {
        _svc = svc;

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

        CancelButton = _btnClose;
        Load += (_, _) => InitData();
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
        _txtRestoreHash = new TextBox
        {
            Name            = "txtRestoreHash",
            PlaceholderText = "ex.: abc1234",
            Bounds          = new Rectangle(106, 24, 200, 22)
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

        _grpRestoreFile.Controls.AddRange([lblHash, _txtRestoreHash, lblFile, _txtRestoreFile, _btnRestoreFile]);
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
        _txtCherryHash = new TextBox
        {
            Name            = "txtCherryHash",
            PlaceholderText = "hash ou intervalo (ex.: abc..def)",
            Bounds          = new Rectangle(92, 20, 286, 22)
        };
        _btnCherryPick = new Button
        {
            Name   = "btnCherryPick",
            Text   = "Aplicar Cherry-Pick",
            Bounds = new Rectangle(384, 20, 144, 24)
        };
        _btnCherryPick.Click += BtnCherryPick_Click;

        _grpCherryPick.Controls.AddRange([lblHash, _txtCherryHash, _btnCherryPick]);
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
        _txtResetHash = new TextBox
        {
            Name            = "txtResetHash",
            PlaceholderText = "ex.: abc1234 ou HEAD~3",
            Bounds          = new Rectangle(106, 52, 210, 22)
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

        _grpReset.Controls.AddRange([lblBranch, _cboBranch, lblHash, _txtResetHash, _rdMixed, _rdSoft, _rdHard, _btnReset]);
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

        string? develop = branches.FirstOrDefault(b => b == "develop");
        if (develop != null)       _cboBranch.SelectedItem  = develop;
        else if (branches.Count > 0) _cboBranch.SelectedIndex = 0;
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
        string hash = _txtRestoreHash.Text.Trim();
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
        string hash = _txtCherryHash.Text.Trim();
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
        string hash = _txtResetHash.Text.Trim();
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
            "Dica: use 'git log --oneline' para localizar o hash desejado.",
            "About Restore", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static string Clean(string s) => s.Trim().Replace("\"", "");
}
