using System.Collections.Generic;

namespace GitContextMenu.Core.Git.Models;

public sealed class GitStatus
{
    public string? Branch { get; }
    public int Ahead { get; }
    public int Behind { get; }
    public IReadOnlyList<GitFileStatus> Staged { get; }
    public IReadOnlyList<GitFileStatus> Unstaged { get; }
    public IReadOnlyList<string> Untracked { get; }

    public bool HasUnpushedCommits => Ahead > 0;
    public bool IsClean => Staged.Count == 0 && Unstaged.Count == 0 && Untracked.Count == 0;

    public GitStatus(
        string? branch,
        int ahead,
        int behind,
        IReadOnlyList<GitFileStatus> staged,
        IReadOnlyList<GitFileStatus> unstaged,
        IReadOnlyList<string> untracked)
    {
        Branch = branch;
        Ahead = ahead;
        Behind = behind;
        Staged = staged;
        Unstaged = unstaged;
        Untracked = untracked;
    }
}
