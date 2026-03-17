namespace SolutionColors.Test;

[TestClass]
public class BorderSettingsTests
{
    [TestMethod]
    public void BorderSettings_Constructor_InitializesBorderDetails()
    {
        SolutionColors.Options.BorderSettings settings = new SolutionColors.Options.BorderSettings();

        Assert.IsNotNull(settings.BorderDetails);
    }

    [TestMethod]
    public void BorderDetails_Constructor_SetsExpectedDefaults()
    {
        SolutionColors.Options.BorderDetails details = new SolutionColors.Options.BorderDetails();

        Assert.AreEqual(BorderLocation.Bottom, details.Locations);
        Assert.AreEqual(3, details.WidthBottom);
        Assert.AreEqual(3, details.WidthLeft);
        Assert.AreEqual(3, details.WidthRight);
        Assert.AreEqual(3, details.WidthTop);
    }
}
