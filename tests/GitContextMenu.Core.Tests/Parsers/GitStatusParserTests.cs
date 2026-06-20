using GitContextMenu.Core.Git.Models;
using GitContextMenu.Core.Git.Parsers;
using Xunit;

namespace GitContextMenu.Core.Tests.Parsers;

public class GitStatusParserTests
{
    private const string SampleOutput = """
        # branch.oid abc123
        # branch.head main
        # branch.upstream origin/main
        # branch.ab +2 -1
        1 M. N... 100644 100644 100644 aaa bbb src/foo.cs
        1 .M N... 100644 100644 100644 aaa bbb src/bar.cs
        1 A. N... 000000 100644 100644 000 ccc src/new.cs
        1 D. N... 100644 000000 100644 ddd 000 src/deleted.cs
        ? untracked.txt
        """;

    [Fact]
    public void Parse_ExtractsBranchName()
    {
        var status = GitStatusParser.Parse(SampleOutput);
        Assert.Equal("main", status.Branch);
    }

    [Fact]
    public void Parse_ExtractsAheadBehind()
    {
        var status = GitStatusParser.Parse(SampleOutput);
        Assert.Equal(2, status.Ahead);
        Assert.Equal(1, status.Behind);
    }

    [Fact]
    public void Parse_ExtractsStagedFiles()
    {
        var status = GitStatusParser.Parse(SampleOutput);
        Assert.Equal(3, status.Staged.Count); // M., A., D.
        Assert.Contains(status.Staged, f => f.Path == "src/foo.cs" && f.State == FileState.Modified);
        Assert.Contains(status.Staged, f => f.Path == "src/new.cs" && f.State == FileState.Added);
        Assert.Contains(status.Staged, f => f.Path == "src/deleted.cs" && f.State == FileState.Deleted);
    }

    [Fact]
    public void Parse_ExtractsUnstagedFiles()
    {
        var status = GitStatusParser.Parse(SampleOutput);
        Assert.Single(status.Unstaged);
        Assert.Equal("src/bar.cs", status.Unstaged[0].Path);
        Assert.Equal(FileState.Modified, status.Unstaged[0].State);
    }

    [Fact]
    public void Parse_ExtractsUntrackedFiles()
    {
        var status = GitStatusParser.Parse(SampleOutput);
        Assert.Single(status.Untracked);
        Assert.Equal("untracked.txt", status.Untracked[0]);
    }

    [Fact]
    public void Parse_HasUnpushedCommits_WhenAheadPositive()
    {
        var status = GitStatusParser.Parse(SampleOutput);
        Assert.True(status.HasUnpushedCommits);
    }

    [Fact]
    public void Parse_EmptyOutput_ReturnsCleanStatus()
    {
        var status = GitStatusParser.Parse(string.Empty);
        Assert.True(status.IsClean);
        Assert.Null(status.Branch);
        Assert.Equal(0, status.Ahead);
    }

    [Fact]
    public void Parse_DetachedHead()
    {
        var output = "# branch.head (HEAD detached at abc1234)\n";
        var status = GitStatusParser.Parse(output);
        Assert.Equal("(HEAD detached at abc1234)", status.Branch);
    }
}
