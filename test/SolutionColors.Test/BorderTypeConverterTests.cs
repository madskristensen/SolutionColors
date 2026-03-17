namespace SolutionColors.Test;

[TestClass]
public class BorderTypeConverterTests
{
    [TestMethod]
    public void ConvertTo_StringDestination_ReturnsDisplayText()
    {
        SolutionColors.Options.BorderTypeConverter converter = new SolutionColors.Options.BorderTypeConverter();

        object result = converter.ConvertTo(context: null, culture: null, value: new object(), destinationType: typeof(string));

        Assert.AreEqual("(Border settings)", result);
    }
}
