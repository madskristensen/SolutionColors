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
        [DisplayName("Auto-mode")]
        [Description("Automatically assign and apply a color to solutions as they open. You can still manually assign colors when needed. Default: false")]
        [DefaultValue(false)]
        public bool AutoMode { get; set; }
        
        [Category("Border")]
        [DisplayName("Width")]
        [Description("The size of the border in pixels. Default: 3")]
        [DefaultValue(3)]
        public int Width { get; set; } = 3;

        [Category("Border")]
        [DisplayName("Location (requires restart)")]
        [Description("The location of the border in the main document window. Default: Bottom")]
        [TypeConverter(typeof(EnumConverter))]
        [DefaultValue(BorderLocation.Bottom)]
        public BorderLocation Location { get; set; } = BorderLocation.Bottom;

        [Browsable(false)]
        public int RatingRequests { get; set; }
    }

    public enum BorderLocation
    {
        Bottom,
        Left,
        Right,
    }
}
