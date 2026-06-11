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

        commands.PostCheckoutBranch     += OnBranchChanged;
        commands.PostCheckoutRevision   += OnBranchChanged;
        commands.PostCommit             += OnPostCommit;   // dedicated: also restores focus

        string regDir = commands.Module?.WorkingDir ?? string.Empty;
        DebugLog($"Register   inst=#{_instanceId} formOpen={_form is { IsDisposed: false }} dir='{regDir}'");

        // NOTE: We deliberately do NOT adopt the host's working dir into an already-open tree window.
        // The GitExtensions "Change Working Directory" dropdown must not drive ZimerfeldTree's repo
        // selection (cboRepo) or its operations once the window is open — cboRepo is the single source
        // of truth there. The host working dir is only used as the PRE-SELECTED cboRepo value when the
        // window is first opened (see Execute → BranchHierarchyForm ctor → LoadRepositories).
        // For the same reason we no longer subscribe to PostBrowseInitialize / PostRepositoryChanged,
        // which fire on host repo switches.
    }

    /// <summary>Unsubscribe from all events.</summary>
    public override void Unregister(IGitUICommands commands)
    {
        DebugLog($"Unregister inst=#{_instanceId} dir='{commands.Module?.WorkingDir ?? string.Empty}'");
        commands.PostCheckoutBranch     -= OnBranchChanged;
        commands.PostCheckoutRevision   -= OnBranchChanged;
        commands.PostCommit             -= OnPostCommit;

        _commands = null;
        base.Unregister(commands);
    }

    // ── Event handlers ────────────────────────────────────────────────────────
    //
    // NOTE: There is intentionally no handler for PostBrowseInitialize / PostRepositoryChanged.
    // Host repo switches (e.g. the "Change Working Directory" dropdown) must not affect an open
    // ZimerfeldTree window — its repo is chosen exclusively through cboRepo. Only PostCheckoutBranch /
    // PostCheckoutRevision / PostCommit are observed, and they merely refresh the CURRENT (cboRepo)
    // repo's tree; they never change which repo is selected.

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
