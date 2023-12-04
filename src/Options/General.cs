using System.ComponentModel;
using System.Globalization;
using System.Reflection;
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
        [DisplayName("Automatic")]
        [Description("Automatically assign and apply a color to solutions as they open. You can still manually assign colors when needed. Default: false")]
        [DefaultValue(false)]
        public bool AutoMode { get; set; }
        
        [Category("General")]
        [DisplayName("Show on taskbar thumbnails")]
        [Description("Determines if the color icon should be shown in the taskbar thumbnails. Default: MainWindowOnly")]
        [DefaultValue(TaskBarOptions.MainWindowOnly)]
        public TaskBarOptions ShowTaskBarThumbnails { get; set; } = TaskBarOptions.MainWindowOnly;

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

        [Category("General")]
        [DisplayName("Coloration")]
        [Description("Determines the coloration for branches. Default: unitary")]
        [TypeConverter(typeof(EnumDescriptionConverter))]
        [DefaultValue(Coloration.Unitary)]
        public Coloration Coloration { get; set; } = Coloration.Unitary;

        [Category("General")]
        [DisplayName("Base Color")]
        [Description("Determines the color for titlebar, taskbar icon and thumbnails if respective gradient is set to false. Default: master")]
        [TypeConverter(typeof(EnumDescriptionConverter))]
        [DefaultValue(BaseColor.MasterColor)]
        public BaseColor BaseColor { get; set; } = BaseColor.MasterColor;

        [Category("General")]
        [DisplayName("Gradient in taskbar (thumbnail and icon)")]
        [Description("Determines if the thumbnails and icons should have a linear gradient (only for branches and if Coloration is set to combined). Default: false")]
        [DefaultValue(false)]
        public bool UseGradientTaskbar { get; set; } = false;

        [Category("General")]
        [DisplayName("Gradient in titlebar")]
        [Description("Determines if titlebar should have a linear gradient (only for branches and if Coloration is set to combined). Default: false")]
        [DefaultValue(false)]
        public bool UseGradientTitlebar { get; set; } = false;

        [Category("General")]
        [DisplayName("Gradient in borders")]
        [Description("Determines the type of gradient for borders (only for branches and if Coloration is set to combined). Default: radial gradient")]
        [TypeConverter(typeof(EnumDescriptionConverter))]
        [DefaultValue(Gradient.RadialGradient)]
        public Gradient GradientBorders { get; set; } = Gradient.RadialGradient;

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

    public enum BaseColor
    {
        [Description("Master color")]
        MasterColor = 1,
        [Description("Branch color")]
        BranchColor = 2
    }

    public enum Coloration
    {
        [Description("Unitary (only one color for all branches)")]
        Unitary = 1,
        [Description("Color per branch")]
        Branch = 2,
        [Description("Combined (gradient coloring)")]
        Combined = 3
    }

    public enum Gradient
    {
        [Description("Linear gradient")]
        LinearGradient = 1,
        [Description("Radial gradient")]
        RadialGradient = 2
    }
}
