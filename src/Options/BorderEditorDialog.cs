using System.Windows.Forms;
using SolutionColors.Options;

namespace SolutionColors
{
    public partial class BorderEditorDialog : Form
    {
        private BorderSettings _borderSettings;

        public BorderSettings Borders
        {
            get { return _borderSettings; }
            set { _borderSettings = value; }
        }

        public BorderEditorDialog()
        {
            ShowInTaskbar = false;
            InitializeComponent();
        }

        private void BorderEditorDialog_Load(object sender, EventArgs e)
        {
            SetButton(btnBottom, Borders.BorderDetails.Locations.HasFlag(BorderLocation.Bottom));
            SetButton(btnLeft, Borders.BorderDetails.Locations.HasFlag(BorderLocation.Left));
            SetButton(btnRight, Borders.BorderDetails.Locations.HasFlag(BorderLocation.Right));
            SetButton(btnTop, Borders.BorderDetails.Locations.HasFlag(BorderLocation.Top));
            numBottom.Value = Borders.BorderDetails.WidthBottom;
            numLeft.Value = Borders.BorderDetails.WidthLeft;
            numRight.Value = Borders.BorderDetails.WidthRight;
            numTop.Value = Borders.BorderDetails.WidthTop;
        }


        private void SetButton(Button button, bool isSet)
        {
            if (isSet)
            {
                button.ForeColor = System.Drawing.SystemColors.ControlText;
                button.BackColor = System.Drawing.SystemColors.ControlDark;
            }
            else
            {
                button.ForeColor = System.Drawing.SystemColors.ControlDark;
                button.BackColor = System.Drawing.SystemColors.ControlLightLight;
            }
        }

        private void btnBottom_Click(object sender, EventArgs e)
        {
            ToggleLocation(BorderLocation.Bottom);
            SetButton((Button)sender, Borders.BorderDetails.Locations.HasFlag(BorderLocation.Bottom));
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            ToggleLocation(BorderLocation.Left);
            SetButton((Button)sender, Borders.BorderDetails.Locations.HasFlag(BorderLocation.Left));
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            ToggleLocation(BorderLocation.Right);
            SetButton((Button)sender, Borders.BorderDetails.Locations.HasFlag(BorderLocation.Right));
        }

        private void btnTop_Click(object sender, EventArgs e)
        {
            ToggleLocation(BorderLocation.Top);
            SetButton((Button)sender, Borders.BorderDetails.Locations.HasFlag(BorderLocation.Top));
        }


        private void ToggleLocation(BorderLocation location)
        {
            if (Borders.BorderDetails.Locations.HasFlag(location))
            {
                Borders.BorderDetails.Locations &= ~location;
            }
            else
            {
                Borders.BorderDetails.Locations = Borders.BorderDetails.Locations | location;
            }
        }

        private void numBottom_ValueChanged(object sender, EventArgs e)
        {
            Borders.BorderDetails.WidthBottom = (int)((NumericUpDown)sender).Value;
        }

        private void numLeft_ValueChanged(object sender, EventArgs e)
        {
            Borders.BorderDetails.WidthLeft = (int)((NumericUpDown)sender).Value;
        }

        private void numRight_ValueChanged(object sender, EventArgs e)
        {
            Borders.BorderDetails.WidthRight = (int)((NumericUpDown)sender).Value;
        }

        private void numTop_ValueChanged(object sender, EventArgs e)
        {
            Borders.BorderDetails.WidthTop = (int)((NumericUpDown)sender).Value;
        }
    }
}
