using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GitContextMenu.Core.Git.Models;

namespace GitContextMenu.Core.Git;

public interface IGitService
{
    string RepoPath { get; }

    Task<GitResult> RunAsync(string arguments, CancellationToken ct = default);
    IAsyncEnumerable<string> StreamAsync(string arguments, CancellationToken ct = default);

    Task<GitStatus> GetStatusAsync(CancellationToken ct = default);
    Task<IReadOnlyList<GitBranchInfo>> GetBranchesAsync(CancellationToken ct = default);
    Task<bool> HasUnpushedCommitsAsync(CancellationToken ct = default);
}
