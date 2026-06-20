using System.IO;

namespace GitContextMenu.Core.Detection;

public static class GitRepositoryDetector
{
    public static bool IsGitRepository(string? path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        var dir = GetDirectoryInfo(path!);
        while (dir != null)
        {
            var gitPath = Path.Combine(dir.FullName, ".git");
            if (Directory.Exists(gitPath) || File.Exists(gitPath))
                return true;
            dir = dir.Parent;
        }
        return false;
    }

    public static string? GetRepositoryRoot(string? path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        var dir = GetDirectoryInfo(path!);
        while (dir != null)
        {
            var gitPath = Path.Combine(dir.FullName, ".git");
            if (Directory.Exists(gitPath) || File.Exists(gitPath))
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }

    private static DirectoryInfo? GetDirectoryInfo(string path)
    {
        try
        {
            var attrs = File.GetAttributes(path);
            return attrs.HasFlag(FileAttributes.Directory)
                ? new DirectoryInfo(path)
                : new FileInfo(path).Directory;
        }
        catch
        {
            return null;
        }
    }
}
