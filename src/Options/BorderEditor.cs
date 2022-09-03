using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace SolutionColors.Options
{
    public class BorderEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            BorderSettings borderSettings = value as BorderSettings;

            //if value is null -> create new
            if (value is null)
            {
                borderSettings = new BorderSettings();
            }

            if (provider.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService svc && borderSettings != null)
            {
                using (BorderEditorDialog form = new())
                {
                    form.Borders = borderSettings;
                    if (svc.ShowDialog(form) == DialogResult.OK)
                    {
                        borderSettings = form.Borders;
                    }
                }
            }
            return value;
        }
    }
}
