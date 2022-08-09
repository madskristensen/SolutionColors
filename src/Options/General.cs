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
        [Category("Features")]
        [DisplayName("Show border")]
        [Description("Determines if the border should be shown. Turn it off if you only want the task bar colorization. Default: true")]
        [DefaultValue(true)]
        public bool ShowBorder { get; set; } = true;

        [Category("Features")]
        [DisplayName("Show taskbar icon")]
        [Description("Determines if the color icon should be shown in the taskbar. Turn it off if you only want colorization inside VS. Default: true")]
        [DefaultValue(true)]
        public bool ShowTaskBarIcon { get; set; } = true;


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
        [DisplayName("Location")]
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
