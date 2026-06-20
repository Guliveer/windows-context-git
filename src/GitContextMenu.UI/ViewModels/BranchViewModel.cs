using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitContextMenu.Core.Git;
using GitContextMenu.Core.Git.Models;

namespace GitContextMenu.UI.ViewModels;

public partial class BranchViewModel : BaseViewModel
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckoutCommand))]
    private GitBranchInfo? _selectedBranch;

    [ObservableProperty] private string _newBranchName = string.Empty;
    [ObservableProperty] private bool _showNewBranchPanel;

    public ObservableCollection<GitBranchInfo> LocalBranches { get; } = new();
    public ObservableCollection<GitBranchInfo> RemoteBranches { get; } = new();

    public BranchViewModel(IGitService git) : base(git)
    {
        Title = "Branch / Checkout";
        _ = LoadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanCheckout))]
    private async Task CheckoutAsync()
    {
        if (SelectedBranch == null) return;
        IsBusy = true;
        SetInfo($"Checking out '{SelectedBranch.Name}'...");

        var result = SelectedBranch.IsRemote
            ? await Git.RunAsync($"checkout --track {SelectedBranch.Name}")
            : await Git.RunAsync($"checkout {SelectedBranch.Name}");

        if (result.Success) SetSuccess($"Switched to '{SelectedBranch.Name}'.");
        else SetError(result.Stderr.Trim());

        IsBusy = false;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task CreateBranchAsync()
    {
        if (string.IsNullOrWhiteSpace(NewBranchName)) return;
        IsBusy = true;
        SetInfo($"Creating branch '{NewBranchName}'...");

        var result = await Git.RunAsync($"checkout -b {NewBranchName}");
        if (result.Success)
        {
            SetSuccess($"Created and switched to '{NewBranchName}'.");
            NewBranchName = string.Empty;
            ShowNewBranchPanel = false;
        }
        else
        {
            SetError(result.Stderr.Trim());
        }

        IsBusy = false;
        await LoadAsync();
    }

    [RelayCommand]
    private void ToggleNewBranchPanel() => ShowNewBranchPanel = !ShowNewBranchPanel;

    [RelayCommand]
    private async Task RefreshAsync() => await LoadAsync();

    private bool CanCheckout() => SelectedBranch != null && !SelectedBranch.IsCurrent;

    private async Task LoadAsync()
    {
        IsBusy = true;
        LocalBranches.Clear();
        RemoteBranches.Clear();

        var branches = await Git.GetBranchesAsync();
        foreach (var b in branches.Where(b => !b.IsRemote)) LocalBranches.Add(b);
        foreach (var b in branches.Where(b => b.IsRemote))  RemoteBranches.Add(b);

        var current = LocalBranches.FirstOrDefault(b => b.IsCurrent);
        SelectedBranch = current;

        IsBusy = false;
        SetInfo(current != null ? $"Current: {current.Name}" : "Detached HEAD");
    }
}
