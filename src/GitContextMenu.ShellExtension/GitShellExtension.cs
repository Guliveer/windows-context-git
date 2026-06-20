using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GitContextMenu.Core.Detection;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace GitContextMenu.ShellExtension;

[ComVisible(true)]
[COMServerAssociation(AssociationType.Directory)]
[COMServerAssociation(AssociationType.DirectoryBackground)]
[DisplayName("Git Context Menu")]
[Guid("7FA2CE94-8132-426E-9C90-D997F34AB77D")]
public class GitShellExtension : SharpContextMenu
{
    protected override bool CanShowMenu()
    {
        try
        {
            var path = GetTargetPath();
            return path != null && GitRepositoryDetector.IsGitRepository(path);
        }
        catch
        {
            return false;
        }
    }

    protected override ContextMenuStrip CreateMenu()
    {
        try
        {
            var repoPath = GetRepoPath();
            if (repoPath == null) return new ContextMenuStrip();

            var menu = new ContextMenuStrip();
            var gitMenu = new ToolStripMenuItem("Git");

            gitMenu.DropDownItems.Add(MakeItem("Status",              "status", repoPath));
            gitMenu.DropDownItems.Add(new ToolStripSeparator());
            gitMenu.DropDownItems.Add(MakeItem("Fetch",               "fetch",  repoPath));
            gitMenu.DropDownItems.Add(MakeItem("Pull",                "pull",   repoPath));
            var pushItem = MakeItem("Push",                           "push",   repoPath);
            pushItem.Enabled = CommitCache.HasUnpushed(repoPath);
            gitMenu.DropDownItems.Add(pushItem);
            gitMenu.DropDownItems.Add(new ToolStripSeparator());
            gitMenu.DropDownItems.Add(MakeItem("Commit...",           "commit", repoPath));
            gitMenu.DropDownItems.Add(MakeItem("Branch / Checkout...", "branch", repoPath));

            menu.Items.Add(gitMenu);
            return menu;
        }
        catch
        {
            return new ContextMenuStrip();
        }
    }

    private ToolStripMenuItem MakeItem(string label, string operation, string repoPath)
    {
        var item = new ToolStripMenuItem(label);
        item.Click += (_, _) => ProcessLauncher.Launch(operation, repoPath);
        return item;
    }

    private string? GetTargetPath()
    {
        // SelectedItemPaths for right-click on folder; FolderPath for background click
        var paths = SelectedItemPaths;
        if (paths != null)
        {
            foreach (var p in paths)
                return p;
        }
        return FolderPath;
    }

    private string? GetRepoPath()
    {
        var path = GetTargetPath();
        return path == null ? null : GitRepositoryDetector.GetRepositoryRoot(path);
    }
}
