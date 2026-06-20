using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using GitContextMenu.Core.Git;

namespace GitContextMenu.UI.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    protected readonly IGitService Git;

    [ObservableProperty] private string _title = "Git";
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private Brush _statusBrush = Brushes.Transparent;
    [ObservableProperty] private bool _isBusy;

    protected BaseViewModel(IGitService git)
    {
        Git = git;
    }

    public string RepoPath => Git.RepoPath;

    protected void SetSuccess(string message)
    {
        StatusMessage = message;
        StatusBrush = new SolidColorBrush(Color.FromRgb(0xA6, 0xE3, 0xA1)); // Green
    }

    protected void SetError(string message)
    {
        StatusMessage = message;
        StatusBrush = new SolidColorBrush(Color.FromRgb(0xF3, 0x8B, 0xA8)); // Red
    }

    protected void SetInfo(string message)
    {
        StatusMessage = message;
        StatusBrush = new SolidColorBrush(Color.FromRgb(0xF9, 0xE2, 0xAF)); // Yellow
    }
}
