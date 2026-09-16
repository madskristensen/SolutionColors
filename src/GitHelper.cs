using System.IO;
using System.Threading.Tasks;

namespace SolutionColors
{
    public static class GitHelper
    {
        private const string _gitDirectory = ".git";
        private const string _headFile = "HEAD";
        private const string _branchRefPrefix = "ref: refs/heads/";
        private const string _gitDirPrefix = "gitdir:";
        public const string DefaultBranch = "master";

        public static async Task<string> GetBranchNameAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Solution solution = await VS.Solutions.GetCurrentSolutionAsync();
            string rootDir = solution?.GetRootDirectory();

            if (rootDir == null)
            {
                return DefaultBranch;
            }

            // Do file I/O on background thread
            return await Task.Run(() => GetBranchFromFileSystem(rootDir));
        }

        internal static string GetBranchFromFileSystem(string rootDir)
        {
            DirectoryInfo directoryInfo = new(rootDir);
            while (directoryInfo != null)
            {
                string gitPath = Path.Combine(directoryInfo.FullName, _gitDirectory);

                // Standard git repository
                if (Directory.Exists(gitPath))
                {
                    string headPath = Path.Combine(gitPath, _headFile);
                    string branch = TryReadBranch(headPath);
                    if (branch != null)
                    {
                        return branch;
                    }
                }

                // Git worktree support
                if (File.Exists(gitPath))
                {
                    string gitFileContent = TryReadAllText(gitPath);
                    if (gitFileContent?.Trim().StartsWith(_gitDirPrefix, StringComparison.Ordinal) == true)
                    {
                        string worktreeDir = gitFileContent.Trim().Substring(_gitDirPrefix.Length).Trim();
                        if (!Path.IsPathRooted(worktreeDir))
                        {
                            worktreeDir = Path.GetFullPath(Path.Combine(directoryInfo.FullName, worktreeDir));
                        }

                        string headPath = Path.Combine(worktreeDir, _headFile);
                        string branch = TryReadBranch(headPath);
                        if (branch != null)
                        {
                            return branch;
                        }
                    }
                }

                directoryInfo = Directory.GetParent(directoryInfo.FullName);
            }

            // If there is no GIT repo, we always are in default branch
            return DefaultBranch;
        }

        private static string TryReadBranch(string headPath)
        {
            if (!File.Exists(headPath))
            {
                return null;
            }

            string content = TryReadAllText(headPath);
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            string head = content.Trim();
            return head.StartsWith(_branchRefPrefix, StringComparison.Ordinal)
                ? head.Substring(_branchRefPrefix.Length)
                : head;
        }

        private static string TryReadAllText(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (IOException)
            {
                // File locked or unavailable (e.g. concurrent git operation)
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }
    }
}
