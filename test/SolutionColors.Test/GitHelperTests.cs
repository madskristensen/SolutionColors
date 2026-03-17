using System.Reflection;

namespace SolutionColors.Test;

[TestClass]
public class GitHelperTests
{
    [TestMethod]
    public void GetBranchFromFileSystem_WithNestedStandardRepository_ReturnsBranchName()
    {
        string tempDirectory = CreateTempDirectory();

        try
        {
            string repositoryDirectory = Path.Combine(tempDirectory, "repo");
            string nestedDirectory = Path.Combine(repositoryDirectory, "src", "project");
            string gitDirectory = Path.Combine(repositoryDirectory, ".git");

            Directory.CreateDirectory(gitDirectory);
            Directory.CreateDirectory(nestedDirectory);
            File.WriteAllText(Path.Combine(gitDirectory, "HEAD"), "ref: refs/heads/feature/test");

            string branch = InvokeGetBranchFromFileSystem(nestedDirectory);

            Assert.AreEqual("feature/test", branch);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void GetBranchFromFileSystem_WithGitWorktreeFile_ReturnsBranchName()
    {
        string tempDirectory = CreateTempDirectory();

        try
        {
            string repositoryDirectory = Path.Combine(tempDirectory, "repo");
            string worktreeDirectory = Path.Combine(tempDirectory, "worktrees", "repo");

            Directory.CreateDirectory(repositoryDirectory);
            Directory.CreateDirectory(worktreeDirectory);
            File.WriteAllText(Path.Combine(repositoryDirectory, ".git"), $"gitdir: {worktreeDirectory}");
            File.WriteAllText(Path.Combine(worktreeDirectory, "HEAD"), "ref: refs/heads/worktree-branch");

            string branch = InvokeGetBranchFromFileSystem(repositoryDirectory);

            Assert.AreEqual("worktree-branch", branch);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void GetBranchFromFileSystem_WithoutRepository_ReturnsDefaultBranch()
    {
        string tempDirectory = CreateTempDirectory();

        try
        {
            string branch = InvokeGetBranchFromFileSystem(tempDirectory);

            Assert.AreEqual(GitHelper.DefaultBranch, branch);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    private static string InvokeGetBranchFromFileSystem(string rootDirectory)
    {
        MethodInfo method = typeof(GitHelper).GetMethod("GetBranchFromFileSystem", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.IsNotNull(method);

        object branch = method.Invoke(obj: null, parameters: [rootDirectory]);

        Assert.IsNotNull(branch);
        return (string)branch;
    }

    private static string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), "SolutionColors.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
