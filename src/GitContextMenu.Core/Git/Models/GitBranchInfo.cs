namespace GitContextMenu.Core.Git.Models;

public sealed class GitBranchInfo
{
    public string Name { get; }
    public bool IsCurrent { get; }
    public bool IsRemote { get; }
    public string? Upstream { get; }

    public GitBranchInfo(string name, bool isCurrent, bool isRemote, string? upstream = null)
    {
        Name = name;
        IsCurrent = isCurrent;
        IsRemote = isRemote;
        Upstream = upstream;
    }
}
