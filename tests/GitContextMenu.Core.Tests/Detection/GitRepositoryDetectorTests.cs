using System.IO;
using GitContextMenu.Core.Detection;
using Xunit;

namespace GitContextMenu.Core.Tests.Detection;

public class GitRepositoryDetectorTests
{
    [Fact]
    public void IsGitRepository_ReturnsFalse_ForNull()
    {
        Assert.False(GitRepositoryDetector.IsGitRepository(null));
    }

    [Fact]
    public void IsGitRepository_ReturnsFalse_ForNonExistentPath()
    {
        Assert.False(GitRepositoryDetector.IsGitRepository(@"C:\NoSuchDirectory_XYZ_12345"));
    }

    [Fact]
    public void IsGitRepository_ReturnsTrue_ForDirectoryWithDotGit()
    {
        var tmp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tmp);
        Directory.CreateDirectory(Path.Combine(tmp, ".git"));
        try
        {
            Assert.True(GitRepositoryDetector.IsGitRepository(tmp));
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }

    [Fact]
    public void IsGitRepository_ReturnsTrue_ForSubdirectoryOfRepo()
    {
        var tmp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var sub = Path.Combine(tmp, "src", "deep");
        Directory.CreateDirectory(sub);
        Directory.CreateDirectory(Path.Combine(tmp, ".git"));
        try
        {
            Assert.True(GitRepositoryDetector.IsGitRepository(sub));
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }

    [Fact]
    public void GetRepositoryRoot_ReturnsRoot_ForSubdirectory()
    {
        var tmp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var sub = Path.Combine(tmp, "src");
        Directory.CreateDirectory(sub);
        Directory.CreateDirectory(Path.Combine(tmp, ".git"));
        try
        {
            var root = GitRepositoryDetector.GetRepositoryRoot(sub);
            Assert.Equal(tmp, root);
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }

    [Fact]
    public void GetRepositoryRoot_ReturnsNull_WhenNoGit()
    {
        var tmp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tmp);
        try
        {
            Assert.Null(GitRepositoryDetector.GetRepositoryRoot(tmp));
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }
}
