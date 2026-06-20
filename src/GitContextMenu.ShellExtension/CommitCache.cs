using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using GitContextMenu.Core.Detection;
using GitContextMenu.Core.Git;

namespace GitContextMenu.ShellExtension;

internal static class CommitCache
{
    private static readonly ConcurrentDictionary<string, CacheEntry> Cache = new();
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

    private sealed class CacheEntry
    {
        public bool HasUnpushed { get; }
        public DateTime Timestamp { get; }

        public CacheEntry(bool hasUnpushed)
        {
            HasUnpushed = hasUnpushed;
            Timestamp = DateTime.UtcNow;
        }

        public bool IsExpired => DateTime.UtcNow - Timestamp > Ttl;
    }

    public static bool HasUnpushed(string repoPath)
    {
        if (Cache.TryGetValue(repoPath, out var entry) && !entry.IsExpired)
            return entry.HasUnpushed;

        // Refresh in background; return safe default (enabled) until we know
        Task.Run(() => RefreshAsync(repoPath));
        return entry?.HasUnpushed ?? true;
    }

    private static async Task RefreshAsync(string repoPath)
    {
        try
        {
            var service = new GitService(repoPath);
            var hasUnpushed = await service.HasUnpushedCommitsAsync().ConfigureAwait(false);
            Cache[repoPath] = new CacheEntry(hasUnpushed);
        }
        catch
        {
            Cache[repoPath] = new CacheEntry(true);
        }
    }
}
