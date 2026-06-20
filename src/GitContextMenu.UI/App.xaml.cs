using System;
using System.Collections.Generic;
using System.Windows;
using GitContextMenu.Core.Git;
using GitContextMenu.UI.ViewModels;
using GitContextMenu.UI.Views;

namespace GitContextMenu.UI;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var args = ParseArgs(e.Args);

        if (!args.TryGetValue("--operation", out var operation) ||
            !args.TryGetValue("--repo", out var repoPath))
        {
            MessageBox.Show(
                "Usage: GitContextMenu.UI.exe --operation <op> --repo <path>",
                "Git Context Menu",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        try
        {
            var window = BuildWindow(operation, repoPath);
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to open window: {ex.Message}",
                "Git Context Menu",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private static Window BuildWindow(string operation, string repoPath)
    {
        var git = new GitService(repoPath);

        return operation switch
        {
            "status" => new ShellWindow(new StatusViewModel(git)),
            "commit" => new ShellWindow(new CommitViewModel(git)),
            "branch" => new ShellWindow(new BranchViewModel(git)),
            "fetch"  => new ShellWindow(new OperationViewModel(git, "Fetch",  "fetch --all --prune")),
            "pull"   => new ShellWindow(new OperationViewModel(git, "Pull",   "pull")),
            "push"   => new ShellWindow(new OperationViewModel(git, "Push",   "push")),
            _        => throw new ArgumentException($"Unknown operation: '{operation}'"),
        };
    }

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < args.Length - 1; i += 2)
        {
            if (args[i].StartsWith("--"))
                result[args[i]] = args[i + 1];
        }
        return result;
    }
}
