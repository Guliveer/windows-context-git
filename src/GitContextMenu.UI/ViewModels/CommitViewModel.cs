using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitContextMenu.Core.Git;
using GitContextMenu.Core.Git.Models;

namespace GitContextMenu.UI.ViewModels;

public partial class CommitViewModel : BaseViewModel
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CommitCommand))]
    [NotifyCanExecuteChangedFor(nameof(CommitAndPushCommand))]
    private string _commitMessage = string.Empty;

    public ObservableCollection<StagedFileItem> StagedFiles { get; } = new();
    public ObservableCollection<GitFileStatus> UnstagedFiles { get; } = new();

    public CommitViewModel(IGitService git) : base(git)
    {
        Title = "Commit";
        _ = LoadStatusAsync();
    }

    [RelayCommand]
    private async Task StageAllAsync()
    {
        IsBusy = true;
        await Git.RunAsync("add -A");
        await LoadStatusAsync();
    }

    [RelayCommand]
    private async Task StageFileAsync(GitFileStatus file)
    {
        await Git.RunAsync($"add -- \"{file.Path}\"");
        await LoadStatusAsync();
    }

    [RelayCommand]
    private async Task UnstageFileAsync(StagedFileItem item)
    {
        await Git.RunAsync($"restore --staged -- \"{item.File.Path}\"");
        await LoadStatusAsync();
    }

    [RelayCommand(CanExecute = nameof(CanCommit))]
    private async Task CommitAsync()
    {
        IsBusy = true;
        SetInfo("Committing...");
        var msg = CommitMessage.Replace("\"", "\\\"");
        var result = await Git.RunAsync($"commit -m \"{msg}\"");

        if (result.Success)
        {
            SetSuccess("Committed successfully.");
            CommitMessage = string.Empty;
            await LoadStatusAsync();
        }
        else
        {
            SetError(result.Stderr.Trim());
        }
        IsBusy = false;
    }

    [RelayCommand(CanExecute = nameof(CanCommit))]
    private async Task CommitAndPushAsync()
    {
        await CommitAsync();
        if (StatusMessage.StartsWith("Committed"))
        {
            IsBusy = true;
            SetInfo("Pushing...");
            var result = await Git.RunAsync("push");
            if (result.Success) SetSuccess("Committed and pushed.");
            else SetError(result.Stderr.Trim());
            IsBusy = false;
        }
    }

    private bool CanCommit() =>
        !string.IsNullOrWhiteSpace(CommitMessage) && StagedFiles.Count > 0;

    private async Task LoadStatusAsync()
    {
        IsBusy = true;
        StagedFiles.Clear();
        UnstagedFiles.Clear();

        var status = await Git.GetStatusAsync();
        foreach (var f in status.Staged)   StagedFiles.Add(new StagedFileItem(f));
        foreach (var f in status.Unstaged) UnstagedFiles.Add(f);

        IsBusy = false;
        SetInfo(StagedFiles.Count > 0
            ? $"{StagedFiles.Count} file(s) staged"
            : "No files staged. Use 'Stage All' or click a file.");
    }
}

public sealed class StagedFileItem
{
    public GitFileStatus File { get; }
    public StagedFileItem(GitFileStatus file) => File = file;
}
