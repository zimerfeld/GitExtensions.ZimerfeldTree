// ZimerfeldTreePlugin.cs — MEF plugin entry point for ZimerfeldTree
// MIT License — Copyright (c) 2026 Zimerfeld

using System.ComponentModel.Composition;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;

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
        Description = "Visualiza branches hierarquicamente em estrutura de árvore (ZimerfeldTree). "
                    + "Diferente do GitFlow clássico — que não prevê feature filha de feature e mantém todas as "
                    + "feature/* irmãs derivando de develop —, o GitFlow do ZimerfeldTree permite uma hierarquia "
                    + "flexível: uma feature/* pode derivar de develop ou de outra feature/* acima dela. Nesse caso "
                    + "o finish feature cascateia as mudanças para a feature/* pai sucessivamente, reaplicando finish "
                    + "feature até chegar em develop.";
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
            // workingDir is the repo selected in the tree window's cboRepo: bind the commands to it via
            // WithWorkingDirectory so the dialog commits against that repo (and its checked-out branch,
            // shown in lblBranch) rather than the GitExtensions host's active repository.
            // Returns null when unavailable (caller falls back to spawning a new process).
            Func<IWin32Window, string, bool?> openCommit = (owner, workingDir) =>
            {
                try
                {
                    var commands = string.IsNullOrEmpty(workingDir)
                        ? _commands
                        : _commands?.WithWorkingDirectory(workingDir);
                    return commands?.StartCommitDialog(owner, string.Empty, false);
                }
                catch { return null; }
            };

            // Open the native push dialog in-process; returns true if push was completed.
            // workingDir is the repo selected in the tree window's cboRepo: bind the commands to it via
            // WithWorkingDirectory so the dialog pushes that repo (and its checked-out branch, shown in
            // lblBranch) rather than the GitExtensions host's active repository — mirroring openCommit.
            Func<IWin32Window, string, bool> openPush = (owner, workingDir) =>
            {
                if (_commands is null) return false;
                try
                {
                    var commands = string.IsNullOrEmpty(workingDir)
                        ? _commands
                        : _commands.WithWorkingDirectory(workingDir);
                    if (commands is null) return false;
                    bool pushCompleted = false;
                    commands.StartPushDialog(owner, pushOnShow: true, forceWithLease: false, pushCompleted: out pushCompleted);
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

        string regDir = commands.Module?.WorkingDir ?? string.Empty;
        DebugLog($"Register   inst=#{_instanceId} formOpen={_form is { IsDisposed: false }} dir='{regDir}'");

        // NOTE: ZimerfeldTree is fully decoupled from the GitExtensions host UI. We subscribe to NO
        // host events, so nothing done in the GitExtensions interface — switching repos via the
        // "Change Working Directory" dropdown, checking out a branch, committing, etc. — affects an
        // open tree window. The window's repository is chosen exclusively through its own cboRepo, and
        // it refreshes only via its own buttons/menus/operations. The host working dir is used only
        // once, as the PRE-SELECTED cboRepo value when the window is opened (see Execute →
        // BranchHierarchyForm ctor → LoadRepositories).
    }

    /// <summary>Unsubscribe from all events.</summary>
    public override void Unregister(IGitUICommands commands)
    {
        DebugLog($"Unregister inst=#{_instanceId} dir='{commands.Module?.WorkingDir ?? string.Empty}'");

        _commands = null;
        base.Unregister(commands);
    }

    // ── Event handlers ────────────────────────────────────────────────────────
    //
    // NOTE: The plugin intentionally subscribes to NO GitExtensions host events. Nothing done in the
    // GitExtensions interface — switching repos via "Change Working Directory", checking out a branch,
    // committing, etc. — affects an open ZimerfeldTree window. The window owns its repository selection
    // (cboRepo) and refreshes only through its own buttons/menus/operations. The host working dir is
    // consulted only once, when the window is opened (Execute → ctor), to pre-select cboRepo.

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
