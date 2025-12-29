using Microsoft.VisualStudio.Telemetry;

namespace SolutionColors
{
    [Command(PackageIds.Lavender)] internal sealed class Lavender : ColorBaseCommand<Lavender> { }
    [Command(PackageIds.Gold)] internal sealed class Gold : ColorBaseCommand<Gold> { }
    [Command(PackageIds.Cyan)] internal sealed class Cyan : ColorBaseCommand<Cyan> { }
    [Command(PackageIds.Burgundy)] internal sealed class Burgundy : ColorBaseCommand<Burgundy> { }
    [Command(PackageIds.Green)] internal sealed class Green : ColorBaseCommand<Green> { }
    [Command(PackageIds.Brown)] internal sealed class Brown : ColorBaseCommand<Brown> { }
    [Command(PackageIds.RoyalBlue)] internal sealed class RoyalBlue : ColorBaseCommand<RoyalBlue> { }
    [Command(PackageIds.Pumpkin)] internal sealed class Pumpkin : ColorBaseCommand<Pumpkin> { }
    [Command(PackageIds.Gray)] internal sealed class Gray : ColorBaseCommand<Gray> { }
    [Command(PackageIds.Volt)] internal sealed class Volt : ColorBaseCommand<Volt> { }
    [Command(PackageIds.Teal)] internal sealed class Teal : ColorBaseCommand<Teal> { }
    [Command(PackageIds.Magenta)] internal sealed class Magenta : ColorBaseCommand<Magenta> { }
    [Command(PackageIds.Mint)] internal sealed class Mint : ColorBaseCommand<Mint> { }
    [Command(PackageIds.DarkBrown)] internal sealed class DarkBrown : ColorBaseCommand<DarkBrown> { }
    [Command(PackageIds.Blue)] internal sealed class Blue : ColorBaseCommand<Blue> { }
    [Command(PackageIds.Pink)] internal sealed class Pink : ColorBaseCommand<Pink> { }
    [Command(PackageIds.None)] internal sealed class None : ColorBaseCommand<None> { }

    internal abstract class ColorBaseCommand<T> : BaseCommand<T> where T : class, new()
    {
        private const string NoneColor = "None";
        private readonly string _color;

        protected ColorBaseCommand()
        {
            _color = GetType().Name;
            ColorCache.AddColor(_color);
        }

        protected override void BeforeQueryStatus(EventArgs e)
        {
            Solution solution = VS.Solutions.GetCurrentSolution();

            if (solution?.Name?.EndsWith(".wsp") == true)
            {
                Command.Visible = Command.Enabled = SetColorCommandFolder.IsRoot;
            }

            // Only update text for the "None" command
            if (_color != NoneColor)
            {
                return;
            }

#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
            Package.JoinableTaskFactory.Run(async () =>
            {
                if (await ColorHelper.SolutionHasCustomColorAsync())
                {
                    Command.Text = "None";
                    Command.Checked = false;
                }
                else
                {
                    Command.Text = "Automatic";
                    Command.Checked = General.Instance.AutoMode;
                }
            });
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            if (_color == NoneColor)
            {
                await HandleNoneColorAsync();
            }
            else
            {
                await ColorHelper.SetColorAsync(_color);
                await ColorHelper.ColorizeAsync();
            }

            TelemetryEvent tel = Telemetry.CreateEvent("ChangedColor");
            tel.Properties["Color"] = _color;
            Telemetry.TrackEvent(tel);
        }

        private static async Task HandleNoneColorAsync()
        {
            General options = await General.GetLiveInstanceAsync();

            if (await ColorHelper.SolutionHasCustomColorAsync())
            {
                // Solution has a custom color - clear it
                if (options.AutoMode)
                {
                    bool disableAutoMode = await VS.MessageBox.ShowConfirmAsync(
                        "Automatic colorization is currently enabled. Do you wish to disable it too?");

                    if (disableAutoMode)
                    {
                        options.AutoMode = false;
                        await options.SaveAsync();
                    }
                }

                await ColorHelper.SetColorAsync((string)null);
                await ColorHelper.ColorizeAsync();
            }
            else
            {
                // No custom color - toggle automatic mode
                options.AutoMode = !options.AutoMode;
                await options.SaveAsync();
                await ColorHelper.ResetAsync();
            }
        }
    }
}
