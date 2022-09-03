using System.IO;
using System.Windows.Forms;
using System.Windows.Media;

namespace SolutionColors
{
    [Command(PackageIds.Custom)]
    internal class CustomCommand : BaseCommand<CustomCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            ColorDialog dialog = new();
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                Color converted = Color.FromArgb(dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B);
                
                await ColorHelper.SetColorAsync(converted.ToString());
                await ColorHelper.ColorizeAsync();
                
                string fileName = await ColorHelper.GetFileNameAsync();
                File.WriteAllText(fileName, converted.ToString());
            }
        }
    }
}
