namespace SolutionColors.Test;

[TestClass]
public class ColorHelperTests
{
    [TestMethod]
    public void ShouldRemoveColorization_BranchMode_NonMasterBranchWithColor_KeepsColor()
    {
        // Issue #59: a non-master branch with its own color must be colorized
        // even when the master branch has no color.
        bool remove = ColorHelper.ShouldRemoveColorization(
            Coloration.Branch,
            currentBranch: "BranchA",
            masterColor: string.Empty,
            currentBranchHasColor: true,
            autoMode: false);

        Assert.IsFalse(remove);
    }

    [TestMethod]
    public void ShouldRemoveColorization_BranchMode_NonMasterBranchWithoutColor_RemovesColor()
    {
        bool remove = ColorHelper.ShouldRemoveColorization(
            Coloration.Branch,
            currentBranch: "BranchA",
            masterColor: string.Empty,
            currentBranchHasColor: false,
            autoMode: false);

        Assert.IsTrue(remove);
    }

    [TestMethod]
    public void ShouldRemoveColorization_BranchMode_MasterBranchWithoutColor_RemovesColor()
    {
        bool remove = ColorHelper.ShouldRemoveColorization(
            Coloration.Branch,
            currentBranch: GitHelper.DefaultBranch,
            masterColor: string.Empty,
            currentBranchHasColor: false,
            autoMode: false);

        Assert.IsTrue(remove);
    }

    [TestMethod]
    public void ShouldRemoveColorization_BranchMode_MasterBranchWithColor_KeepsColor()
    {
        bool remove = ColorHelper.ShouldRemoveColorization(
            Coloration.Branch,
            currentBranch: GitHelper.DefaultBranch,
            masterColor: "Blue",
            currentBranchHasColor: true,
            autoMode: false);

        Assert.IsFalse(remove);
    }

    [TestMethod]
    public void ShouldRemoveColorization_UnitaryMode_NoMasterColor_RemovesColor()
    {
        bool remove = ColorHelper.ShouldRemoveColorization(
            Coloration.Unitary,
            currentBranch: "BranchA",
            masterColor: string.Empty,
            currentBranchHasColor: true,
            autoMode: false);

        Assert.IsTrue(remove);
    }

    [TestMethod]
    public void ShouldRemoveColorization_UnitaryMode_WithMasterColor_KeepsColor()
    {
        bool remove = ColorHelper.ShouldRemoveColorization(
            Coloration.Unitary,
            currentBranch: GitHelper.DefaultBranch,
            masterColor: "Green",
            currentBranchHasColor: false,
            autoMode: false);

        Assert.IsFalse(remove);
    }

    [TestMethod]
    public void ShouldRemoveColorization_CombinedMode_NoMasterColor_RemovesColor()
    {
        bool remove = ColorHelper.ShouldRemoveColorization(
            Coloration.Combined,
            currentBranch: "BranchA",
            masterColor: string.Empty,
            currentBranchHasColor: true,
            autoMode: false);

        Assert.IsTrue(remove);
    }

    [TestMethod]
    public void ShouldRemoveColorization_AutoMode_AlwaysKeepsColor()
    {
        bool remove = ColorHelper.ShouldRemoveColorization(
            Coloration.Branch,
            currentBranch: "BranchA",
            masterColor: string.Empty,
            currentBranchHasColor: false,
            autoMode: true);

        Assert.IsFalse(remove);
    }
}
