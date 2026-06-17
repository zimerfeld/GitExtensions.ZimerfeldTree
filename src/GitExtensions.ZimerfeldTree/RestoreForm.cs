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

    // This window's own language, persisted independently of the other windows (in this window's
    // settings file). Initialized to the app-wide choice as the first-open default; overwritten by
    // the saved per-window value in RestoreSettings.
    private AppLanguage _lang = I18n.Current;

    // Loaded for _lang; reassigned by ApplyLanguage when the user switches languages via the
    // bottom-panel dropdown (so the open window re-localizes live, like ZimerfeldTree).
    private Translator _t = I18n.Load("ZimerfeldRestore", I18n.Current);

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
    private Button   _btnBrowseFile  = null!;
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

    // ── Restore Tree (whole working tree from any commit) ──
    private ComboBox _cboTreeHash   = null!;
    private Button   _btnRestoreTree = null!;

    // ── Revert (safe undo — new commit) ──
    private ComboBox _cboRevertHash = null!;
    private Button   _btnRevert     = null!;
    private Button   _btnRevertMerge = null!;

    // ── New Branch / Tag from a commit (+ inspect detached) ──
    private ComboBox _cboNewRefHash = null!;
    private TextBox  _txtNewRefName = null!;
    private Button   _btnNewBranch  = null!;
    private Button   _btnNewTag     = null!;
    private Button   _btnInspect    = null!;

    // ── Reflog recovery (recover lost commits / deleted branches) ──
    private ComboBox _cboReflog        = null!;
    private TextBox  _txtReflogBranch  = null!;
    private Button   _btnReflogReset   = null!;
    private Button   _btnReflogBranch  = null!;

    // ── Discard local changes (working tree) ──
    private Button _btnDiscardUnstaged = null!;
    private Button _btnDiscardHard     = null!;
    private Button _btnClean           = null!;

    // ── Rebase (drop a commit — rewrites history) ──
    private ComboBox _cboRebaseHash  = null!;
    private Button   _btnRebaseDrop  = null!;
    private Button   _btnRebaseAbort = null!;

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
        // Width 980: enough for the now-eleven tabs and the wide hash dropdowns. The window grew
        // (was 830) to host every history-recovery operation as its own aligned tab.
        Size            = new Size(980, 824 + SponsorBanner.PanelHeight);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        StartPosition   = FormStartPosition.Manual;
        Font            = new Font("Segoe UI", 9f);
        Icon            = PluginIcon.ForForm();

        // Docked shell (6 px padding → symmetric borders): a short tab control on top holding the
        // input groups, and the result group filling the space below the tabs (like ZimerfeldGitFlow),
        // with the sponsor banner on top and a centered close-button bar below. Multiline=true so all
        // tabs are always visible (wrap to a second row) instead of hidden behind scroll arrows; the
        // extra header row is why the height is 218 (vs the old single-row 196).
        var content = new Panel { Name = "contentPanel", Dock = DockStyle.Fill, Padding = new Padding(6, 4, 6, 6) };
        _tabs = new TabControl { Name = "tabs", Dock = DockStyle.Top, Height = 218, Multiline = true };

        BuildHeader();
        // Tab order runs from the safest operations to the most destructive: restore/inspect →
        // cherry-pick/revert → reset → create-ref/reflog → discard → rebase.
        BuildEmergencyTab();
        BuildRestoreFileTab();
        BuildRestoreTreeTab();
        BuildCherryPickTab();
        BuildRevertTab();
        BuildResetTab();
        BuildNewRefTab();
        BuildReflogTab();
        BuildDiscardTab();
        BuildRebaseTab();
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
            ApplyLanguage();   // InitData loaded this window's saved language into _lang
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

        // Restore File — Browse is right-aligned on the file row; the path field stretches up to it.
        Stretch(_cboRestoreHash);
        RightAlign(_btnBrowseFile);
        _txtRestoreFile.Width = Math.Max(60, _btnBrowseFile.Left - 8 - _txtRestoreFile.Left);
        RightAlign(_btnRestoreFile);

        // Restore Tree
        Stretch(_cboTreeHash);
        RightAlign(_btnRestoreTree);

        // Cherry-Pick
        Stretch(_cboCherryHash);
        RightAlign(_btnCherryPick);

        // Revert — two right-aligned buttons (merge revert outermost, plain revert tucked left).
        Stretch(_cboRevertHash);
        RightAlign(_btnRevertMerge);
        _btnRevert.Left = _btnRevertMerge.Left - 12 - _btnRevert.Width;

        // Reset Branch — stretch the combos and right-align the Reset button on the radio row; size the
        // radios so they fill the row only up to (not over) the button, keeping the button visible.
        Stretch(_cboBranch);
        Stretch(_cboResetHash);
        RightAlign(_btnReset);
        int radioRight = _btnReset.Left - 12;
        foreach (var rd in new[] { _rdMixed, _rdSoft, _rdHard })
            rd.Width = Math.Max(60, radioRight - rd.Left);

        // New Branch/Tag — three right-aligned buttons (branch outermost, then tag, then inspect).
        Stretch(_cboNewRefHash);
        Stretch(_txtNewRefName);
        RightAlign(_btnNewBranch);
        _btnNewTag.Left  = _btnNewBranch.Left - 12 - _btnNewTag.Width;
        _btnInspect.Left = _btnNewTag.Left    - 12 - _btnInspect.Width;

        // Reflog — two right-aligned buttons (reset outermost/red, create-branch to its left).
        Stretch(_cboReflog);
        Stretch(_txtReflogBranch);
        RightAlign(_btnReflogReset);
        _btnReflogBranch.Left = _btnReflogReset.Left - 12 - _btnReflogBranch.Width;

        // Discard — three full-width stacked buttons.
        foreach (var b in new[] { _btnDiscardUnstaged, _btnDiscardHard, _btnClean })
            b.Width = Math.Max(120, b.Parent!.ClientSize.Width - SideMargin - b.Left);

        // Rebase — drop button outermost/red, abort to its left.
        Stretch(_cboRebaseHash);
        RightAlign(_btnRebaseDrop);
        _btnRebaseAbort.Left = _btnRebaseDrop.Left - 12 - _btnRebaseAbort.Width;
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
            Bounds          = new Rectangle(174, 52, 420, 22)
        };

        // "Browse…" opens a native file dialog rooted at the repository folder; the picked file is
        // converted to a path relative to the repo and rejected if it sits outside it (see Browse_Click).
        _btnBrowseFile = new Button
        {
            Name   = "btnBrowseFile",
            Text   = _t["browseBtn"],
            Bounds = new Rectangle(600, 52, 100, 22)
        };
        _btnBrowseFile.Click += BtnBrowseFile_Click;

        _btnRestoreFile = new Button
        {
            Name   = "btnRestoreFile",
            Text   = _t["restoreFileBtn"],
            Bounds = new Rectangle(550, 84, 150, 24)
        };
        _btnRestoreFile.Click += BtnRestoreFile_Click;

        AddTab("restoreFileGroup", lblHash, _cboRestoreHash, lblFile, _txtRestoreFile, _btnBrowseFile, _btnRestoreFile);

        _localized.AddRange(new KeyValuePair<Control, string>[]
        {
            new(lblHash, "commitHash"), new(lblFile, "fileRelative"),
            new(_btnBrowseFile, "browseBtn"), new(_btnRestoreFile, "restoreFileBtn"),
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
        // right on the first radio row — i.e. directly below cboResetHash with the same ~10 px margin
        // the other tabs use — and is right-aligned by LayoutResponsive.
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
            Bounds = new Rectangle(550, 84, 150, 24)   // below cboResetHash, like the other tabs' action buttons
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

    private void BuildRestoreTreeTab()
    {
        var lblHash = new Label
        {
            Text      = _t["commitHash"],
            AutoSize  = false,
            Bounds    = new Rectangle(14, 26, 90, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboTreeHash = new ComboBox
        {
            Name          = "cboTreeHash",
            DropDownStyle = ComboBoxStyle.DropDown,
            Bounds        = new Rectangle(110, 24, 590, 22),
            DropDownWidth = 590
        };
        var lblHint = new Label
        {
            Name      = "lblTreeHint",
            Text      = _t["treeHint"],
            AutoSize  = false,
            ForeColor = SystemColors.GrayText,
            Bounds    = new Rectangle(14, 54, 686, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _btnRestoreTree = new Button
        {
            Name   = "btnRestoreTree",
            Text   = _t["restoreTreeBtn"],
            Bounds = new Rectangle(550, 84, 150, 24)
        };
        _btnRestoreTree.Click += BtnRestoreTree_Click;

        AddTab("restoreTreeGroup", lblHash, _cboTreeHash, lblHint, _btnRestoreTree);

        _localized.AddRange(new KeyValuePair<Control, string>[]
        {
            new(lblHash, "commitHash"), new(lblHint, "treeHint"), new(_btnRestoreTree, "restoreTreeBtn"),
        });
    }

    private void BuildRevertTab()
    {
        var lblHash = new Label
        {
            Text      = _t["commitHash"],
            AutoSize  = false,
            Bounds    = new Rectangle(14, 26, 90, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboRevertHash = new ComboBox
        {
            Name          = "cboRevertHash",
            DropDownStyle = ComboBoxStyle.DropDown,
            Bounds        = new Rectangle(110, 24, 590, 22),
            DropDownWidth = 590
        };
        var lblHint = new Label
        {
            Name      = "lblRevertHint",
            Text      = _t["revertHint"],
            AutoSize  = false,
            ForeColor = SystemColors.GrayText,
            Bounds    = new Rectangle(14, 54, 686, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        // Two right-aligned buttons (like Emergency): plain revert and merge revert (-m 1).
        _btnRevert = new Button
        {
            Name   = "btnRevert",
            Text   = _t["revertBtn"],
            Bounds = new Rectangle(388, 84, 150, 26)
        };
        _btnRevert.Click += BtnRevert_Click;
        _btnRevertMerge = new Button
        {
            Name   = "btnRevertMerge",
            Text   = _t["revertMergeBtn"],
            Bounds = new Rectangle(550, 84, 150, 26)
        };
        _btnRevertMerge.Click += BtnRevertMerge_Click;

        AddTab("revertGroup", lblHash, _cboRevertHash, lblHint, _btnRevert, _btnRevertMerge);

        _localized.AddRange(new KeyValuePair<Control, string>[]
        {
            new(lblHash, "commitHash"), new(lblHint, "revertHint"),
            new(_btnRevert, "revertBtn"), new(_btnRevertMerge, "revertMergeBtn"),
        });
    }

    private void BuildNewRefTab()
    {
        var lblHash = new Label
        {
            Text      = _t["commitHash"],
            AutoSize  = false,
            Bounds    = new Rectangle(14, 26, 90, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboNewRefHash = new ComboBox
        {
            Name          = "cboNewRefHash",
            DropDownStyle = ComboBoxStyle.DropDown,
            Bounds        = new Rectangle(110, 24, 590, 22),
            DropDownWidth = 590
        };
        var lblName = new Label
        {
            Name      = "lblNewRefName",
            Text      = _t["newRefName"],
            AutoSize  = false,
            Bounds    = new Rectangle(14, 54, 90, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _txtNewRefName = new TextBox
        {
            Name            = "txtNewRefName",
            PlaceholderText = _t["newRefPlaceholder"],
            Bounds          = new Rectangle(110, 52, 590, 22)
        };
        // Three right-aligned buttons: create branch, create tag, inspect (detached checkout).
        _btnInspect = new Button
        {
            Name   = "btnInspect",
            Text   = _t["inspectBtn"],
            Bounds = new Rectangle(226, 84, 150, 26)
        };
        _btnInspect.Click += BtnInspect_Click;
        _btnNewTag = new Button
        {
            Name   = "btnNewTag",
            Text   = _t["newTagBtn"],
            Bounds = new Rectangle(388, 84, 150, 26)
        };
        _btnNewTag.Click += BtnNewTag_Click;
        _btnNewBranch = new Button
        {
            Name   = "btnNewBranch",
            Text   = _t["newBranchBtn"],
            Bounds = new Rectangle(550, 84, 150, 26)
        };
        _btnNewBranch.Click += BtnNewBranch_Click;

        AddTab("newRefGroup", lblHash, _cboNewRefHash, lblName, _txtNewRefName,
            _btnInspect, _btnNewTag, _btnNewBranch);

        _localized.AddRange(new KeyValuePair<Control, string>[]
        {
            new(lblHash, "commitHash"), new(lblName, "newRefName"),
            new(_btnInspect, "inspectBtn"), new(_btnNewTag, "newTagBtn"), new(_btnNewBranch, "newBranchBtn"),
        });
    }

    private void BuildReflogTab()
    {
        var lblEntry = new Label
        {
            Name      = "lblReflogEntry",
            Text      = _t["reflogEntry"],
            AutoSize  = false,
            Bounds    = new Rectangle(14, 26, 90, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboReflog = new ComboBox
        {
            Name          = "cboReflog",
            DropDownStyle = ComboBoxStyle.DropDown,
            Bounds        = new Rectangle(110, 24, 590, 22),
            DropDownWidth = 590
        };
        var lblBranch = new Label
        {
            Name      = "lblReflogBranch",
            Text      = _t["newRefName"],
            AutoSize  = false,
            Bounds    = new Rectangle(14, 54, 90, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _txtReflogBranch = new TextBox
        {
            Name            = "txtReflogBranch",
            PlaceholderText = _t["newRefPlaceholder"],
            Bounds          = new Rectangle(110, 52, 590, 22)
        };
        _btnReflogBranch = new Button
        {
            Name   = "btnReflogBranch",
            Text   = _t["reflogBranchBtn"],
            Bounds = new Rectangle(388, 84, 150, 26)
        };
        _btnReflogBranch.Click += BtnReflogBranch_Click;
        _btnReflogReset = new Button
        {
            Name      = "btnReflogReset",
            Text      = _t["reflogResetBtn"],
            ForeColor = Color.DarkRed,
            Bounds    = new Rectangle(550, 84, 150, 26)
        };
        _btnReflogReset.Click += BtnReflogReset_Click;

        AddTab("reflogGroup", lblEntry, _cboReflog, lblBranch, _txtReflogBranch,
            _btnReflogBranch, _btnReflogReset);

        _localized.AddRange(new KeyValuePair<Control, string>[]
        {
            new(lblEntry, "reflogEntry"), new(lblBranch, "newRefName"),
            new(_btnReflogBranch, "reflogBranchBtn"), new(_btnReflogReset, "reflogResetBtn"),
        });
    }

    private void BuildDiscardTab()
    {
        var lblHint = new Label
        {
            Name      = "lblDiscardHint",
            Text      = _t["discardHint"],
            AutoSize  = false,
            Bounds    = new Rectangle(14, 22, 686, 36),
            TextAlign = ContentAlignment.TopLeft
        };
        // Three stacked, full-width action buttons, increasingly destructive.
        _btnDiscardUnstaged = new Button
        {
            Name   = "btnDiscardUnstaged",
            Text   = _t["discardUnstagedBtn"],
            Bounds = new Rectangle(14, 64, 686, 26)
        };
        _btnDiscardUnstaged.Click += BtnDiscardUnstaged_Click;
        _btnDiscardHard = new Button
        {
            Name      = "btnDiscardHard",
            Text      = _t["discardHardBtn"],
            ForeColor = Color.DarkRed,
            Bounds    = new Rectangle(14, 94, 686, 26)
        };
        _btnDiscardHard.Click += BtnDiscardHard_Click;
        _btnClean = new Button
        {
            Name      = "btnClean",
            Text      = _t["cleanBtn"],
            ForeColor = Color.DarkRed,
            Bounds    = new Rectangle(14, 124, 686, 26)
        };
        _btnClean.Click += BtnClean_Click;

        AddTab("discardGroup", lblHint, _btnDiscardUnstaged, _btnDiscardHard, _btnClean);

        _localized.AddRange(new KeyValuePair<Control, string>[]
        {
            new(lblHint, "discardHint"), new(_btnDiscardUnstaged, "discardUnstagedBtn"),
            new(_btnDiscardHard, "discardHardBtn"), new(_btnClean, "cleanBtn"),
        });
    }

    private void BuildRebaseTab()
    {
        var lblHash = new Label
        {
            Text      = _t["commitHash"],
            AutoSize  = false,
            Bounds    = new Rectangle(14, 26, 90, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _cboRebaseHash = new ComboBox
        {
            Name          = "cboRebaseHash",
            DropDownStyle = ComboBoxStyle.DropDown,
            Bounds        = new Rectangle(110, 24, 590, 22),
            DropDownWidth = 590
        };
        var lblHint = new Label
        {
            Name      = "lblRebaseHint",
            Text      = _t["rebaseHint"],
            AutoSize  = false,
            ForeColor = Color.DarkRed,
            Bounds    = new Rectangle(14, 54, 686, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _btnRebaseAbort = new Button
        {
            Name   = "btnRebaseAbort",
            Text   = _t["rebaseAbortBtn"],
            Bounds = new Rectangle(388, 84, 150, 26)
        };
        _btnRebaseAbort.Click += BtnRebaseAbort_Click;
        _btnRebaseDrop = new Button
        {
            Name      = "btnRebaseDrop",
            Text      = _t["rebaseDropBtn"],
            ForeColor = Color.DarkRed,
            Bounds    = new Rectangle(550, 84, 150, 26)
        };
        _btnRebaseDrop.Click += BtnRebaseDrop_Click;

        AddTab("rebaseGroup", lblHash, _cboRebaseHash, lblHint, _btnRebaseAbort, _btnRebaseDrop);

        _localized.AddRange(new KeyValuePair<Control, string>[]
        {
            new(lblHash, "commitHash"), new(lblHint, "rebaseHint"),
            new(_btnRebaseAbort, "rebaseAbortBtn"), new(_btnRebaseDrop, "rebaseDropBtn"),
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

    /// <summary>Updates this window's own language and re-localizes in place (persisted on close).</summary>
    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        if (_suppressLangEvent) return;
        _lang = _cboLanguage.SelectedIndex switch
        {
            1 => AppLanguage.English,
            2 => AppLanguage.Portuguese,
            _ => AppLanguage.Automatic,
        };
        ApplyLanguage();
    }

    /// <summary>Reloads _lang's dictionary and reapplies every registered text in place.</summary>
    private void ApplyLanguage()
    {
        _t = I18n.Load("ZimerfeldRestore", _lang);
        Text = _t["title"];
        foreach (var (c, key) in _localized) c.Text = _t[key];
        _lblHead.Text = _t.F("headLabel", _svc.GetHeadRef());
        _txtRestoreFile.PlaceholderText = _t["filePlaceholder"];

        // Re-localize the "Select..." prompt sitting at index 0 of each hash combo (and the reflog combo).
        foreach (var cbo in HashCombos.Append(_cboReflog))
        {
            if (cbo.Items.Count == 0 || cbo.Items[0] is not string) continue;
            bool wasPrompt = cbo.SelectedIndex == 0;
            cbo.Items[0] = _t["selectPrompt"];
            if (wasPrompt) cbo.SelectedIndex = 0;
        }

        PopulateLanguageCombo();
    }

    /// <summary>Every commit-hash dropdown, populated from the same recent-commits list in InitData.</summary>
    private ComboBox[] HashCombos => new[]
    {
        _cboRestoreHash, _cboTreeHash, _cboCherryHash, _cboRevertHash,
        _cboResetHash, _cboNewRefHash, _cboRebaseHash,
    };

    /// <summary>Repopulates the language dropdown with localized labels and selects this window's _lang.</summary>
    private void PopulateLanguageCombo()
    {
        _suppressLangEvent = true;
        _cboLanguage.Items.Clear();
        _cboLanguage.Items.AddRange([_t["langAutomatic"], _t["langEnglish"], _t["langPortuguese"]]);
        _cboLanguage.SelectedIndex = (int)_lang;
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
        foreach (var cbo in HashCombos)
        {
            // index 0 is the "Select..." prompt; the commit refs follow (already newest-first).
            cbo.Items.Add(_t["selectPrompt"]);
            cbo.Items.AddRange(refs.Cast<object>().ToArray());
            cbo.SelectedIndex = 0;
        }

        // Reflog tab: list the HEAD movement history (reset/rebase/checkout/commit) so a "lost"
        // commit or a deleted branch can be recovered. index 0 is the "Select..." prompt.
        _cboReflog.Items.Add(_t["selectPrompt"]);
        _cboReflog.Items.AddRange(LoadReflogRefs().Cast<object>().ToArray());
        _cboReflog.SelectedIndex = 0;

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

    /// <summary>
    /// Loads the HEAD reflog (most recent first) as selectable refs. Each entry records a movement
    /// of HEAD — commit, reset, rebase, checkout, merge — letting the user jump a branch back to a
    /// state that is no longer reachable from any branch (e.g. after a bad reset --hard, or to
    /// recover a deleted branch). %gd is the reflog selector (HEAD@{n}), %gs the reflog subject.
    /// </summary>
    private List<CommitRef> LoadReflogRefs()
    {
        var (output, _) = _svc.RunGitFlow(
            "log -g -150 --date=format:\"%Y-%m-%d %H:%M:%S\" " +
            "--pretty=format:\"%h%x1f%gd%x1f%cd%x1f%gs\"");
        var refs = new List<CommitRef>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('\x1f');
            if (parts.Length == 4 && parts[0].Length >= 7)
                // name = reflog subject, source = selector (HEAD@{n}); reflog is already chronological.
                refs.Add(new CommitRef(parts[3], parts[0], parts[1], parts[2]));
        }
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
                ["resetMode"]   = _rdHard.Checked ? "hard" : _rdSoft.Checked ? "soft" : "mixed",
                // Show Debug and language are remembered per-window (this file is exclusive to Restore).
                ["showDebug"]   = _chkShowDebug.Checked ? "1" : "0",
                ["language"]    = _lang.ToString()
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

        // Show Debug: restore this window's own saved state; with no saved value, keep the
        // constructor default (the owner's Show-Debug state passed in via showControlIds).
        if (saved.TryGetValue("showDebug", out var sd) && sd.Length > 0)
            _chkShowDebug.Checked = sd is "1" or "true";

        // Language: restore this window's own saved choice; with no saved value, keep the app-wide
        // default (_lang was initialized to I18n.Current). ApplyLanguage (called next) re-localizes.
        if (saved.TryGetValue("language", out var lng) && Enum.TryParse<AppLanguage>(lng, out var pl))
            _lang = pl;
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

    /// <summary>
    /// Opens a native file-picker rooted at the repository folder and writes the chosen file back as a
    /// path relative to the repo. The managed OpenFileDialog cannot hard-block navigating elsewhere, so
    /// the guard is enforced after selection: a file outside the repo root is rejected with a warning.
    /// </summary>
    private void BtnBrowseFile_Click(object? sender, EventArgs e)
    {
        string root = Path.GetFullPath(_svc.WorkingDir).TrimEnd(Path.DirectorySeparatorChar);
        using var dlg = new OpenFileDialog
        {
            Title            = _t["restoreFileTitle"],
            InitialDirectory = root,
            RestoreDirectory = false,
            CheckFileExists  = false,
            Filter           = _t["allFilesFilter"] + "|*.*"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        string picked = Path.GetFullPath(dlg.FileName);
        string rootWithSep = root + Path.DirectorySeparatorChar;
        if (!picked.StartsWith(rootWithSep, StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(_t.F("fileOutsideRepo", root),
                _t["restoreFileTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        // Store the repo-relative path with forward slashes (git's path convention).
        _txtRestoreFile.Text = Path.GetRelativePath(root, picked).Replace('\\', '/');
    }

    private void BtnRestoreTree_Click(object? sender, EventArgs e)
    {
        string hash = HashOf(_cboTreeHash);
        if (hash.Length == 0)
        {
            MessageBox.Show(_t["informHashTarget"],
                _t["restoreTreeTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        // Stages the whole tracked tree from that commit; history untouched (mirrors Emergency-restore).
        bool ok = RunGit($"checkout {Clean(hash)} -- .");
        if (ok) RevealInTree(null);
    }

    private void BtnRevert_Click(object? sender, EventArgs e)   => DoRevert(merge: false);
    private void BtnRevertMerge_Click(object? sender, EventArgs e) => DoRevert(merge: true);

    /// <summary>
    /// Reverts the selected commit by creating a NEW commit that undoes it — the safe way to undo on a
    /// branch already shared/pushed (no history rewrite). For a merge commit, -m 1 reverts relative to
    /// the first parent (the branch that received the merge).
    /// </summary>
    private void DoRevert(bool merge)
    {
        string hash = HashOf(_cboRevertHash);
        if (hash.Length == 0)
        {
            MessageBox.Show(_t["informHashTarget"],
                _t["revertTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        string args = merge
            ? $"revert -m 1 --no-edit {Clean(hash)}"
            : $"revert --no-edit {Clean(hash)}";
        bool ok = RunGit(args);
        if (ok) RevealInTree(null);
    }

    private void BtnNewBranch_Click(object? sender, EventArgs e) => DoNewRef(tag: false);
    private void BtnNewTag_Click(object? sender, EventArgs e)    => DoNewRef(tag: true);

    /// <summary>Creates a branch or tag pointing at the chosen commit — a non-destructive way to
    /// "fork off" a past state while leaving every existing branch untouched.</summary>
    private void DoNewRef(bool tag)
    {
        string hash = HashOf(_cboNewRefHash);
        string name = _txtNewRefName.Text.Trim();
        if (hash.Length == 0 || name.Length == 0)
        {
            MessageBox.Show(_t["informHashAndName"],
                _t["newRefTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        bool ok = RunGit($"{(tag ? "tag" : "branch")} {Clean(name)} {Clean(hash)}");
        if (ok) RevealInTree(tag ? null : name);
    }

    /// <summary>Checks out the chosen commit as a detached HEAD so the user can inspect that exact
    /// version of the code (read-only time travel). No branch is moved.</summary>
    private void BtnInspect_Click(object? sender, EventArgs e)
    {
        string hash = HashOf(_cboNewRefHash);
        if (hash.Length == 0)
        {
            MessageBox.Show(_t["informHashTarget"],
                _t["newRefTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var dr = MessageBox.Show(_t["confirmInspectWarn"],
            _t["newRefTitle"], MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        if (dr != DialogResult.Yes) return;
        bool ok = RunGit($"checkout {Clean(hash)}");
        if (ok) RevealInTree(null);
    }

    private void BtnReflogBranch_Click(object? sender, EventArgs e)
    {
        string sha  = HashOf(_cboReflog);
        string name = _txtReflogBranch.Text.Trim();
        if (sha.Length == 0 || name.Length == 0)
        {
            MessageBox.Show(_t["informReflogAndName"],
                _t["reflogTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        // Recreate a branch at the reflog entry — recovers a deleted branch / lost commit.
        bool ok = RunGit($"branch {Clean(name)} {Clean(sha)}");
        if (ok) RevealInTree(name);
    }

    private void BtnReflogReset_Click(object? sender, EventArgs e)
    {
        string sha = HashOf(_cboReflog);
        if (sha.Length == 0)
        {
            MessageBox.Show(_t["informReflogEntry"],
                _t["reflogTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var dr = MessageBox.Show(_t.F("confirmReflogResetWarn", _svc.GetCurrentBranch(), sha),
            _t["reflogTitle"], MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (dr != DialogResult.Yes) return;
        bool ok = RunGit($"reset --hard {Clean(sha)}");
        if (ok) RevealInTree(_svc.GetCurrentBranch());
    }

    private void BtnDiscardUnstaged_Click(object? sender, EventArgs e)
    {
        var dr = MessageBox.Show(_t["confirmDiscardUnstaged"],
            _t["discardTitle"], MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (dr != DialogResult.Yes) return;
        // Discards unstaged changes to tracked files only (staged content and untracked files survive).
        bool ok = RunGit("checkout -- .");
        if (ok) RevealInTree(null);
    }

    private void BtnDiscardHard_Click(object? sender, EventArgs e)
    {
        var dr = MessageBox.Show(_t["confirmDiscardHard"],
            _t["discardTitle"], MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (dr != DialogResult.Yes) return;
        // Resets the working tree and index back to HEAD — discards every staged/unstaged change.
        bool ok = RunGit("reset --hard HEAD");
        if (ok) RevealInTree(null);
    }

    private void BtnClean_Click(object? sender, EventArgs e)
    {
        var dr = MessageBox.Show(_t["confirmClean"],
            _t["discardTitle"], MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (dr != DialogResult.Yes) return;
        // Removes untracked files and directories (does not touch tracked or ignored files).
        bool ok = RunGit("clean -fd");
        if (ok) RevealInTree(null);
    }

    private void BtnRebaseDrop_Click(object? sender, EventArgs e)
    {
        string hash = HashOf(_cboRebaseHash);
        if (hash.Length == 0)
        {
            MessageBox.Show(_t["informHashTarget"],
                _t["rebaseTitle"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var dr = MessageBox.Show(_t.F("confirmRebaseDrop", hash),
            _t["rebaseTitle"], MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (dr != DialogResult.Yes) return;

        // Replay every commit AFTER <hash> onto <hash>'s parent, dropping <hash> itself. A conflict
        // leaves the rebase in progress; surface that so the user can resolve or hit "Abort Rebase".
        string safe = Clean(hash);
        bool ok = RunGit($"rebase --onto {safe}^ {safe}");
        if (ok) RevealInTree(_svc.GetCurrentBranch());
        else    _txtResult.Text += "\r\n\r\n" + _t["rebaseConflictHint"];
    }

    private void BtnRebaseAbort_Click(object? sender, EventArgs e)
    {
        bool ok = RunGit("rebase --abort");
        if (ok) RevealInTree(_svc.GetCurrentBranch());
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

    /// <summary>
    /// Shows the About dialog. The explanatory text is long (it documents every tab plus the
    /// collaboration guidance), so it is hosted in a scrollable read-only text box inside a sized,
    /// centered dialog rather than a MessageBox, which would overflow the screen.
    /// </summary>
    private void ShowAbout()
    {
        using var dlg = new Form
        {
            Text            = _t["aboutTitle"],
            FormBorderStyle = FormBorderStyle.Sizable,
            StartPosition   = FormStartPosition.CenterParent,
            MinimizeBox     = false,
            MaximizeBox     = true,
            ShowIcon        = false,
            ShowInTaskbar   = false,
            Size            = new Size(760, 640),
            MinimumSize     = new Size(480, 360),
            Font            = new Font("Segoe UI", 9f)
        };
        var txt = new TextBox
        {
            Multiline  = true,
            ReadOnly   = true,
            ScrollBars = ScrollBars.Vertical,
            WordWrap   = true,
            BorderStyle = BorderStyle.None,
            BackColor  = SystemColors.Window,
            Dock       = DockStyle.Fill,
            Text       = _t["aboutBody"].Replace("\n", "\r\n")
        };
        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 44 };
        var ok = new Button { Text = _t["closeBtn"], Width = 90, Height = 28, DialogResult = DialogResult.OK };
        bottom.Controls.Add(ok);
        bottom.Layout += (_, _) => ok.Location = new Point(
            (bottom.Width - ok.Width) / 2, (bottom.Height - ok.Height) / 2);
        var pad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 12, 12, 6) };
        pad.Controls.Add(txt);
        dlg.Controls.Add(pad);
        dlg.Controls.Add(bottom);
        dlg.AcceptButton = ok;
        dlg.CancelButton = ok;
        txt.Select(0, 0);
        dlg.ShowDialog(this);
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
