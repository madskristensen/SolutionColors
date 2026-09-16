using System.Windows.Media;

namespace SolutionColors.Test;

[TestClass]
[DoNotParallelize]
public class ColorCacheTests
{
    [TestInitialize]
    public void Initialize()
    {
        ColorCache.Reset();
    }

    [TestMethod]
    public void TryParseColor_WithKnownColorName_ReturnsTrue()
    {
        bool success = ColorCache.TryParseColor("Tomato", out Color color);

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

        Assert.AreEqual("OrangeRed", ColorCache.GetColorCode("Pumpkin"));
        Assert.AreEqual(0, ColorCache.GetIndex("Pumpkin"));
    }

    [TestMethod]
    public void AddColor_WithCustomName_StoresNameAsValue()
    {
        const string colorName = "#FF123456";

        ColorCache.AddColor(colorName);

        Assert.AreEqual(colorName, ColorCache.GetColorCode(colorName));
        Assert.AreEqual(0, ColorCache.GetIndex(colorName));
    }

    [TestMethod]
    public void GetColorCode_WithUnknownName_ReturnsNull()
    {
        Assert.IsNull(ColorCache.GetColorCode("definitely-not-a-known-color"));
        Assert.AreEqual(-1, ColorCache.GetIndex("missing-color-entry"));
    }

    [TestMethod]
    public void AddColor_WhenCalledTwice_DoesNotAddDuplicate()
    {
        ColorCache.AddColor("Lavender");
        ColorCache.AddColor("Lavender");

        Assert.AreEqual("MediumPurple", ColorCache.GetColorCode("Lavender"));
        Assert.AreEqual(0, ColorCache.GetIndex("Lavender"));
        Assert.AreEqual(Colors.Gray.ToString(), ColorCache.GetColor("C:\\temp\\solution.sln"));
    }

    [TestMethod]
    public void GetColor_WithKnownPalette_ReturnsDeterministicPaletteColor()
    {
        ConfigureKnownPalette();

        string resolvedColor = ColorCache.GetColor("C:\\Source\\Solution.sln");

        Assert.AreEqual("MediumAquamarine", resolvedColor);
    }

    [TestMethod]
    public void GetColor_WithEquivalentWindowsPaths_ReturnsSameColor()
    {
        ConfigureKnownPalette();

        string firstColor = ColorCache.GetColor("C:\\Source\\Solution.sln");
        string secondColor = ColorCache.GetColor("c:/source/solution.sln");

        Assert.AreEqual(firstColor, secondColor);
    }

    [TestMethod]
    public void GetColor_WithRepresentativePaths_UsesAvailablePalette()
    {
        ConfigureKnownPalette();
        string[] paths =
        [
            "C:\\Source\\One.sln",
            "C:\\Source\\Two.sln",
            "C:\\Source\\Three.sln",
            "D:\\Projects\\Four.sln",
            "C:\\B.sln"
        ];

        string[] colors = paths.Select(ColorCache.GetColor).Distinct().ToArray();

        CollectionAssert.AreEquivalent(
            new[] { "OrangeRed", "MediumAquamarine" },
            colors);
    }

    [TestMethod]
    public void GetStableHash_WithKnownPath_ReturnsExpectedHash()
    {
        Assert.AreEqual(51422297, ColorCache.GetStableHash("C:\\Source\\Solution.sln"));
    }

    [TestMethod]
    public void GetStableHash_WithEquivalentWindowsPaths_ReturnsSameHash()
    {
        int firstHash = ColorCache.GetStableHash("C:\\Source\\Solution.sln");
        int secondHash = ColorCache.GetStableHash("c:/source/solution.sln");

        Assert.AreEqual(firstHash, secondHash);
    }

    private static void ConfigureKnownPalette()
    {
        ColorCache.AddColor("Pumpkin");
        ColorCache.AddColor("Mint");
        ColorCache.AddColor("None");
    }
}
