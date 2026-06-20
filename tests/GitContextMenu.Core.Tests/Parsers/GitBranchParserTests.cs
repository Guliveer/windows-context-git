using GitContextMenu.Core.Git.Parsers;
using Xunit;

namespace GitContextMenu.Core.Tests.Parsers;

public class GitBranchParserTests
{
    private const string SampleOutput =
        "main\t*\torigin/main\n" +
        "feature/foo\t \t\n" +
        "remotes/origin/main\t \torigin/main\n" +
        "remotes/origin/feature/foo\t \t\n";

    [Fact]
    public void Parse_FindsCurrentBranch()
    {
        var branches = GitBranchParser.Parse(SampleOutput);
        var current = branches[0];
        Assert.Equal("main", current.Name);
        Assert.True(current.IsCurrent);
    }

    [Fact]
    public void Parse_FindsUpstream()
    {
        var branches = GitBranchParser.Parse(SampleOutput);
        Assert.Equal("origin/main", branches[0].Upstream);
    }

    [Fact]
    public void Parse_NullUpstream_WhenEmpty()
    {
        var branches = GitBranchParser.Parse(SampleOutput);
        Assert.Null(branches[1].Upstream);
    }

    [Fact]
    public void Parse_DetectsRemoteBranches()
    {
        var branches = GitBranchParser.Parse(SampleOutput);
        Assert.False(branches[0].IsRemote);
        Assert.True(branches[2].IsRemote);
    }

    [Fact]
    public void Parse_StripsRemotesPrefix()
    {
        var branches = GitBranchParser.Parse(SampleOutput);
        Assert.Equal("origin/main", branches[2].Name);
    }

    [Fact]
    public void Parse_EmptyOutput_ReturnsEmptyList()
    {
        var branches = GitBranchParser.Parse(string.Empty);
        Assert.Empty(branches);
    }
}
