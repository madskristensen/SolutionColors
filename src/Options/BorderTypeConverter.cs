using System.ComponentModel;
using System.Globalization;

namespace SolutionColors.Options
{
    public class BorderTypeConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return "(Border settings)";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
