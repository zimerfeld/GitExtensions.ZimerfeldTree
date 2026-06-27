// BranchNode.cs — Data models for ZimerfeldTree plugin
// Licensed under CC BY-NC-ND 4.0 — Copyright (c) 2026 Zimerfeld

namespace GitExtensions.ZimerfeldTree;

/// <summary>Classifies a branch by its origin.</summary>
public enum BranchType
{
    Local,
    Remote,
    Tag
}

/// <summary>Represents a single branch, remote-tracking branch, or tag.</summary>
public sealed class BranchInfo
{
    /// <summary>Full ref name as returned by git (e.g. "feature/login", "origin/main", "v1.0.0").</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>True when this is the currently checked-out branch.</summary>
    public bool IsCurrent { get; init; }

    public BranchType Type { get; init; }

    /// <summary>For remote branches, the name of the remote (e.g. "origin"). Null for local / tags.</summary>
    public string? RemoteName { get; init; }

    /// <summary>
    /// Display name stripped of the remote prefix.
    /// "origin/feature/login" → "feature/login", "main" → "main".
    /// </summary>
    public string DisplayName =>
        RemoteName is not null && FullName.StartsWith(RemoteName + "/", StringComparison.Ordinal)
            ? FullName[(RemoteName.Length + 1)..]
            : FullName;

    /// <summary>True when a remote upstream is configured for this branch.</summary>
    public bool HasUpstream { get; set; }

    /// <summary>
    /// Commits this branch is ahead of its upstream (commits to push, ↑).
    /// Zero when there is no upstream or no divergence.
    /// </summary>
    public int AheadCount  { get; set; }

    /// <summary>
    /// Commits this branch is behind its upstream (commits to pull, ↓).
    /// Zero when there is no upstream or no divergence.
    /// </summary>
    public int BehindCount { get; set; }
}
