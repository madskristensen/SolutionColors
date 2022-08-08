namespace SolutionColors
{
    [Command(PackageIds.Options)]
    internal class OptionsCommand : BaseCommand<OptionsCommand>
    {
        protected override void Execute(object sender, EventArgs e) => 
            Package.ShowOptionPage(typeof(OptionsProvider.GeneralOptions));
    }
}
