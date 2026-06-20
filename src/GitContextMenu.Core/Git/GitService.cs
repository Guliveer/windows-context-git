using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitContextMenu.Core.Git.Models;
using GitContextMenu.Core.Git.Parsers;

namespace GitContextMenu.Core.Git;

public sealed class GitService : IGitService
{
    public string RepoPath { get; }

    public GitService(string repoPath)
    {
        RepoPath = repoPath ?? throw new ArgumentNullException(nameof(repoPath));
    }

    public async Task<GitResult> RunAsync(string arguments, CancellationToken ct = default)
    {
        using var proc = Process.Start(BuildPsi(arguments))
            ?? throw new InvalidOperationException("Failed to start git process.");

        var stdoutTask = proc.StandardOutput.ReadToEndAsync();
        var stderrTask = proc.StandardError.ReadToEndAsync();

        await Task.Run(() => proc.WaitForExit(), ct).ConfigureAwait(false);

        return new GitResult(proc.ExitCode, await stdoutTask, await stderrTask);
    }

    public async IAsyncEnumerable<string> StreamAsync(
        string arguments,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var proc = Process.Start(BuildPsi(arguments))
            ?? throw new InvalidOperationException("Failed to start git process.");

        string? line;
        while ((line = await proc.StandardOutput.ReadLineAsync().ConfigureAwait(false)) != null)
        {
            if (ct.IsCancellationRequested) yield break;
            yield return line;
        }

        while ((line = await proc.StandardError.ReadLineAsync().ConfigureAwait(false)) != null)
        {
            if (ct.IsCancellationRequested) yield break;
            yield return "[err] " + line;
        }

        await Task.Run(() => proc.WaitForExit(), ct).ConfigureAwait(false);
    }

    public async Task<GitStatus> GetStatusAsync(CancellationToken ct = default)
    {
        var result = await RunAsync("status --porcelain=v2 --branch", ct).ConfigureAwait(false);
        return GitStatusParser.Parse(result.Stdout);
    }

    public async Task<IReadOnlyList<GitBranchInfo>> GetBranchesAsync(CancellationToken ct = default)
    {
        var result = await RunAsync(
            "branch --all --format=%(refname:short)%09%(HEAD)%09%(upstream:short)",
            ct).ConfigureAwait(false);
        return GitBranchParser.Parse(result.Stdout);
    }

    public async Task<bool> HasUnpushedCommitsAsync(CancellationToken ct = default)
    {
        var result = await RunAsync("log @{u}..HEAD --oneline", ct).ConfigureAwait(false);
        return result.Success && !string.IsNullOrWhiteSpace(result.Stdout);
    }

    private ProcessStartInfo BuildPsi(string arguments) => new ProcessStartInfo
    {
        FileName = "git",
        Arguments = arguments,
        WorkingDirectory = RepoPath,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
        StandardOutputEncoding = Encoding.UTF8,
        StandardErrorEncoding = Encoding.UTF8,
        Environment =
        {
            ["LANG"] = "en_US.UTF-8",
            ["LC_ALL"] = "en_US.UTF-8",
            ["GIT_TERMINAL_PROMPT"] = "0",
        },
    };
}
