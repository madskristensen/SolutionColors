using System.Windows;
using System.Windows.Media;

namespace SolutionColors.Test;

[TestClass]
[DoNotParallelize]
public class ColorHelperTests
{
    [DataTestMethod]
    [DataRow(Coloration.Branch, "feature", "", true, false, false)]
    [DataRow(Coloration.Branch, "feature", "", false, false, true)]
    [DataRow(Coloration.Branch, "master", "", false, false, true)]
    [DataRow(Coloration.Branch, "master", "Blue", true, false, false)]
    [DataRow(Coloration.Unitary, "feature", "", true, false, true)]
    [DataRow(Coloration.Unitary, "master", "Green", false, false, false)]
    [DataRow(Coloration.Combined, "feature", "", true, false, true)]
    [DataRow(Coloration.Combined, "feature", "Green", false, false, false)]
    [DataRow(Coloration.Branch, "feature", "", false, true, false)]
    public void ShouldRemoveColorization_ReturnsExpectedResult(
        Coloration coloration,
        string currentBranch,
        string masterColor,
        bool currentBranchHasColor,
        bool autoMode,
        bool expected)
    {
        bool actual = ColorHelper.ShouldRemoveColorization(
            coloration,
            currentBranch,
            masterColor,
            currentBranchHasColor,
            autoMode);

        Assert.AreEqual(expected, actual);
    }

    [DataTestMethod]
    [DataRow(true, null, "color.txt")]
    [DataRow(false, null, "icon.img")]
    [DataRow(true, "", "color.txt")]
    [DataRow(false, "   ", "icon.img")]
    public void GetSettingsFileName_WithoutSolutionName_ReturnsLegacyFileName(
        bool isColor,
        string solutionName,
        string expected)
    {
        Assert.AreEqual(expected, ColorHelper.GetSettingsFileName(isColor, solutionName));
    }

    [DataTestMethod]
    [DataRow(true, "C:\\Source\\Product.slnx", "Product.slnx.color.txt")]
    [DataRow(false, "C:\\Source\\Product.sln", "Product.sln.icon.img")]
    [DataRow(true, "Product.sln", "Product.sln.color.txt")]
    public void GetSettingsFileName_WithSolutionName_ReturnsSolutionSpecificFileName(
        bool isColor,
        string solutionName,
        string expected)
    {
        Assert.AreEqual(expected, ColorHelper.GetSettingsFileName(isColor, solutionName));
    }

    [TestMethod]
    public void GetCustomSettingsDirectory_WithRelativePath_ResolvesFromSolutionRoot()
    {
        string directory = ColorHelper.GetCustomSettingsDirectory(
            "C:\\Workspace\\Projects\\Product",
            "Product.sln",
            "..\\..\\SolutionColors");

        Assert.AreEqual("C:\\Workspace\\SolutionColors", directory);
    }

    [TestMethod]
    public void GetCustomSettingsDirectory_WithAbsolutePath_PreservesAbsoluteLocation()
    {
        string directory = ColorHelper.GetCustomSettingsDirectory(
            "C:\\Workspace\\Product",
            "Product.sln",
            "D:\\Shared\\SolutionColors");

        Assert.AreEqual("D:\\Shared\\SolutionColors", directory);
    }

    [TestMethod]
    public void GetCustomSettingsDirectory_WithTrailingSeparator_ResolvesCorrectly()
    {
        string directory = ColorHelper.GetCustomSettingsDirectory(
            "C:\\Workspace\\Product\\",
            "Product.sln",
            "Settings\\");

        Assert.AreEqual("C:\\Workspace\\Product\\Settings\\", directory);
    }

    [TestMethod]
    public void GetCustomSettingsDirectory_WithWhitespaceOnlyPath_ReturnsSolutionRoot()
    {
        string directory = ColorHelper.GetCustomSettingsDirectory(
            "C:\\Workspace\\Product",
            "Product.sln",
            "   ");

        Assert.AreEqual("C:\\Workspace\\Product", directory);
    }

    [TestMethod]
    public void GetCustomSettingsDirectory_WithTokens_ExpandsSolutionValues()
    {
        string directory = ColorHelper.GetCustomSettingsDirectory(
            "C:\\Workspace\\Product",
            "Product.slnx",
            "$(SolutionDir)\\..\\Settings\\$(SolutionName)");

        Assert.AreEqual("C:\\Workspace\\Settings\\Product", directory);
    }

    [TestMethod]
    public void GetCustomSettingsDirectory_WithEmptySolutionName_ExpandsToEmptyValue()
    {
        string directory = ColorHelper.GetCustomSettingsDirectory(
            "C:\\Workspace\\Product",
            null,
            "$(SolutionDir)\\Settings\\$(SolutionName)");

        Assert.AreEqual("C:\\Workspace\\Product\\Settings\\", directory);
    }

    [TestMethod]
    public void GetCustomSettingsDirectory_WithRootSolutionDirectory_PreservesRootSeparator()
    {
        string directory = ColorHelper.GetCustomSettingsDirectory(
            "C:\\",
            "Product.sln",
            "$(SolutionDir)Settings");

        Assert.AreEqual("C:\\Settings", directory);
    }

    [TestMethod]
    public void GetCustomSettingsDirectory_WithEnvironmentVariableAndToken_ExpandsBoth()
    {
        const string variableName = "SOLUTIONCOLORS_TEST_ROOT";
        string previousValue = Environment.GetEnvironmentVariable(variableName);
        Environment.SetEnvironmentVariable(variableName, "C:\\Shared");

        try
        {
            string directory = ColorHelper.GetCustomSettingsDirectory(
                "C:\\Workspace\\Product",
                "Product.sln",
                $"%{variableName}%\\$(SolutionName)");

            Assert.AreEqual("C:\\Shared\\Product", directory);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variableName, previousValue);
        }
    }

    [TestMethod]
    public void GetCustomSettingsDirectory_WithInvalidPath_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            ColorHelper.GetCustomSettingsDirectory(
                "C:\\Workspace\\Product",
                "Product.sln",
                "Settings\0Invalid"));
    }

    [DataTestMethod]
    [DataRow("C:\\Custom", true, "C:\\Custom")]
    [DataRow("C:\\Custom", false, "C:\\Custom")]
    [DataRow("", true, "C:\\Workspace\\Product")]
    [DataRow(null, true, "C:\\Workspace\\Product")]
    [DataRow("", false, "C:\\Workspace\\Product\\.vs\\Product")]
    public void GetSettingsDirectory_AppliesCustomRootAndDefaultPrecedence(
        string customDirectory,
        bool saveInRoot,
        string expected)
    {
        string directory = ColorHelper.GetSettingsDirectory(
            "C:\\Workspace\\Product",
            "Product.sln",
            customDirectory,
            saveInRoot);

        Assert.AreEqual(expected, directory);
    }

    [DataTestMethod]
    [DataRow(Coloration.Unitary, BaseColor.BranchColor, "#FFFF0000")]
    [DataRow(Coloration.Branch, BaseColor.MasterColor, "#FF0000FF")]
    [DataRow(Coloration.Combined, BaseColor.MasterColor, "#FFFF0000")]
    [DataRow(Coloration.Combined, BaseColor.BranchColor, "#FF0000FF")]
    public void GetTaskbarAndTitlebarBrushes_WithoutGradient_UseExpectedSolidColor(
        Coloration coloration,
        BaseColor baseColor,
        string expectedColor)
    {
        General options = new()
        {
            Coloration = coloration,
            BaseColor = baseColor,
            UseGradientTaskbar = false,
            UseGradientTitlebar = false
        };

        AssertSolidColor(ColorHelper.GetBrushForTaskbar(Colors.Red, Colors.Blue, options), expectedColor);
        AssertSolidColor(ColorHelper.GetBrushForTitlebar(Colors.Red, Colors.Blue, options), expectedColor);
    }

    [TestMethod]
    public void GetTaskbarBrush_WithCombinedGradient_UsesTaskbarGradientGeometry()
    {
        General options = new() { Coloration = Coloration.Combined, UseGradientTaskbar = true };

        LinearGradientBrush brush = (LinearGradientBrush)ColorHelper.GetBrushForTaskbar(Colors.Red, Colors.Blue, options);

        Assert.AreEqual(new Point(0.3, 0), brush.StartPoint);
        Assert.AreEqual(new Point(0.7, 0), brush.EndPoint);
        AssertGradientColors(brush, Colors.Red, Colors.Blue);
    }

    [TestMethod]
    public void GetTitlebarBrush_WithCombinedGradient_UsesTitlebarGradientGeometry()
    {
        General options = new() { Coloration = Coloration.Combined, UseGradientTitlebar = true };

        LinearGradientBrush brush = (LinearGradientBrush)ColorHelper.GetBrushForTitlebar(Colors.Red, Colors.Blue, options);

        Assert.AreEqual(new Point(0.8, 0), brush.StartPoint);
        Assert.AreEqual(new Point(0.9, 0), brush.EndPoint);
        AssertGradientColors(brush, Colors.Red, Colors.Blue);
    }

    [DataTestMethod]
    [DataRow(Coloration.Unitary, "#FFFF0000")]
    [DataRow(Coloration.Branch, "#FF0000FF")]
    public void GetBorderBrush_WithSingleColorMode_UsesExpectedSolidColor(
        Coloration coloration,
        string expectedColor)
    {
        General options = new() { Coloration = coloration };

        Brush brush = ColorHelper.GetBrushForBorder(Colors.Red, Colors.Blue, options, BorderLocation.Bottom);

        AssertSolidColor(brush, expectedColor);
    }

    [TestMethod]
    public void GetBorderBrush_WithRadialGradient_UsesExpectedStops()
    {
        General options = new()
        {
            Coloration = Coloration.Combined,
            GradientBorders = Gradient.RadialGradient
        };

        RadialGradientBrush brush = (RadialGradientBrush)ColorHelper.GetBrushForBorder(
            Colors.Red,
            Colors.Blue,
            options,
            BorderLocation.Bottom);

        Assert.HasCount(3, brush.GradientStops);
        Assert.AreEqual(Colors.Blue, brush.GradientStops[0].Color);
        Assert.AreEqual(0, brush.GradientStops[0].Offset);
        Assert.AreEqual(Colors.Blue, brush.GradientStops[1].Color);
        Assert.AreEqual(0.75, brush.GradientStops[1].Offset);
        Assert.AreEqual(Colors.Red, brush.GradientStops[2].Color);
        Assert.AreEqual(1, brush.GradientStops[2].Offset);
    }

    [DataTestMethod]
    [DataRow(BorderLocation.Bottom, 0.3, 0, 0.7, 0, false)]
    [DataRow(BorderLocation.Left, 0, 0.3, 0, 0.7, true)]
    [DataRow(BorderLocation.Right, 0, 0.3, 0, 0.7, false)]
    [DataRow(BorderLocation.Top, 0.3, 0, 0.7, 0, true)]
    public void GetBorderBrush_WithLinearGradient_UsesLocationGeometryAndColorOrder(
        BorderLocation location,
        double startX,
        double startY,
        double endX,
        double endY,
        bool masterFirst)
    {
        General options = new()
        {
            Coloration = Coloration.Combined,
            GradientBorders = Gradient.LinearGradient
        };

        LinearGradientBrush brush = (LinearGradientBrush)ColorHelper.GetBrushForBorder(
            Colors.Red,
            Colors.Blue,
            options,
            location);

        Assert.AreEqual(new Point(startX, startY), brush.StartPoint);
        Assert.AreEqual(new Point(endX, endY), brush.EndPoint);
        AssertGradientColors(
            brush,
            masterFirst ? Colors.Red : Colors.Blue,
            masterFirst ? Colors.Blue : Colors.Red);
    }

    private static void AssertSolidColor(Brush brush, string expectedColor)
    {
        SolidColorBrush solidBrush = brush as SolidColorBrush;
        Assert.IsNotNull(solidBrush);
        Assert.AreEqual(expectedColor, solidBrush.Color.ToString());
    }

    private static void AssertGradientColors(GradientBrush brush, Color first, Color second)
    {
        Assert.HasCount(2, brush.GradientStops);
        Assert.AreEqual(first, brush.GradientStops[0].Color);
        Assert.AreEqual(second, brush.GradientStops[1].Color);
    }
}
