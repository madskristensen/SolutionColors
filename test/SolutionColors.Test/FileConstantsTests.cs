namespace SolutionColors.Test;

[TestClass]
public class FileConstantsTests
{
    [TestMethod]
    public void Constants_HaveExpectedFileNames()
    {
        Assert.AreEqual("color.txt", FileConstants.ColorFileName);
        Assert.AreEqual("icon.img", FileConstants.IconFileName);
        Assert.AreEqual(".vs", FileConstants.VsSettingsFolder);
    }
}
