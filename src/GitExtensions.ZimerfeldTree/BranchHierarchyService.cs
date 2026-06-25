// BranchHierarchyService.cs — Git operations for ZimerfeldTree plugin
// MIT License — Copyright (c) 2026 Zimerfeld

using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;

namespace GitExtensions.ZimerfeldTree;

/// <summary>
/// Executes git commands and parses their output to supply branch data
/// to <see cref="BranchHierarchyForm"/>.
/// </summary>
public sealed class BranchHierarchyService
{
    public string WorkingDir { get; set; }

    public BranchHierarchyService(string workingDir)
    {
        WorkingDir = workingDir ?? string.Empty;
    }

    // ── Internal runner ──────────────────────────────────────────────────────

    private string RunGit(string arguments, out int exitCode)
    {
        var psi = new ProcessStartInfo("git", arguments)
        {
            WorkingDirectory = WorkingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8
        };

        using var proc = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start git.");
        string stdout = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();
        exitCode = proc.ExitCode;
        return stdout;
    }

    private (string stdout, string stderr, int code) RunGitFull(string arguments)
    {
        var psi = new ProcessStartInfo("git", arguments)
        {
            WorkingDirectory = WorkingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8
        };

        using var proc = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start git.");
        string stdout = proc.StandardOutput.ReadToEnd();
        string stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit();
        return (stdout, stderr, proc.ExitCode);
    }

    // ── Queries ──────────────────────────────────────────────────────────────

    /// <summary>Returns the name of the currently checked-out branch, or empty string.</summary>
    public string GetCurrentBranch()
    {
        try
        {
            return RunGit("rev-parse --abbrev-ref HEAD", out _).Trim();
        }
        catch { return string.Empty; }
    }

    /// <summary>Returns the number of pending changes (staged, unstaged and untracked) in the working tree.</summary>
    public int GetPendingChangesCount()
    {
        try
        {
            return SplitLines(RunGit("status --porcelain", out _)).Count();
        }
        catch { return 0; }
    }

    /// <summary>Opens the GitExtensions Push dialog for the current working directory.</summary>
    public (bool ok, string err) OpenPushWindow()
    {
        try
        {
            string exe = Process.GetCurrentProcess().MainModule?.FileName ?? "GitExtensions.exe";
            Process.Start(new ProcessStartInfo(exe, "push")
            {
                WorkingDirectory = WorkingDir,
                UseShellExecute  = false
            });
            return (true, string.Empty);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    /// <summary>Opens the GitExtensions Commit window for the current working directory.</summary>
    public (bool ok, string err) OpenCommitWindow()
    {
        try
        {
            // The plugin DLL is hosted by GitExtensions.exe, so the host process points to it.
            string exe = Process.GetCurrentProcess().MainModule?.FileName ?? "GitExtensions.exe";
            var psi = new ProcessStartInfo(exe, "commit")
            {
                WorkingDirectory = WorkingDir,
                UseShellExecute = false
            };
            Process.Start(psi);
            return (true, string.Empty);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    /// <summary>Returns all local branches.</summary>
    public List<BranchInfo> GetLocalBranches()
    {
        var current = GetCurrentBranch();
        var result = new List<BranchInfo>();
        try
        {
            string raw = RunGit("branch --format=%(refname:short)", out _);
            foreach (var line in SplitLines(raw))
            {
                // git emits "(HEAD detached at <ref>)" as a pseudo-entry when HEAD is detached — skip it
                if (line.StartsWith("(")) continue;
                result.Add(new BranchInfo
                {
                    FullName = line,
                    IsCurrent = line == current,
                    Type = BranchType.Local
                });
            }
        }
        catch { /* repo may be empty or not initialized */ }
        return result;
    }

    /// <summary>Returns all remote-tracking branches.</summary>
    public List<BranchInfo> GetRemoteBranches()
    {
        var result = new List<BranchInfo>();
        try
        {
            string raw = RunGit("branch -r --format=%(refname:short)", out _);
            foreach (var line in SplitLines(raw))
            {
                if (line.Contains("->")) continue; // formato sem --format: "origin/HEAD -> origin/main"
                var slash = line.IndexOf('/');
                if (slash < 0) continue; // formato com --format: symref "origin/HEAD" sai como "origin" (sem barra)
                string remote     = line[..slash];
                string branchPart = line[(slash + 1)..];
                if (branchPart.Equals("HEAD", StringComparison.OrdinalIgnoreCase)) continue;
                result.Add(new BranchInfo
                {
                    FullName   = line,
                    Type       = BranchType.Remote,
                    RemoteName = remote
                });
            }
        }
        catch { }
        return result;
    }

    /// <summary>Returns all tags.</summary>
    public List<BranchInfo> GetTags()
    {
        // A tag is "current" only when HEAD is detached exactly at that tag's commit.
        // When HEAD is on a branch (even if the branch tip is tagged), no tag is current —
        // this avoids the newly-created release tag appearing as checked-out after "finish".
        bool headDetached = GetCurrentBranch() == "HEAD";
        string currentTag = headDetached ? GetCurrentTagName() : string.Empty;

        var result = new List<BranchInfo>();
        try
        {
            string raw = RunGit("tag --sort=-version:refname", out _);
            foreach (var line in SplitLines(raw))
            {
                result.Add(new BranchInfo
                {
                    FullName  = line,
                    Type      = BranchType.Tag,
                    IsCurrent = !string.IsNullOrEmpty(currentTag) && line == currentTag
                });
            }
        }
        catch { }
        return result;
    }

    /// <summary>Returns the exact tag name when HEAD is detached at a tagged commit, otherwise empty.</summary>
    private string GetCurrentTagName()
    {
        try
        {
            var (stdout, _, code) = RunGitFull("describe --exact-match --tags HEAD");
            return code == 0 ? stdout.Trim() : string.Empty;
        }
        catch { return string.Empty; }
    }

    // ── Mutations ────────────────────────────────────────────────────────────

    public (bool ok, string error) Checkout(string branchName)
    {
        try
        {
            var (_, err, code) = RunGitFull($"checkout \"{EscapeArg(branchName)}\"");
            return code == 0 ? (true, string.Empty) : (false, err.Trim());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    /// <summary>Creates a local tracking branch from a remote-tracking branch and checks it out.</summary>
    /// <param name="remoteBranch">Remote ref, e.g. "origin/feature/login".</param>
    /// <param name="customLocalName">When non-null, uses this name instead of stripping the remote prefix.</param>
    public (bool ok, string error) CheckoutRemoteAsLocal(string remoteBranch, string? customLocalName = null)
    {
        // remoteBranch = "origin/feature/login"  →  localName = "feature/login"
        string localName = customLocalName ?? (remoteBranch.Contains('/')
            ? remoteBranch[(remoteBranch.IndexOf('/') + 1)..]
            : remoteBranch);
        try
        {
            var (_, err, code) = RunGitFull(
                $"checkout -b \"{EscapeArg(localName)}\" --track \"{EscapeArg(remoteBranch)}\"");
            return code == 0 ? (true, string.Empty) : (false, err.Trim());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    /// <summary>Checks out a ref in detached HEAD mode.</summary>
    public (bool ok, string error) CheckoutDetached(string refName)
    {
        try
        {
            var (_, err, code) = RunGitFull($"checkout --detach \"{EscapeArg(refName)}\"");
            return code == 0 ? (true, string.Empty) : (false, err.Trim());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    /// <summary>
    /// Creates a new local branch from <paramref name="fromRef"/> and immediately checks it out
    /// using a single <c>git checkout -b</c> command.
    /// When <paramref name="fromRef"/> is a local branch, the parent-child relationship is
    /// automatically saved to <c>.git/zimerfeld-basedon.json</c> so the tree nests the new
    /// branch under its origin branch.
    /// </summary>
    public (bool ok, string error) CreateAndCheckoutBranch(string newName, string fromRef)
    {
        try
        {
            var (_, err, code) = RunGitFull(
                $"checkout -b \"{EscapeArg(newName)}\" \"{EscapeArg(fromRef)}\"");
            if (code == 0)
            {
                var localNames = new HashSet<string>(
                    GetLocalBranches().Select(b => b.FullName), StringComparer.Ordinal);
                if (localNames.Contains(fromRef))
                    SaveBasedOnOverride(newName, fromRef);
            }
            return code == 0 ? (true, string.Empty) : (false, err.Trim());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public (bool ok, string error) DeleteBranch(string branchName, bool isRemote = false)
    {
        try
        {
            string args;
            if (isRemote)
            {
                var slash = branchName.IndexOf('/');
                if (slash < 0) return (false, "Formato de branch remota inválido.");
                string remote = branchName[..slash];
                string branch = branchName[(slash + 1)..];
                args = $"push {remote} --delete \"{EscapeArg(branch)}\"";
            }
            else
            {
                args = $"branch -d \"{EscapeArg(branchName)}\"";
            }
            var (_, err, code) = RunGitFull(args);
            return code == 0 ? (true, string.Empty) : (false, err.Trim());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public (bool ok, string error) DeleteBranchForce(string branchName)
    {
        try
        {
            var (_, err, code) = RunGitFull($"branch -D \"{EscapeArg(branchName)}\"");
            return code == 0 ? (true, string.Empty) : (false, err.Trim());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    /// <summary>Deletes a tag locally only (<c>git tag -d</c>). Does not touch any remote.</summary>
    public (bool ok, string error) DeleteLocalTag(string tagName)
    {
        try
        {
            var (_, err, code) = RunGitFull($"tag -d \"{EscapeArg(tagName)}\"");
            return code == 0 ? (true, string.Empty) : (false, err.Trim());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    /// <summary>
    /// Deletes a tag from the default remote (<c>git push &lt;remote&gt; --delete &lt;tag&gt;</c>).
    /// A remote that doesn't carry the tag is treated as success — the goal is already met.
    /// </summary>
    public (bool ok, string error) DeleteRemoteTag(string tagName)
    {
        try
        {
            string remote = GetDefaultRemote();
            if (remote.Length == 0) return (true, string.Empty);

            var (_, rerr, rcode) = RunGitFull($"push {remote} --delete \"{EscapeArg(tagName)}\"");
            if (rcode == 0) return (true, string.Empty);
            if (rerr.Contains("remote ref does not exist", StringComparison.OrdinalIgnoreCase))
                return (true, string.Empty);
            return (false, $"falha ao remover do remoto '{remote}': {rerr.Trim()}");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    /// <summary>
    /// Deletes a local branch from the default remote (<c>git push &lt;remote&gt; --delete &lt;branch&gt;</c>).
    /// A remote that doesn't carry the branch is treated as success.
    /// </summary>
    public (bool ok, string error) DeleteRemoteBranch(string branchName)
    {
        try
        {
            string remote = GetDefaultRemote();
            if (remote.Length == 0) return (true, string.Empty);

            var (_, rerr, rcode) = RunGitFull($"push {remote} --delete \"{EscapeArg(branchName)}\"");
            if (rcode == 0) return (true, string.Empty);
            if (rerr.Contains("remote ref does not exist", StringComparison.OrdinalIgnoreCase))
                return (true, string.Empty);
            return (false, $"falha ao remover do remoto '{remote}': {rerr.Trim()}");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    // ── Ref resolution & restore (used to revert an aborted deletion) ─────────────

    /// <summary>Resolves a local branch to its commit SHA (empty string when it doesn't exist).</summary>
    public string ResolveLocalBranchSha(string name) => RevParse($"refs/heads/{name}");

    /// <summary>Resolves a tag to its commit SHA (empty string when it doesn't exist).</summary>
    public string ResolveTagSha(string name) => RevParse($"refs/tags/{name}");

    /// <summary>Resolves a remote-tracking branch (e.g. "origin/main") to its commit SHA.</summary>
    public string ResolveRemoteBranchSha(string fullName) => RevParse($"refs/remotes/{fullName}");

    private string RevParse(string rev)
    {
        try
        {
            var (o, _, c) = RunGitFull($"rev-parse --verify --quiet \"{EscapeArg(rev)}\"");
            return c == 0 ? o.Trim() : string.Empty;
        }
        catch { return string.Empty; }
    }

    /// <summary>Recreates a local branch at the given SHA (used to undo a deletion).</summary>
    public (bool ok, string error) CreateLocalBranch(string name, string sha)
    {
        try
        {
            var (_, err, code) = RunGitFull($"branch \"{EscapeArg(name)}\" {EscapeArg(sha)}");
            return code == 0 ? (true, string.Empty) : (false, err.Trim());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    /// <summary>Recreates a tag at the given SHA (used to undo a deletion).</summary>
    public (bool ok, string error) CreateTag(string name, string sha)
    {
        try
        {
            var (_, err, code) = RunGitFull($"tag \"{EscapeArg(name)}\" {EscapeArg(sha)}");
            return code == 0 ? (true, string.Empty) : (false, err.Trim());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    /// <summary>Pushes a branch back to a remote at the given SHA (used to undo a remote deletion).</summary>
    public (bool ok, string error) RestoreRemoteBranch(string remote, string branchName, string sha)
    {
        try
        {
            if (string.IsNullOrEmpty(remote)) return (false, "nenhum remoto configurado");
            var (_, err, code) = RunGitFull($"push {remote} {EscapeArg(sha)}:refs/heads/{EscapeArg(branchName)}");
            return code == 0 ? (true, string.Empty) : (false, err.Trim());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    /// <summary>Pushes a tag back to the default remote at the given SHA (used to undo a remote deletion).</summary>
    public (bool ok, string error) RestoreRemoteTag(string tagName, string sha)
    {
        try
        {
            string remote = GetDefaultRemote();
            if (remote.Length == 0) return (true, string.Empty);
            var (_, err, code) = RunGitFull($"push {remote} {EscapeArg(sha)}:refs/tags/{EscapeArg(tagName)}");
            return code == 0 ? (true, string.Empty) : (false, err.Trim());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public (bool ok, string error) RenameBranch(string oldName, string newName)
    {
        try
        {
            var (_, err, code) = RunGitFull(
                $"branch -m \"{EscapeArg(oldName)}\" \"{EscapeArg(newName)}\"");
            return code == 0 ? (true, string.Empty) : (false, err.Trim());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public (bool ok, string error) MergeBranch(string branchName)
    {
        try
        {
            var (_, err, code) = RunGitFull($"merge \"{EscapeArg(branchName)}\"");
            return code == 0 ? (true, string.Empty) : (false, err.Trim());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    /// <summary>Runs <c>git pull --tags</c> for the current branch, fetching all remote tags.</summary>
    public (bool ok, string error) Pull()
    {
        try
        {
            var (_, err, code) = RunGitFull("pull --tags");
            return code == 0 ? (true, string.Empty) : (false, err.Trim());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    /// <summary>
    /// Removes stale remote-tracking refs (<c>refs/remotes/&lt;remote&gt;/*</c>) for branches that no
    /// longer exist on the default remote, so they stop appearing in the tree. Runs
    /// <c>git remote prune &lt;remote&gt;</c>, which contacts the remote — call only from the async
    /// refresh path, never from the synchronous window-open load. Best-effort: a missing remote or a
    /// network failure is swallowed, since pruning is a tree-accuracy nicety, not a hard requirement.
    /// </summary>
    public void PruneRemotes()
    {
        try
        {
            string remote = GetDefaultRemote();
            if (remote.Length == 0) return;
            RunGitFull($"remote prune {remote}");
        }
        catch { }
    }

    /// <summary>
    /// Contacts the default remote to refresh the remote-tracking ref of the current branch only
    /// (<c>git fetch &lt;remote&gt; &lt;branch&gt;</c>), so its ahead/behind counts reflect the real
    /// remote state instead of the last fetch. Network-bound — call only from an async/background
    /// path, never from the synchronous window-open load. Best-effort: a missing upstream, missing
    /// remote, or network failure is swallowed. Returns true when a fetch actually ran.
    /// </summary>
    public bool FetchCurrentBranchUpstream()
    {
        try
        {
            // Resolve the upstream of the current branch (e.g. "origin/develop"). Fails cleanly when
            // HEAD is detached or no upstream is configured — nothing to fetch in those cases.
            string upstream = RunGit("rev-parse --abbrev-ref --symbolic-full-name @{u}", out int uc).Trim();
            if (uc != 0 || upstream.Length == 0) return false;

            int slash = upstream.IndexOf('/');
            if (slash <= 0) return false;
            string remote = upstream[..slash];
            string branch = upstream[(slash + 1)..];

            var (_, _, code) = RunGitFull($"fetch {EscapeArg(remote)} {EscapeArg(branch)}");
            return code == 0;
        }
        catch { return false; }
    }

    public (bool ok, string error) RebaseBranch(string branchName)
    {
        try
        {
            var (_, err, code) = RunGitFull($"rebase \"{EscapeArg(branchName)}\"");
            return code == 0 ? (true, string.Empty) : (false, err.Trim());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    // ── Git Flow ───────────────────────────────────────────────────────────────

    /// <summary>The branch types supported by git flow, in display order.</summary>
    public static readonly string[] GitFlowTypes = ["feature", "bugfix", "release", "hotfix", "support"];

    /// <summary>Runs an arbitrary git command and returns the combined stdout+stderr and exit code.</summary>
    public (string output, int code) RunGitFlow(string arguments)
    {
        try
        {
            var (stdout, stderr, code) = RunGitFull(arguments);
            string combined = stdout.TrimEnd();
            stderr = stderr.TrimEnd();
            if (stderr.Length > 0)
                combined = combined.Length > 0 ? combined + "\n" + stderr : stderr;
            return (combined, code);
        }
        catch (Exception ex) { return (ex.Message, -1); }
    }

    /// <summary>Returns the full symbolic HEAD ref (e.g. "refs/heads/develop").</summary>
    public string GetHeadRef()
    {
        try
        {
            string s = RunGit("rev-parse --symbolic-full-name HEAD", out int code).Trim();
            if (code == 0 && s.Length > 0) return s;
        }
        catch { }
        var cur = GetCurrentBranch();
        return cur.Length > 0 ? "refs/heads/" + cur : string.Empty;
    }

    /// <summary>Returns the configured git flow prefix for a branch type, or the default "type/".</summary>
    public string GetGitFlowPrefix(string type)
    {
        try
        {
            string s = RunGit($"config --get gitflow.prefix.{type}", out int code).Trim();
            if (code == 0 && s.Length > 0) return s;
        }
        catch { }
        return type + "/";
    }

    /// <summary>Returns the names of configured remotes (e.g. "origin").</summary>
    public List<string> GetRemotes()
    {
        var result = new List<string>();
        try
        {
            foreach (var line in SplitLines(RunGit("remote", out _)))
                result.Add(line);
        }
        catch { }
        return result;
    }

    /// <summary>
    /// Returns the actual local branch name configured for a git flow role
    /// (e.g. "main" → reads <c>gitflow.branch.main</c> which is set to "main").
    /// Falls back to the role name itself when the config key is missing.
    /// </summary>
    public string GetGitFlowBranchName(string role)
    {
        try
        {
            string s = RunGit($"config --get gitflow.branch.{role}", out int code).Trim();
            if (code == 0 && s.Length > 0) return s;
        }
        catch { }
        return role;
    }

    /// <summary>
    /// Returns the remote to use for push operations: "origin" when present,
    /// otherwise the first configured remote, or empty string if none.
    /// </summary>
    public string GetDefaultRemote()
    {
        var remotes = GetRemotes();
        if (remotes.Count == 0) return string.Empty;
        foreach (var r in remotes)
            if (string.Equals(r, "origin", StringComparison.Ordinal))
                return r;
        return remotes[0];
    }

    /// <summary>
    /// Returns tracking info for every local branch that has an upstream configured,
    /// including branches that are in sync (both counts = 0).
    /// Uses a single pipe-delimited <c>git for-each-ref</c> call.
    /// Key = short branch name, Value = (hasUpstream=true, ahead, behind).
    /// </summary>
    public Dictionary<string, (bool hasUpstream, int ahead, int behind)> GetBranchTrackingInfo()
    {
        var result = new Dictionary<string, (bool, int, int)>(StringComparer.Ordinal);
        try
        {
            // Pipe-delimited format: branchname|upstream_short|upstream_track
            // Pipe is not valid in git ref names on major hosting platforms, making it
            // a reliable field separator.  The outer quotes keep the --format as one argument.
            string raw = RunGit(
                "for-each-ref \"--format=%(refname:short)|%(upstream:short)|%(upstream:track)\" refs/heads/",
                out int code);
            if (code != 0) return result;

            foreach (var line in SplitLines(raw))
            {
                int p1 = line.IndexOf('|');
                if (p1 < 0) continue;
                int p2 = line.IndexOf('|', p1 + 1);
                if (p2 < 0) continue;

                string branch   = line[..p1];
                string upstream = line[(p1 + 1)..p2];
                string track    = line[(p2 + 1)..];

                if (string.IsNullOrEmpty(upstream)) continue; // no upstream configured

                int ahead  = 0;
                int behind = 0;
                var ma = System.Text.RegularExpressions.Regex.Match(track, @"ahead (\d+)");
                var mb = System.Text.RegularExpressions.Regex.Match(track, @"behind (\d+)");
                if (ma.Success) int.TryParse(ma.Groups[1].Value, out ahead);
                if (mb.Success) int.TryParse(mb.Groups[1].Value, out behind);

                result[branch] = (true, ahead, behind);
            }
        }
        catch { }
        return result;
    }

    /// <summary>Returns local branches matching a git flow prefix, with the prefix stripped off.</summary>
    public List<string> GetGitFlowBranches(string prefix)
    {
        var result = new List<string>();
        foreach (var b in GetLocalBranches())
            if (b.FullName.StartsWith(prefix, StringComparison.Ordinal))
                result.Add(b.FullName[prefix.Length..]);
        return result;
    }

    // ── Ancestry analysis (pure merge-base) ──────────────────────────────────

    /// <summary>
    /// Builds a map of branchName → parentBranchName (null = root).
    /// Uses a single <c>git log --all</c> call to build the full commit graph in memory,
    /// then determines parentage via BFS — O(commits) instead of the previous O(N² × subprocess).
    /// </summary>
    public Dictionary<string, string?> BuildParentMap(List<BranchInfo> branches)
    {
        var tips               = GatherTips();
        var (graph, tipToName) = BuildCommitGraph(tips);
        var result             = new Dictionary<string, string?>(StringComparer.Ordinal);

        foreach (var b in branches)
            result[b.FullName] = FindParentInGraph(b.FullName, tips, tipToName, graph);

        ApplyBasedOnOverrides(result);
        BreakCycles(result);
        return result;
    }

    // Gets all local-branch tip SHAs in a single git call: name → full SHA.
    private Dictionary<string, string> GatherTips()
    {
        var tips = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            string raw = RunGit("branch --format=\"%(refname:short) %(objectname)\"", out _);
            foreach (var line in SplitLines(raw))
            {
                int sp = line.IndexOf(' ');
                if (sp > 0) tips[line[..sp]] = line[(sp + 1)..].Trim();
            }
        }
        catch { }
        return tips;
    }

    // Gets all remote-tracking tip SHAs in a single git call: name → full SHA.
    private Dictionary<string, string> GatherRemoteTips()
    {
        var tips = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            string raw = RunGit("branch -r --format=\"%(refname:short) %(objectname)\"", out _);
            foreach (var line in SplitLines(raw))
            {
                if (line.Contains("->")) continue;
                int sp = line.IndexOf(' ');
                string name = sp > 0 ? line[..sp] : line;
                int sl = name.IndexOf('/');
                if (sl < 0) continue; // symref sem barra (ex: "origin" para origin/HEAD)
                if (name[(sl + 1)..].Equals("HEAD", StringComparison.OrdinalIgnoreCase)) continue;
                if (sp > 0) tips[name] = line[(sp + 1)..].Trim();
            }
        }
        catch { }
        return tips;
    }

    /// <summary>Same as <see cref="BuildParentMap"/> but for remote-tracking branches.</summary>
    public Dictionary<string, string?> BuildRemoteParentMap(List<BranchInfo> branches)
    {
        var tips               = GatherRemoteTips();
        var (graph, tipToName) = BuildCommitGraph(tips);
        var result             = new Dictionary<string, string?>(StringComparer.Ordinal);

        foreach (var b in branches)
            result[b.FullName] = FindParentInGraph(b.FullName, tips, tipToName, graph);

        BreakCycles(result);
        return result;
    }

    /// <summary>
    /// Builds the full commit graph in ONE <c>git log --all</c> subprocess call.
    /// Returns the parent map (commitHash → parentHashes[]) and the reverse-tip lookup
    /// (tipHash → branchName) derived from <paramref name="tips"/>.
    /// </summary>
    private (Dictionary<string, string[]> graph, Dictionary<string, string> tipToName)
        BuildCommitGraph(Dictionary<string, string> tips)
    {
        // Reverse of tips: hash → branch name (last writer wins for shared-tip edge case).
        var tipToName = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var kv in tips)
            tipToName[kv.Value] = kv.Key;

        var graph = new Dictionary<string, string[]>(StringComparer.Ordinal);
        try
        {
            string raw = RunGit("log --all --format=\"%H %P\"", out _);
            foreach (var line in SplitLines(raw))
            {
                var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;
                graph[parts[0]] = parts.Length > 1 ? parts[1..] : [];
            }
        }
        catch { }

        return (graph, tipToName);
    }

    /// <summary>
    /// BFS from <paramref name="branch"/>'s tip upward through the commit graph.
    /// Returns the name of the nearest ancestor branch tip (the "parent" branch),
    /// or null when none is found.  O(commits reachable) — no subprocess calls.
    /// </summary>
    private static string? FindParentInGraph(
        string branch,
        Dictionary<string, string> tips,
        Dictionary<string, string> tipToName,
        Dictionary<string, string[]> graph)
    {
        if (!tips.TryGetValue(branch, out string? startHash)) return null;

        var visited = new HashSet<string>(StringComparer.Ordinal) { startHash };
        var queue   = new Queue<string>();

        if (graph.TryGetValue(startHash, out var initParents))
            foreach (var p in initParents)
                queue.Enqueue(p);

        while (queue.Count > 0)
        {
            string commit = queue.Dequeue();
            if (!visited.Add(commit)) continue;

            // First branch tip found via BFS is the nearest ancestor — git's first-parent
            // ordering ensures the "main" branch is found before merged-in branches.
            if (tipToName.TryGetValue(commit, out string? name) && name != branch)
                return name;

            if (graph.TryGetValue(commit, out var parents))
                foreach (var p in parents)
                    if (!visited.Contains(p))
                        queue.Enqueue(p);
        }

        return null;
    }

    private static void BreakCycles(Dictionary<string, string?> parentMap)
    {
        foreach (var key in parentMap.Keys.ToList())
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            string? cur = key;
            while (cur != null)
            {
                if (!seen.Add(cur)) { parentMap[cur] = null; break; }
                parentMap.TryGetValue(cur, out cur);
            }
        }
    }

    // ── Based-on overrides (manual visual nesting, repo-local) ────────────────
    //
    // The plugin's distinguishing feature is a hierarchical view: a branch created
    // "based on" another branch should appear nested under it, even when git ancestry
    // can't express that (e.g. they share a tip and no commit diverges). These links
    // are pure visualization — stored inside .git so they are per-repo and uncommitted.

    // Resolves the repo-local file that stores child→parent visual links.
    private string? BasedOnOverridePath()
    {
        try
        {
            string gitDir = RunGit("rev-parse --git-dir", out int code).Trim();
            if (code != 0 || gitDir.Length == 0) return null;
            if (!Path.IsPathRooted(gitDir))
                gitDir = Path.GetFullPath(Path.Combine(WorkingDir, gitDir));
            return Path.Combine(gitDir, "zimerfeld-basedon.json");
        }
        catch { return null; }
    }

    /// <summary>
    /// Reads the manual "based-on" links (childBranch → parentBranch) used to nest a
    /// branch under another purely for visualization, independent of git ancestry.
    /// </summary>
    public Dictionary<string, string> LoadBasedOnOverrides()
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            string? path = BasedOnOverridePath();
            if (path == null || !File.Exists(path)) return map;
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                string? parent = prop.Value.GetString();
                if (!string.IsNullOrEmpty(parent)) map[prop.Name] = parent;
            }
        }
        catch { }
        return map;
    }

    /// <summary>
    /// Records that <paramref name="child"/> should appear nested under <paramref name="parent"/>
    /// in the tree. Links whose child branch no longer exists are pruned on write so the
    /// file stays bounded.
    /// </summary>
    public void SaveBasedOnOverride(string child, string parent)
    {
        try
        {
            string? path = BasedOnOverridePath();
            if (path == null || string.IsNullOrEmpty(child) || string.IsNullOrEmpty(parent)) return;

            var map = LoadBasedOnOverrides();
            map[child] = parent;

            var live = new HashSet<string>(GetLocalBranches().Select(b => b.FullName), StringComparer.Ordinal);
            live.Add(child); // freshly created; the tip query above may not see it yet
            foreach (var key in map.Keys.ToList())
                if (!live.Contains(key)) map.Remove(key);

            File.WriteAllText(path, JsonSerializer.Serialize(map));
        }
        catch { }
    }

    /// <summary>
    /// Returns the manual based-on parent recorded for <paramref name="branch"/>, or null when
    /// the branch has no based-on link.
    /// </summary>
    public string? GetBasedOnParent(string branch)
        => LoadBasedOnOverrides().TryGetValue(branch, out var parent) ? parent : null;

    /// <summary>
    /// Cleanup after <paramref name="finished"/> is finished/deleted: drops its own based-on link
    /// and re-points any branches that were based on it to <paramref name="newParent"/> (the branch
    /// it was merged into), so the visual tree stays connected.
    /// </summary>
    public void RebaseBasedOnOnFinish(string finished, string newParent)
    {
        try
        {
            string? path = BasedOnOverridePath();
            if (path == null) return;

            var map = LoadBasedOnOverrides();
            bool changed = map.Remove(finished);
            foreach (var key in map.Keys.ToList())
                if (string.Equals(map[key], finished, StringComparison.Ordinal))
                {
                    if (string.IsNullOrEmpty(newParent)) map.Remove(key);
                    else                                 map[key] = newParent;
                    changed = true;
                }

            if (changed) File.WriteAllText(path, JsonSerializer.Serialize(map));
        }
        catch { }
    }

    // Overlays manual based-on links on top of the computed ancestry map. A link wins
    // over git ancestry — the whole point is to show a relationship history can't express.
    // BreakCycles (called next) guards against a user wiring A→B and B→A.
    private void ApplyBasedOnOverrides(Dictionary<string, string?> map)
    {
        var overrides = LoadBasedOnOverrides();
        if (overrides.Count == 0) return;
        foreach (var kv in overrides)
            if (map.ContainsKey(kv.Key)) // only branches actually present in the tree
                map[kv.Key] = kv.Value;
    }

    /// <summary>
    /// Overlays the manual based-on links onto an already-built parent map (e.g. the rigid
    /// GitFlow map) and breaks any resulting cycles, so based-on relationships are honored even
    /// when the tree isn't using pure git ancestry. Mutates and returns <paramref name="map"/>.
    /// </summary>
    public Dictionary<string, string?> OverlayBasedOn(Dictionary<string, string?> map)
    {
        ApplyBasedOnOverrides(map);
        BreakCycles(map);
        return map;
    }

    // ── Settings reader ──────────────────────────────────────────────────────

    /// <summary>
    /// Reads the list of recently opened repositories from the GitExtensions settings file
    /// at <c>%APPDATA%\GitExtensions\GitExtensions\GitExtensions.settings</c>.
    /// Returns an empty list when the file cannot be parsed or does not exist.
    /// </summary>
    public static List<string> GetRepositoriesFromSettings()
    {
        var result = new List<string>();
        try
        {
            string settingsFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GitExtensions", "GitExtensions", "GitExtensions.settings");

            if (!File.Exists(settingsFile)) return result;

            var doc = XDocument.Load(settingsFile);

            // GitExtensions stores repository history under key "history" as an
            // XML-encoded string:
            //   <item>
            //     <key><string>history</string></key>
            //     <value><string>&lt;RepositoryHistory&gt;&lt;Repositories&gt;
            //       &lt;Repository&gt;&lt;Path&gt;C:\...\&lt;/Path&gt;&lt;/Repository&gt;
            //     &lt;/Repositories&gt;&lt;/RepositoryHistory&gt;</string></value>
            //   </item>

            var historyValue = doc
                .Descendants("item")
                .FirstOrDefault(item =>
                    item.Element("key")?.Element("string")?.Value
                        .Equals("history", StringComparison.OrdinalIgnoreCase) == true)
                ?.Element("value")
                ?.Element("string")
                ?.Value;

            if (!string.IsNullOrWhiteSpace(historyValue))
            {
                var inner = XDocument.Parse(historyValue);
                foreach (var pathEl in inner.Descendants("Path"))
                {
                    var path = pathEl.Value?.Trim();
                    if (!string.IsNullOrEmpty(path))
                        result.Add(path);
                }

                if (result.Count > 0)
                    return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            }

            // Fallback: scan all element values that look like valid git working dirs
            foreach (var el in doc.Descendants())
            {
                var val = el.Value?.Trim();
                if (string.IsNullOrEmpty(val) || val.Length < 3 || val.Length > 260) continue;
                if (val.Contains('\n') || val.Contains('\r')) continue;
                try
                {
                    if (Directory.Exists(val) &&
                        (Directory.Exists(Path.Combine(val, ".git")) ||
                         File.Exists(Path.Combine(val, ".git"))))
                    {
                        result.Add(val);
                    }
                }
                catch { }
            }
        }
        catch { }

        return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static IEnumerable<string> SplitLines(string raw) =>
        raw.Split('\n', StringSplitOptions.RemoveEmptyEntries)
           .Select(l => l.Trim())
           .Where(l => !string.IsNullOrEmpty(l));

    /// <summary>Strips double-quote characters to prevent argument injection.</summary>
    private static string EscapeArg(string arg) => arg.Replace("\"", "");
}
