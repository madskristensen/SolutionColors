using EnvDTE;
using EnvDTE80;

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
        private readonly string _color;

        public ColorBaseCommand()
        {
            _color = GetType().Name;
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            if (_color == "None")
            {
                await ColorHelper.RemoveBorderAsync();

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                DTE2 dte = await VS.GetServiceAsync<DTE, DTE2>();

                dte.Solution.Globals.VariablePersists["color"] = false;
                dte.Solution.SaveAs(dte.Solution.FullName);
            }
            else
            {
                await ColorHelper.SetColorAsync(_color);
            }
        }
    }
}
