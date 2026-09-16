namespace SolutionColors.Test;

[TestClass]
public class GitHelperTests
{
    [TestMethod]
    public void GetBranchFromFileSystem_WithNestedStandardRepository_ReturnsBranchName()
    {
        RunInTempDirectory(tempDirectory =>
        {
            string repositoryDirectory = Path.Combine(tempDirectory, "repo");
            string nestedDirectory = Path.Combine(repositoryDirectory, "src", "project");
            string gitDirectory = Path.Combine(repositoryDirectory, ".git");
            Directory.CreateDirectory(gitDirectory);
            Directory.CreateDirectory(nestedDirectory);
            File.WriteAllText(Path.Combine(gitDirectory, "HEAD"), "ref: refs/heads/feature/test");

            Assert.AreEqual("feature/test", GitHelper.GetBranchFromFileSystem(nestedDirectory));
        });
    }

    [TestMethod]
    public void GetBranchFromFileSystem_WithAbsoluteGitWorktreeFile_ReturnsBranchName()
    {
        RunInTempDirectory(tempDirectory =>
        {
            string repositoryDirectory = Path.Combine(tempDirectory, "repo");
            string worktreeDirectory = Path.Combine(tempDirectory, "worktrees", "repo");
            Directory.CreateDirectory(repositoryDirectory);
            Directory.CreateDirectory(worktreeDirectory);
            File.WriteAllText(Path.Combine(repositoryDirectory, ".git"), $"gitdir: {worktreeDirectory}");
            File.WriteAllText(Path.Combine(worktreeDirectory, "HEAD"), "ref: refs/heads/worktree-branch");

            Assert.AreEqual("worktree-branch", GitHelper.GetBranchFromFileSystem(repositoryDirectory));
        });
    }

    [TestMethod]
    public void GetBranchFromFileSystem_WithRelativeGitWorktreeFile_ReturnsBranchName()
    {
        RunInTempDirectory(tempDirectory =>
        {
            string repositoryDirectory = Path.Combine(tempDirectory, "repo");
            string worktreeDirectory = Path.Combine(tempDirectory, "metadata");
            Directory.CreateDirectory(repositoryDirectory);
            Directory.CreateDirectory(worktreeDirectory);
            File.WriteAllText(Path.Combine(repositoryDirectory, ".git"), "gitdir:\t..\\metadata\r\n");
            File.WriteAllText(Path.Combine(worktreeDirectory, "HEAD"), "ref: refs/heads/relative-worktree\r\n");

            Assert.AreEqual("relative-worktree", GitHelper.GetBranchFromFileSystem(repositoryDirectory));
        });
    }

    [TestMethod]
    public void GetBranchFromFileSystem_WithDetachedHead_ReturnsCommit()
    {
        RunInTempDirectory(tempDirectory =>
        {
            string gitDirectory = Path.Combine(tempDirectory, ".git");
            Directory.CreateDirectory(gitDirectory);
            File.WriteAllText(Path.Combine(gitDirectory, "HEAD"), "0123456789abcdef\r\n");

            Assert.AreEqual("0123456789abcdef", GitHelper.GetBranchFromFileSystem(tempDirectory));
        });
    }

    [DataTestMethod]
    [DataRow(false, null)]
    [DataRow(true, "")]
    public void GetBranchFromFileSystem_WithMissingOrEmptyHead_ReturnsDefaultBranch(bool createHead, string content)
    {
        RunInTempDirectory(tempDirectory =>
        {
            string gitDirectory = Path.Combine(tempDirectory, ".git");
            Directory.CreateDirectory(gitDirectory);
            if (createHead)
            {
                File.WriteAllText(Path.Combine(gitDirectory, "HEAD"), content);
            }

            Assert.AreEqual(GitHelper.DefaultBranch, GitHelper.GetBranchFromFileSystem(tempDirectory));
        });
    }

    [TestMethod]
    public void GetBranchFromFileSystem_WithMalformedGitFile_ReturnsDefaultBranch()
    {
        RunInTempDirectory(tempDirectory =>
        {
            File.WriteAllText(Path.Combine(tempDirectory, ".git"), "not a git directory");

            Assert.AreEqual(GitHelper.DefaultBranch, GitHelper.GetBranchFromFileSystem(tempDirectory));
        });
    }

    [TestMethod]
    public void GetBranchFromFileSystem_WithoutRepository_ReturnsDefaultBranch()
    {
        RunInTempDirectory(tempDirectory =>
            Assert.AreEqual(GitHelper.DefaultBranch, GitHelper.GetBranchFromFileSystem(tempDirectory)));
    }

    private static void RunInTempDirectory(Action<string> test)
    {
        string path = Path.Combine(Path.GetTempPath(), "SolutionColors.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);

        try
        {
            test(path);
        }
        finally
        {
            Directory.Delete(path, recursive: true);
        }
    }
}
