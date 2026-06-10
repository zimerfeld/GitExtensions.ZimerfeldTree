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

    // ── Constructor ───────────────────────────────────────────────────────────

    public ZimerfeldTreePlugin() : base(false)
    {
        // false = plugin has no configurable settings in the GitExtensions settings dialog
        Name        = "ZimerfeldTree";
        Description = "Visualiza branches hierarquicamente em estrutura de árvore (ZimerfeldTree)";
        Icon        = PluginIcon.ForMenu();
    }

    // ── IGitPlugin ────────────────────────────────────────────────────────────

    /// <summary>
    /// Called when the user clicks Plugins → ZimerfeldTree.
    /// Opens the window (or brings it to the front if already open).
    /// </summary>
    public override bool Execute(GitUIEventArgs args)
    {
        string workDir = args.GitModule?.WorkingDir ?? string.Empty;
        DebugLog($"Execute    inst=#{_instanceId} dir='{workDir}'");

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

        // Minimize the GitExtensions main window so ZimerfeldTree takes center stage.
        var gitForm = args.OwnerForm as Form
                   ?? Control.FromHandle(args.OwnerForm?.Handle ?? IntPtr.Zero) as Form;
        gitForm?.BeginInvoke(() => gitForm.WindowState = FormWindowState.Minimized);

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

        string regDir = commands.Module?.WorkingDir ?? string.Empty;
        DebugLog($"Register   inst=#{_instanceId} formOpen={_form is { IsDisposed: false }} dir='{regDir}'");

        // The host calls Register for the UICommands of the NEWLY active repo whenever it switches
        // repositories (e.g. the "Change Working Directory" dropdown). If the tree window is already
        // open, adopt that working dir here — this is the reliable switch signal even when no Post*
        // event follows. Guards in UpdateWorkingDir make it a no-op when the repo did not change.
        if (_form is { IsDisposed: false } form && !string.IsNullOrEmpty(regDir))
        {
            form.InvokeIfRequired(() =>
            {
                form.UpdateWorkingDir(regDir);
                form.RefreshTree();
            });
        }
    }

    /// <summary>Unsubscribe from all events.</summary>
    public override void Unregister(IGitUICommands commands)
    {
        DebugLog($"Unregister inst=#{_instanceId} dir='{commands.Module?.WorkingDir ?? string.Empty}'");
        commands.PostBrowseInitialize   -= OnRepositoryChanged;
        commands.PostCheckoutBranch     -= OnBranchChanged;
        commands.PostCheckoutRevision   -= OnBranchChanged;
        commands.PostCommit             -= OnPostCommit;
        commands.PostRepositoryChanged  -= OnExternalChange;

        _commands = null;
        base.Unregister(commands);
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void OnRepositoryChanged(object? sender, GitUIEventArgs e)
    {
        string newDir = e.GitModule?.WorkingDir ?? string.Empty;
        DebugLog($"PostBrowseInitialize inst=#{_instanceId} formOpen={_form is { IsDisposed: false }} dir='{newDir}'");
        if (_form is null || _form.IsDisposed) return;

        _form.InvokeIfRequired(() =>
        {
            _form.UpdateWorkingDir(newDir);
            _form.RefreshTree();
        });
    }

    private void OnBranchChanged(object? sender, GitUIPostActionEventArgs e)
    {
        DebugLog($"PostCheckout inst=#{_instanceId} formOpen={_form is { IsDisposed: false }}");
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

    // PostRepositoryChanged fires both on genuine external changes (GitExtensions main window) and
    // as an "echo" of our own RepoChangedNotifier.Notify() (raised by the form's NotifyRepoChanged
    // after a child window like GitFlow/Restore closes). NotifyExternalRepoChanged refreshes on the
    // former but ignores the latter (tree already current) — avoiding a redundant overlay flash.
    private void OnExternalChange(object? sender, GitUIEventArgs e)
    {
        string newDir = e.GitModule?.WorkingDir ?? string.Empty;
        DebugLog($"PostRepositoryChanged inst=#{_instanceId} formOpen={_form is { IsDisposed: false }} dir='{newDir}'");
        if (_form is null || _form.IsDisposed) return;
        // The "Change Working Directory" dropdown switches the active repo through this event (not
        // PostBrowseInitialize), so pass the new working dir along: the form adopts it before
        // refreshing, otherwise the tree would reload the previous repo and the switch wouldn't show.
        _form.InvokeIfRequired(() => _form.NotifyExternalRepoChanged(newDir));
    }

    // ── Diagnostic logging (temporary) ──────────────────────────────────────────
    // Appends one timestamped line per plugin lifecycle/event to a log file, so the exact sequence
    // fired on a "Change Working Directory" switch can be confirmed. Best-effort; never throws.
    private static int _instanceCounter;
    private readonly int _instanceId = System.Threading.Interlocked.Increment(ref _instanceCounter);

    private static readonly string DebugLogPath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GitExtensions", "ZimerfeldTree.debug.log");

    internal static void DebugLog(string message)
    {
        try
        {
            System.IO.File.AppendAllText(DebugLogPath,
                $"{DateTime.Now:HH:mm:ss.fff}  {message}{Environment.NewLine}");
        }
        catch { /* logging must never break the plugin */ }
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
