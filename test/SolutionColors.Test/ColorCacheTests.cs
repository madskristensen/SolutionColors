namespace SolutionColors.Test;

[TestClass]
public class ColorCacheTests
{
    [TestMethod]
    public void TryParseColor_WithKnownColorName_ReturnsTrue()
    {
        bool success = ColorCache.TryParseColor("Tomato", out System.Windows.Media.Color color);

        Assert.IsTrue(success);
        Assert.AreEqual("#FFFF6347", color.ToString());
    }

    [TestMethod]
    public void TryParseColor_WithInvalidColor_ReturnsFalse()
    {
        bool success = ColorCache.TryParseColor("not-a-color", out _);

        Assert.IsFalse(success);
    }

    [TestMethod]
    public void GetColorCode_WithHexColor_ReturnsInput()
    {
        const string hexColor = "#FF112233";

        string colorCode = ColorCache.GetColorCode(hexColor);

        Assert.AreEqual(hexColor, colorCode);
    }

    [TestMethod]
    public void AddColor_WithTranslatedName_StoresTranslatedValue()
    {
        ColorCache.AddColor("Pumpkin");

        string colorCode = ColorCache.GetColorCode("Pumpkin");

        Assert.AreEqual("OrangeRed", colorCode);
    }

    [TestMethod]
    public void AddColor_WithCustomName_StoresNameAsValue()
    {
        string colorName = "#FF" + Guid.NewGuid().ToString("N").Substring(0, 6);

        ColorCache.AddColor(colorName);

        string colorCode = ColorCache.GetColorCode(colorName);

        Assert.AreEqual(colorName, colorCode);
    }

    [TestMethod]
    public void GetColorCode_WithUnknownName_ReturnsNull()
    {
        string colorCode = ColorCache.GetColorCode("definitely-not-a-known-color");

        Assert.IsNull(colorCode);
    }

    [TestMethod]
    public void GetIndex_WithExistingColor_ReturnsNonNegative()
    {
        string colorName = "#FF" + Guid.NewGuid().ToString("N").Substring(0, 6);
        ColorCache.AddColor(colorName);

        int index = ColorCache.GetIndex(colorName);

        Assert.IsTrue(index >= 0);
    }

    [TestMethod]
    public void GetIndex_WithMissingColor_ReturnsNegativeOne()
    {
        int index = ColorCache.GetIndex("missing-color-entry");

        Assert.AreEqual(-1, index);
    }

    [TestMethod]
    public void AddColor_WhenCalledTwice_KeepsSameMappedValue()
    {
        string colorName = "Lavender";
        ColorCache.AddColor(colorName);
        string firstValue = ColorCache.GetColorCode(colorName);

        ColorCache.AddColor(colorName);
        string secondValue = ColorCache.GetColorCode(colorName);

        Assert.AreEqual(firstValue, secondValue);
    }

    [TestMethod]
    public void GetColor_WithConfiguredMap_ReturnsParsableColor()
    {
        ColorCache.AddColor("Pumpkin");
        ColorCache.AddColor("Mint");

        string resolvedColor = ColorCache.GetColor("C:\\temp\\solution.sln");
        bool success = ColorCache.TryParseColor(resolvedColor, out System.Windows.Media.Color _);

        Assert.IsTrue(success);
    }
}
