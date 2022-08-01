using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SolutionColors
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class GeneralOptions : BaseOptionPage<General> { }
    }

    public class General : BaseOptionModel<General>, IRatingConfig
    {
        [Category("Border")]
        [DisplayName("Width")]
        [Description("The size of the border in pixels. Default: 3")]
        [DefaultValue(3)]
        public int Width { get; set; } = 3;

        [Browsable(false)]
        public int RatingRequests { get; set; }

        //[Category("Border")]
        //[DisplayName("Location (requires restart)")]
        //[Description("The location of the border in the main document window. Default: Left")]
        //[TypeConverter(typeof(EnumConverter))]
        //[DefaultValue(BorderLocation.Left)]
        //public BorderLocation Location { get; set; } = BorderLocation.Left;

        //public enum BorderLocation
        //{
        //    Left,
        //    Right,
        //    Bottom
        //}
    }
}
