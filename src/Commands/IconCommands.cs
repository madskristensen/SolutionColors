using System.IO;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.Win32;
using static SolutionColors.ColorHelper;
using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace SolutionColors
{
    [Command(PackageIds.cSetIcon)]
    internal class SetIconCommand : BaseCommand<SetIconCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            string iconLocation = await GetFileNameAsync(false);

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Filetypes that are supported by BitmapImage
                openFileDialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.gif;*.png;*.ico;*.tif;*.tiff;*.wdp;*.raw";
                openFileDialog.Title = "Please select an Image file";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.Copy(openFileDialog.FileName, iconLocation, true);
                        await ColorHelper.ApplyIconAsync();
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show($"{openFileDialog.FileName} - {error.Message}", "Could not set icon", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            Telemetry.TrackUserTask("SetIcon");
        }
    }

    [Command(PackageIds.cRemoveIcon)]
    internal class RemoveIconCommand : BaseCommand<RemoveIconCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            string iconLocation = GetFileName(false);
            File.Delete(iconLocation);
            await ColorHelper.ApplyIconAsync();
        }

        protected override void BeforeQueryStatus(EventArgs e)
        {
            string iconLocation = GetFileName(false);
            Command.Enabled = File.Exists(iconLocation);
        }
    }
}
