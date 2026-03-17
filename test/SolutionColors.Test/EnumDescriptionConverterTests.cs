using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace SolutionColors.Test;

[TestClass]
public class EnumDescriptionConverterTests
{
    private enum SampleEnum
    {
        [System.ComponentModel.DescriptionAttribute("Friendly value")]
        Friendly,
        Plain
    }

    [TestMethod]
    public void ConvertTo_WithDescription_ReturnsDescriptionText()
    {
        TypeConverter converter = CreateEnumDescriptionConverter(typeof(SampleEnum));

        object result = converter.ConvertTo(context: null, culture: CultureInfo.InvariantCulture, value: SampleEnum.Friendly, destinationType: typeof(string));

        Assert.AreEqual("Friendly value", result);
    }

    [TestMethod]
    public void ConvertTo_WithoutDescription_ReturnsEnumName()
    {
        TypeConverter converter = CreateEnumDescriptionConverter(typeof(SampleEnum));

        object result = converter.ConvertTo(context: null, culture: CultureInfo.InvariantCulture, value: SampleEnum.Plain, destinationType: typeof(string));

        Assert.AreEqual(nameof(SampleEnum.Plain), result);
    }

    [TestMethod]
    public void ConvertFrom_WithDescription_ReturnsEnumValue()
    {
        TypeConverter converter = CreateEnumDescriptionConverter(typeof(SampleEnum));

        object result = converter.ConvertFrom(context: null, culture: CultureInfo.InvariantCulture, value: "Friendly value");

        Assert.AreEqual(SampleEnum.Friendly, result);
    }

    [TestMethod]
    public void ConvertFrom_WithEnumName_ReturnsEnumValue()
    {
        TypeConverter converter = CreateEnumDescriptionConverter(typeof(SampleEnum));

        object result = converter.ConvertFrom(context: null, culture: CultureInfo.InvariantCulture, value: nameof(SampleEnum.Plain));

        Assert.AreEqual(SampleEnum.Plain, result);
    }

    private static TypeConverter CreateEnumDescriptionConverter(Type enumType)
    {
        Type converterType = typeof(General).Assembly.GetType("SolutionColors.Options.EnumDescriptionConverter", throwOnError: true);
        object converter = Activator.CreateInstance(
            converterType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: [enumType],
            culture: CultureInfo.InvariantCulture);

        Assert.IsNotNull(converter);
        return (TypeConverter)converter;
    }
}
