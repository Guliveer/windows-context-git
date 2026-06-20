using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using GitContextMenu.Core.Git;
using GitContextMenu.Core.Git.Models;

namespace GitContextMenu.UI.ViewModels;

public partial class StatusViewModel : BaseViewModel
{
    public ObservableCollection<GitFileStatus> StagedFiles   { get; } = new();
    public ObservableCollection<GitFileStatus> UnstagedFiles { get; } = new();
    public ObservableCollection<string>        UntrackedFiles { get; } = new();

    public string? Branch   { get; private set; }
    public int     Ahead    { get; private set; }
    public int     Behind   { get; private set; }
    public bool    IsClean  { get; private set; }
    public bool    HasStaged    => StagedFiles.Count > 0;
    public bool    HasUnstaged  => UnstagedFiles.Count > 0;
    public bool    HasUntracked => UntrackedFiles.Count > 0;

    public StatusViewModel(IGitService git) : base(git)
    {
        Title = "Status";
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        IsBusy = true;
        SetInfo("Ładowanie...");
        StagedFiles.Clear();
        UnstagedFiles.Clear();
        UntrackedFiles.Clear();

        GitStatus status;
        try
        {
            status = await Git.GetStatusAsync();
        }
        catch (System.Exception ex)
        {
            SetError($"Błąd: {ex.Message}");
            IsBusy = false;
            return;
        }

        Branch = status.Branch;
        Ahead  = status.Ahead;
        Behind = status.Behind;
        IsClean = status.IsClean;

        foreach (var f in status.Staged)    StagedFiles.Add(f);
        foreach (var f in status.Unstaged)  UnstagedFiles.Add(f);
        foreach (var f in status.Untracked) UntrackedFiles.Add(f);

        OnPropertyChanged(nameof(Branch));
        OnPropertyChanged(nameof(Ahead));
        OnPropertyChanged(nameof(Behind));
        OnPropertyChanged(nameof(IsClean));
        OnPropertyChanged(nameof(HasStaged));
        OnPropertyChanged(nameof(HasUnstaged));
        OnPropertyChanged(nameof(HasUntracked));

        IsBusy = false;

        if (status.IsClean)
            SetSuccess("Working tree clean.");
        else
            SetInfo($"{status.Staged.Count} staged · {status.Unstaged.Count} unstaged · {status.Untracked.Count} untracked");
    }
}
