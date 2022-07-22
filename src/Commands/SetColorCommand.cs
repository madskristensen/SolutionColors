using EnvDTE;
using EnvDTE80;
using EnvDTE90;

namespace SolutionColors
{
    [Command(PackageIds.Blue)] internal sealed class Blue : ColorBaseCommand<Blue> { }
    [Command(PackageIds.Red)] internal sealed class Red : ColorBaseCommand<Red> { }
    [Command(PackageIds.Green)] internal sealed class Green : ColorBaseCommand<Green> { }
    [Command(PackageIds.Turquoise)] internal sealed class Turquoise : ColorBaseCommand<Turquoise> { }
    [Command(PackageIds.Purple)] internal sealed class Purple : ColorBaseCommand<Purple> { }
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
