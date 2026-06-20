namespace GitContextMenu.Core.Git.Models;

public enum FileState
{
    Modified,
    Added,
    Deleted,
    Renamed,
    Copied,
    Untracked,
    Conflict,
    Unknown,
}

public sealed class GitFileStatus
{
    public string Path { get; }
    public string? OldPath { get; }
    public FileState State { get; }

    public GitFileStatus(string path, FileState state, string? oldPath = null)
    {
        Path = path;
        State = state;
        OldPath = oldPath;
    }

    public string StateLabel => State switch
    {
        FileState.Modified  => "M",
        FileState.Added     => "A",
        FileState.Deleted   => "D",
        FileState.Renamed   => "R",
        FileState.Copied    => "C",
        FileState.Untracked => "?",
        FileState.Conflict  => "!",
        _                   => "?",
    };
}
