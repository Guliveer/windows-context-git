using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace GitContextMenu.ShellExtension;

internal static class ProcessLauncher
{
    private static string UiExePath =>
        Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
            "GitContextMenu.UI.exe");

    public static void Launch(string operation, string repoPath)
    {
        try
        {
            var exePath = UiExePath;

            if (!File.Exists(exePath))
            {
                MessageBox.Show(
                    $"GitContextMenu.UI.exe nie znaleziono:\n{exePath}",
                    "Git Context Menu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // Cudzysłowy wokół ścieżki z repo — ścieżka może zawierać spacje
            // Wewnętrzne cudzysłowy escapowane przez \"
            var escapedRepo = repoPath.Replace("\"", "\\\"");
            var arguments = $"--operation {operation} --repo \"{escapedRepo}\"";

            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                UseShellExecute = false,
            };

            Process.Start(psi);
        }
        catch (Exception ex)
        {
            try
            {
                EventLog.WriteEntry(
                    "GitContextMenu",
                    $"Failed to launch UI for operation '{operation}': {ex}",
                    EventLogEntryType.Error);
            }
            catch { }
        }
    }
}
