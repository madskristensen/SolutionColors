global using System;
global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;

namespace SolutionColors
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), "Environment", "Fonts and Colors\\Solution Colors", 0, 0, true, SupportsProfiles = true, ProvidesLocalizedCategoryName = false)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuids.SolutionColorsString)]
    public sealed class SolutionColorsPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();

            var isSolutionLoaded = await VS.Solutions.IsOpenAsync();

            if (isSolutionLoaded)
            {
                HandleOpenSolution();
            }

            await JoinableTaskFactory.SwitchToMainThreadAsync();

            VS.Events.SolutionEvents.OnAfterOpenSolution += HandleOpenSolution;
            VS.Events.SolutionEvents.OnAfterCloseSolution += HandleCloseSolution;
        }

        private void HandleCloseSolution()
        {
            ColorHelper.RemoveBorderAsync().FireAndForget();
        }

        private void HandleOpenSolution(Solution sol = null)
        {
            JoinableTaskFactory.RunAsync(async () =>
            {
                await JoinableTaskFactory.SwitchToMainThreadAsync();

                if (VsShellUtilities.ShellIsShuttingDown)
                {
                    return;
                }

                var color = await ColorHelper.GetColorAsync();

                if (!string.IsNullOrEmpty(color))
                {
                    await ColorHelper.SetColorAsync(color);
                }
            }).FireAndForget();
        }
    }
}