namespace SolutionColors.Test;

[TestClass]
public class ColorEntryTests
{
    [TestMethod]
    public void Parse_WithBranchAndColor_ReturnsEntry()
    {
        ColorEntry entry = ColorEntry.Parse("main:Tomato");

        Assert.IsNotNull(entry);
        Assert.AreEqual("main", entry.Branch);
        Assert.AreEqual("Tomato", entry.Color);
    }

    [TestMethod]
    public void Parse_WithLegacyColorFormat_UsesDefaultBranch()
    {
        ColorEntry entry = ColorEntry.Parse("Tomato");

        Assert.IsNotNull(entry);
        Assert.AreEqual(GitHelper.DefaultBranch, entry.Branch);
        Assert.AreEqual("Tomato", entry.Color);
    }

    [TestMethod]
    public void Parse_WithWhitespace_ReturnsNull()
    {
        ColorEntry entry = ColorEntry.Parse("   ");

        Assert.IsNull(entry);
    }

    [TestMethod]
    public void ToString_ReturnsBranchAndColor()
    {
        ColorEntry entry = new ColorEntry { Branch = "main", Color = "Tomato" };

        string serialized = entry.ToString();

        Assert.AreEqual("main:Tomato", serialized);
    }

    [TestMethod]
    public void Parse_WithMultipleSegments_UsesFirstTwoSegments()
    {
        ColorEntry entry = ColorEntry.Parse("feature:Tomato:ignored");

        Assert.IsNotNull(entry);
        Assert.AreEqual("feature", entry.Branch);
        Assert.AreEqual("Tomato", entry.Color);
    }
}
