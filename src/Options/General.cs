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
        [Category("General")]
        [DisplayName("Auto-mode")]
        [Description("Automatically assign and apply a color to solutions as they open. You can still manually assign colors when needed. Default: false")]
        [DefaultValue(false)]
        public bool AutoMode { get; set; }
        
        [Category("General")]
        [DisplayName("Show border")]
        [Description("Determines if the border should be shown. Turn it off if you only want the task bar colorization. Default: true")]
        [DefaultValue(true)]
        public bool ShowBorder { get; set; } = true;

        [Category("General")]
        [DisplayName("Show on taskbar thumbnails")]
        [Description("Determines if the color icon should be shown in the taskbar thumbnails. Default: true")]
        [DefaultValue(true)]
        public bool ShowTaskBarThumbnails { get; set; } = true;

        [Category("General")]
        [DisplayName("Show on taskbar icons")]
        [Description("Determines if the color icon should be shown in the taskbar icon itself. Only works when taskbar items are ungrouped. Default: false")]
        [DefaultValue(false)]
        public bool ShowTaskBarOverlay { get; set; }

        [Category("General")]
        [DisplayName("Show on title bar")]
        [Description("Determines if solution/folder name on the title bar should be colorized. Default: true")]
        [DefaultValue(true)]
        public bool ShowTitleBar { get; set; } = true;


        [Category("Border")]
        [DisplayName("Width")]
        [Description("The size of the border in pixels. Default: 3")]
        [DefaultValue(3)]
        public int Width { get; set; } = 3;

        [Category("Border")]
        [DisplayName("Location")]
        [Description("The location of the border in the main document window. Default: Top")]
        [TypeConverter(typeof(EnumConverter))]
        [DefaultValue(BorderLocation.Top)]
        public BorderLocation Location { get; set; } = BorderLocation.Top;



        [Browsable(false)]
        public int RatingRequests { get; set; }
    }

    public enum BorderLocation
    {
        Bottom,
        Left,
        Right,
        Top,
    }
}
