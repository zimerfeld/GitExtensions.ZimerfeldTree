// ZimerfeldTreePlugin.cs — MEF plugin entry point for ZimerfeldTree
// MIT License — Copyright (c) 2026 Zimerfeld

using System.ComponentModel.Composition;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;

namespace GitExtensions.ZimerfeldTree;

/// <summary>
/// GitExtensions plugin that displays a hierarchical branch tree in a persistent non-modal window.
/// Registered via MEF so GitExtensions discovers it automatically at startup.
/// </summary>
[Export(typeof(IGitPlugin))]
public sealed class ZimerfeldTreePlugin : GitPluginBase
{
    // Singleton form — one per GitExtensions session
    private BranchHierarchyForm? _form;

    // Current commands instance — updated by Register/Unregister as repos change.
    // Used to open the native commit dialog so Commit Template plugins are visible.
    private IGitUICommands? _commands;

    // F3 global shortcut filter — intercepts F3 anywhere in the GitExtensions process.
    private F3MessageFilter? _f3Filter;

    // ── Constructor ───────────────────────────────────────────────────────────

    public ZimerfeldTreePlugin() : base(false)
    {
        // false = plugin has no configurable settings in the GitExtensions settings dialog
        Name        = "ZimerfeldTree";
        Description = "Visualiza branches hierarquicamente em estrutura de árvore (ZimerfeldTree)";
        Icon        = TreeOfLifeIcon.ForMenu();
    }

    // ── IGitPlugin ────────────────────────────────────────────────────────────

    /// <summary>
    /// Called when the user clicks Plugins → ZimerfeldTree.
    /// Opens the window (or brings it to the front if already open).
    /// </summary>
    public override bool Execute(GitUIEventArgs args)
    {
        string workDir = args.GitModule?.WorkingDir ?? string.Empty;

        if (_form is null || _form.IsDisposed)
        {
            // Notify GitExtensions to refresh its own UI after a checkout
            Action? notifyChanged = null;
            try { notifyChanged = () => args.GitUICommands?.RepoChangedNotifier?.Notify(); }
            catch { /* RepoChangedNotifier may not be available in every build */ }

            // Open the native commit dialog in-process so Commit Template plugins load correctly.
            // Returns null when unavailable (caller falls back to spawning a new process).
            Func<IWin32Window, bool?> openCommit = owner =>
            {
                try { return _commands?.StartCommitDialog(owner, string.Empty, false); }
                catch { return null; }
            };

            // Open the native push dialog in-process; returns true if push was completed.
            Func<IWin32Window, bool> openPush = owner =>
            {
                if (_commands is null) return false;
                try
                {
                    bool pushCompleted = false;
                    _commands.StartPushDialog(owner, pushOnShow: true, forceWithLease: false, pushCompleted: out pushCompleted);
                    return pushCompleted;
                }
                catch { return false; }
            };

            _form = new BranchHierarchyForm(workDir, notifyChanged, openCommit, openPush);
            _form.FormClosed += (_, _) => _form = null;
        }
        else
        {
            // Update working dir in case the user switched repos since last open
            _form.UpdateWorkingDir(workDir);
        }

        _form.Show();
        _form.BringToFront();

        // Return false: GitExtensions should NOT refresh its own UI (we manage our own state)
        return false;
    }

    /// <summary>Subscribe to GitExtensions events so the tree stays in sync automatically.</summary>
    public override void Register(IGitUICommands commands)
    {
        base.Register(commands);
        _commands = commands;

        commands.PostBrowseInitialize   += OnRepositoryChanged;
        commands.PostCheckoutBranch     += OnBranchChanged;
        commands.PostCheckoutRevision   += OnBranchChanged;
        commands.PostCommit             += OnPostCommit;   // dedicated: also restores focus
        commands.PostRepositoryChanged  += OnExternalChange;

        // Register F3 as a process-wide shortcut that brings ZimerfeldTree to the front.
        // Guard against double-registration if Register is called more than once per session.
        if (_f3Filter is null)
        {
            _f3Filter = new F3MessageFilter(() => _form);
            Application.AddMessageFilter(_f3Filter);
        }
    }

    /// <summary>Unsubscribe from all events.</summary>
    public override void Unregister(IGitUICommands commands)
    {
        commands.PostBrowseInitialize   -= OnRepositoryChanged;
        commands.PostCheckoutBranch     -= OnBranchChanged;
        commands.PostCheckoutRevision   -= OnBranchChanged;
        commands.PostCommit             -= OnPostCommit;
        commands.PostRepositoryChanged  -= OnExternalChange;

        if (_f3Filter != null)
        {
            Application.RemoveMessageFilter(_f3Filter);
            _f3Filter = null;
        }

        _commands = null;
        base.Unregister(commands);
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void OnRepositoryChanged(object? sender, GitUIEventArgs e)
    {
        if (_form is null || _form.IsDisposed) return;

        string newDir = e.GitModule?.WorkingDir ?? string.Empty;
        _form.InvokeIfRequired(() =>
        {
            _form.UpdateWorkingDir(newDir);
            _form.RefreshTree();
        });
    }

    private void OnBranchChanged(object? sender, GitUIPostActionEventArgs e)
    {
        if (_form is null || _form.IsDisposed) return;
        _form.InvokeIfRequired(() => _form.RefreshTree());
    }

    // Separate handler for PostCommit: refreshes the tree AND restores focus to ZimerfeldTree,
    // so the window comes back to the front after the GitExtensions Commit dialog closes.
    private void OnPostCommit(object? sender, GitUIPostActionEventArgs e)
    {
        if (_form is null || _form.IsDisposed) return;
        _form.InvokeIfRequired(() =>
        {
            _form.RefreshTree();
            _form.FocusAfterCommit();
        });
    }

    private void OnExternalChange(object? sender, GitUIEventArgs e)
    {
        if (_form is null || _form.IsDisposed) return;
        _form.InvokeIfRequired(() => _form.RefreshTree());
    }
}

// ── F3 process-wide keyboard shortcut ────────────────────────────────────────

/// <summary>
/// Intercepts F3 anywhere inside the GitExtensions process and brings ZimerfeldTree to the front.
/// Text-input controls (TextBox, RichTextBox, ComboBox) are excluded so find/filter F3 keys
/// are not swallowed while the user is typing.
/// </summary>
internal sealed class F3MessageFilter(Func<BranchHierarchyForm?> getForm) : IMessageFilter
{
    private const int WM_KEYDOWN = 0x0100;
    private const int VK_F3     = 0x72;

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg != WM_KEYDOWN || (int)m.WParam != VK_F3) return false;

        var form = getForm();
        if (form is null || form.IsDisposed) return false;

        // Let F3 pass through when the user is typing in a text input.
        var focused = Control.FromHandle(m.HWnd);
        if (focused is TextBox or RichTextBox or ComboBox) return false;

        form.InvokeIfRequired(() =>
        {
            if (!form.Visible) form.Show();
            form.BringToFront();
            form.Activate();
        });
        return true; // consumed — GitExtensions does not process this F3
    }
}

// ── Thread-marshal helper extension ──────────────────────────────────────────

internal static class ControlExtensions
{
    /// <summary>Marshals <paramref name="action"/> to the UI thread if needed.</summary>
    public static void InvokeIfRequired(this Control control, Action action)
    {
        if (control.InvokeRequired)
            control.BeginInvoke(action);
        else
            action();
    }
}
