using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitContextMenu.Core.Git;

namespace GitContextMenu.UI.ViewModels;

public partial class OperationViewModel : BaseViewModel
{
    private readonly string _gitArguments;
    private CancellationTokenSource? _cts;

    [ObservableProperty] private bool _isRunning;

    public ObservableCollection<OutputLine> Lines { get; } = new();

    public OperationViewModel(IGitService git, string operationName, string gitArguments)
        : base(git)
    {
        _gitArguments = gitArguments;
        Title = operationName;
    }

    [RelayCommand]
    private async Task RunAsync()
    {
        _cts = new CancellationTokenSource();
        IsRunning = true;
        IsBusy = true;
        Lines.Clear();
        SetInfo("Running...");

        var success = true;
        await foreach (var line in Git.StreamAsync(_gitArguments, _cts.Token))
        {
            var isError = line.StartsWith("[err]");
            if (isError) success = false;
            Lines.Add(new OutputLine(line, isError));
        }

        IsRunning = false;
        IsBusy = false;

        if (success)
            SetSuccess("Completed successfully.");
        else
            SetError("Completed with errors.");
    }

    [RelayCommand]
    private void Cancel()
    {
        _cts?.Cancel();
        SetError("Cancelled.");
        IsRunning = false;
        IsBusy = false;
    }
}

public sealed class OutputLine
{
    public string Text { get; }
    public bool IsError { get; }

    public OutputLine(string text, bool isError)
    {
        Text = text;
        IsError = isError;
    }
}
