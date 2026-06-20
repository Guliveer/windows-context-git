namespace GitContextMenu.Core.Git.Models;

public sealed class GitResult
{
    public int ExitCode { get; }
    public string Stdout { get; }
    public string Stderr { get; }
    public bool Success => ExitCode == 0;

    public GitResult(int exitCode, string stdout, string stderr)
    {
        ExitCode = exitCode;
        Stdout = stdout ?? string.Empty;
        Stderr = stderr ?? string.Empty;
    }
}
