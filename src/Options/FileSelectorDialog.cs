using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms;

namespace SolutionColors.Options
{
    [Editor(typeof(FilePathEditor), typeof(UITypeEditor))]
    [TypeConverter(typeof(FileSelectorSettingConverter))]
    public class FileSelectorSetting
    {
        [Browsable(false)]
        public string FilePath { get; set; }
    }

    // Custom UITypeEditor for file path
    public class FilePathEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return new FileSelectorSetting { FilePath = openFileDialog.FileName };
            }
            return value;
        }
    }

    public class FileSelectorSettingConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is FileSelectorSetting fileSelectorSetting)
            {
                return fileSelectorSetting.FilePath;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}