using System;
using System.Collections.Generic;
using GitContextMenu.Core.Git.Models;

namespace GitContextMenu.Core.Git.Parsers;

public static class GitStatusParser
{
    // Parsuje output "git status --porcelain=v2 --branch"
    public static GitStatus Parse(string output)
    {
        string? branch = null;
        int ahead = 0, behind = 0;
        var staged = new List<GitFileStatus>();
        var unstaged = new List<GitFileStatus>();
        var untracked = new List<string>();

        foreach (var line in output.Split('\n'))
        {
            if (string.IsNullOrEmpty(line)) continue;

            if (line.StartsWith("# branch.head "))
            {
                branch = line.Substring(14).Trim();
                continue;
            }

            if (line.StartsWith("# branch.ab "))
            {
                ParseAheadBehind(line.Substring(12).Trim(), out ahead, out behind);
                continue;
            }

            if (line[0] == '#') continue;

            // Ordinary changed entry: "1 XY sub mH mI mW hH hI path"
            if (line[0] == '1' && line.Length > 4 && line[1] == ' ')
            {
                char x = line[2]; // staged
                char y = line[3]; // unstaged
                var path = ExtractPath(line);

                if (x != '.') staged.Add(new GitFileStatus(path, MapState(x)));
                if (y != '.') unstaged.Add(new GitFileStatus(path, MapState(y)));
                continue;
            }

            // Renamed/copied: "2 XY sub mH mI mW hH hI score path\torigPath"
            if (line[0] == '2' && line.Length > 4 && line[1] == ' ')
            {
                char x = line[2];
                char y = line[3];
                var parts = ExtractPath(line).Split('\t');
                var newPath = parts[0];
                var oldPath = parts.Length > 1 ? parts[1] : null;

                if (x != '.') staged.Add(new GitFileStatus(newPath, MapState(x), oldPath));
                if (y != '.') unstaged.Add(new GitFileStatus(newPath, MapState(y), oldPath));
                continue;
            }

            // Unmerged: "u XY sub mH mI mW mU hH hI hU path"
            if (line[0] == 'u' && line.Length > 4)
            {
                var path = ExtractPath(line);
                staged.Add(new GitFileStatus(path, FileState.Conflict));
                continue;
            }

            // Untracked: "? path"
            if (line[0] == '?' && line.Length > 2 && line[1] == ' ')
            {
                untracked.Add(line.Substring(2));
            }
        }

        return new GitStatus(branch, ahead, behind, staged, unstaged, untracked);
    }

    private static void ParseAheadBehind(string ab, out int ahead, out int behind)
    {
        ahead = 0;
        behind = 0;
        var parts = ab.Split(' ');
        foreach (var p in parts)
        {
            if (p.Length < 2) continue;
            if (p[0] == '+' && int.TryParse(p.Substring(1), out var a)) ahead = a;
            if (p[0] == '-' && int.TryParse(p.Substring(1), out var b)) behind = b;
        }
    }

    // Path is the last whitespace-delimited token in porcelain v2 ordinary entries
    private static string ExtractPath(string line)
    {
        // Fields are space-separated; path starts after 8th space for type-1 entries
        int spaceCount = 0;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == ' ' && ++spaceCount == 8)
                return line.Substring(i + 1);
        }
        return line;
    }

    private static FileState MapState(char code) => code switch
    {
        'M' => FileState.Modified,
        'A' => FileState.Added,
        'D' => FileState.Deleted,
        'R' => FileState.Renamed,
        'C' => FileState.Copied,
        '?' => FileState.Untracked,
        'U' => FileState.Conflict,
        _   => FileState.Unknown,
    };
}
