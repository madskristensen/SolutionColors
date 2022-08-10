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
    [ProvideAutoLoad(VSConstants.UICONTEXT.FolderOpened_string, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuids.SolutionColorsString)]
    public sealed class SolutionColorsPackage : ToolkitPackage
    {
        private RatingPrompt _ratingPrompt;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
            
            _ratingPrompt = new RatingPrompt("MadsKristensen.SolutionColors", Vsix.Name, General.Instance, 10);

            if (await VS.Solutions.IsOpenAsync())
            {
                HandleOpenSolution();
            }

            await JoinableTaskFactory.SwitchToMainThreadAsync();


            VS.Events.SolutionEvents.OnAfterOpenSolution += HandleOpenSolution;
            VS.Events.SolutionEvents.OnAfterCloseSolution += HandleCloseSolution;
            VS.Events.SolutionEvents.OnAfterOpenFolder += HandleOpenFolder;
            VS.Events.SolutionEvents.OnAfterCloseFolder += HandleCloseFolder;
            General.Saved += SettingsSaved;
        }

        private void SettingsSaved(General obj)
        {
            JoinableTaskFactory.RunAsync(async () =>
            {
                await JoinableTaskFactory.SwitchToMainThreadAsync();
                await ColorHelper.ResetAsync();
            }).FireAndForget();
        }

        private void HandleOpenFolder(string obj) =>
            HandleOpenSolution();

        private void HandleCloseFolder(string obj) =>
            HandleCloseSolution();

        private void HandleCloseSolution() =>
            ColorHelper.RemoveUIAsync().FireAndForget();

        private void HandleOpenSolution(Solution sol = null)
        {
            JoinableTaskFactory.RunAsync(async () =>
            {
                await JoinableTaskFactory.SwitchToMainThreadAsync();

                string color = await ColorHelper.GetColorAsync();    

                if (!string.IsNullOrEmpty(color))
                {                    
                    await ColorHelper.SetColorAsync(color);

                    await Task.Delay(2000);
                    _ratingPrompt.RegisterSuccessfulUsage();
                }
            }).FireAndForget();
        }
    }
}