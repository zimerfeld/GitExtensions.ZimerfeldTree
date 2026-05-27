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

    // ── Constructor ───────────────────────────────────────────────────────────

    public ZimerfeldTreePlugin() : base(false)
    {
        // false = plugin has no configurable settings in the GitExtensions settings dialog
        Name        = "ZimerfeldTree";
        Description = "Visualiza branches hierarquicamente em estrutura de árvore (ZimerfeldTree)";
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

            _form = new BranchHierarchyForm(workDir, notifyChanged);
            _form.FormClosed += (_, _) => _form = null;
        }
        else
        {
            // Update working dir in case the user switched repos since last open
            _form.UpdateWorkingDir(workDir);
        }

        _form.Show(args.OwnerForm);
        _form.BringToFront();

        // Return false: GitExtensions should NOT refresh its own UI (we manage our own state)
        return false;
    }

    /// <summary>Subscribe to GitExtensions events so the tree stays in sync automatically.</summary>
    public override void Register(IGitUICommands commands)
    {
        base.Register(commands);

        commands.PostBrowseInitialize  += OnRepositoryChanged;
        commands.PostCheckoutBranch    += OnBranchChanged;
        commands.PostCheckoutRevision  += OnBranchChanged;
    }

    /// <summary>Unsubscribe from all events.</summary>
    public override void Unregister(IGitUICommands commands)
    {
        commands.PostBrowseInitialize  -= OnRepositoryChanged;
        commands.PostCheckoutBranch    -= OnBranchChanged;
        commands.PostCheckoutRevision  -= OnBranchChanged;

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
