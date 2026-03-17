namespace SolutionColors.Test;

[TestClass]
public class GeneralDefaultsTests
{
    [TestMethod]
    public void General_Constructor_SetsExpectedDefaults()
    {
        General options = new General();

        Assert.IsFalse(options.AutoMode);
        Assert.AreEqual(SolutionColors.Options.TaskBarOptions.MainWindowOnly, options.ShowTaskBarThumbnails);
        Assert.IsFalse(options.ShowTaskBarOverlay);
        Assert.IsTrue(options.ShowTitleBar);
        Assert.IsFalse(options.SaveInRoot);
        Assert.IsNotNull(options.Borders);
        Assert.AreEqual(Coloration.Unitary, options.Coloration);
        Assert.AreEqual(BaseColor.MasterColor, options.BaseColor);
        Assert.IsFalse(options.UseGradientTaskbar);
        Assert.IsFalse(options.UseGradientTitlebar);
        Assert.AreEqual(Gradient.RadialGradient, options.GradientBorders);
        Assert.AreEqual(0, options.RatingRequests);
    }
}
