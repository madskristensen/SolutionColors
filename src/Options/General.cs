using System.ComponentModel;
using System.Runtime.InteropServices;
using SolutionColors.Options;

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

        [Category("General")]
        [DisplayName("Save settings file in root folder")]
        [Description("Determines if the settings file should be saved in the root folder or in the .vs folder. Default: false")]
        [DefaultValue(false)]
        public bool SaveInRoot { get; set; } = false;

        [Category("General")]
        [DisplayName("Border settings")]
        [Description("Sets border locations and width in pixels. Default: bottom 3")]
        [TypeConverter(typeof(BorderTypeConverter))]
        public BorderSettings Borders { get; set; } = new BorderSettings();

        [Browsable(false)]
        public int RatingRequests { get; set; }
    }

    [Flags]
    public enum BorderLocation
    {
        Bottom = 1,
        Left = 2,
        Right = 4,
        Top = 8
    }
}
