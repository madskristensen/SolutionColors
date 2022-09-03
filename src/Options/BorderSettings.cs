using System.ComponentModel;
using System.Drawing.Design;

namespace SolutionColors.Options
{
    [Editor(typeof(BorderEditor), typeof(UITypeEditor))]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class BorderSettings
    {
        private BorderDetails _borderDetails = new();

        [Browsable(false)]
        public BorderDetails BorderDetails
        {
            get { return _borderDetails; }
            set { _borderDetails = value; }
        }
    }

    public class BorderDetails
    {
        public BorderLocation Locations = BorderLocation.Bottom;
        public int WidthBottom { get; set; } = 3;
        public int WidthLeft { get; set; } = 3;
        public int WidthRight { get; set; } = 3;
        public int WidthTop { get; set; } = 3;
    }
}
