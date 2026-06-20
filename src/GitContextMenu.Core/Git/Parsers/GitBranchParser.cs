using System.Collections.Generic;
using GitContextMenu.Core.Git.Models;

namespace GitContextMenu.Core.Git.Parsers;

public static class GitBranchParser
{
    // Parsuje output "git branch --all --format=%(refname:short)%09%(HEAD)%09%(upstream:short)"
    public static IReadOnlyList<GitBranchInfo> Parse(string output)
    {
        var branches = new List<GitBranchInfo>();

        foreach (var line in output.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split('\t');
            if (parts.Length < 2) continue;

            var name = parts[0].Trim();
            var isCurrent = parts.Length > 1 && parts[1].Trim() == "*";
            var upstream = parts.Length > 2 ? parts[2].Trim() : null;
            if (string.IsNullOrEmpty(upstream)) upstream = null;

            var isRemote = name.StartsWith("remotes/");
            if (isRemote) name = name.Substring("remotes/".Length);

            branches.Add(new GitBranchInfo(name, isCurrent, isRemote, upstream));
        }

        return branches;
    }
}
