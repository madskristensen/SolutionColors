using System.IO;
using System.Threading.Tasks;

namespace SolutionColors
{
    public class GitHelper
    {
        public static async Task<string> GetBranchNameAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Solution solution = await VS.Solutions.GetCurrentSolutionAsync();

            if (solution?.FullPath == null)
            {
                return "master";
            }

            string rootDir;

            if (solution?.Name?.EndsWith(".wsp") == true)
            {
                // .wsp is Open Folder and not regular .sln solutions
                rootDir = solution.FullPath;
            }
            else
            {
                rootDir = Path.GetDirectoryName(solution.FullPath);
            }

            DirectoryInfo directoryInfo = new(rootDir);
            while (directoryInfo != null)
            {
                if (directoryInfo != null && Directory.Exists(Path.Combine(directoryInfo.FullName, ".git")))
                {
                    string content = File.ReadAllText(Path.Combine(directoryInfo.FullName, ".git", "HEAD"));
                    return content.Replace("ref: refs/heads/", "").Trim();
                }

                // git worktree support
                if (File.Exists(Path.Combine(directoryInfo.FullName, ".git")))
                {
                    string gitFileContent = File.ReadAllText(Path.Combine(directoryInfo.FullName, ".git"));
                    string worktreeDir = gitFileContent.Replace("gitdir: ", "").Trim();

                    string content = File.ReadAllText(Path.Combine(worktreeDir, "HEAD"));
                    return content.Replace("ref: refs/heads/", "").Trim();
                }

                directoryInfo = System.IO.Directory.GetParent(directoryInfo.FullName);
            }

            //if there ist no GIT repo, we always are in main branch
            return "master";
        }
    }
}
