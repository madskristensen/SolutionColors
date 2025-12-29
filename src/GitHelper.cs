using System.IO;
using System.Threading.Tasks;

namespace SolutionColors
{
    public static class GitHelper
    {
        private const string _gitDirectory = ".git";
        private const string _headFile = "HEAD";
        private const string _branchRefPrefix = "ref: refs/heads/";
        private const string _gitDirPrefix = "gitdir: ";
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

        private static string GetBranchFromFileSystem(string rootDir)
        {
            DirectoryInfo directoryInfo = new(rootDir);
            while (directoryInfo != null)
            {
                string gitPath = Path.Combine(directoryInfo.FullName, _gitDirectory);

                // Standard git repository
                if (Directory.Exists(gitPath))
                {
                    string headPath = Path.Combine(gitPath, _headFile);
                    if (File.Exists(headPath))
                    {
                        string content = File.ReadAllText(headPath);
                        return content.Replace(_branchRefPrefix, "").Trim();
                    }
                }

                // Git worktree support
                if (File.Exists(gitPath))
                {
                    string gitFileContent = File.ReadAllText(gitPath);
                    string worktreeDir = gitFileContent.Replace(_gitDirPrefix, "").Trim();

                    string headPath = Path.Combine(worktreeDir, _headFile);
                    if (File.Exists(headPath))
                    {
                        string content = File.ReadAllText(headPath);
                        return content.Replace(_branchRefPrefix, "").Trim();
                    }
                }

                directoryInfo = Directory.GetParent(directoryInfo.FullName);
            }

            // If there is no GIT repo, we always are in default branch
            return DefaultBranch;
        }
    }
}
