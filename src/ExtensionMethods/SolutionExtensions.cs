using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace SolutionColors
{
    internal static class SolutionExtensions
    {
        private const string _dotslnf = ".slnf";

        /// <summary>
        /// Return solution filter (slnf) name if slnf is opened, otherwise return solution (sln) name.
        /// </summary>
        public static async Task<string> GetSolutionNameAsync(this Solution solution)
        {
            return await GetSlnfFileNameAsync() ?? solution.Name;
        }

        /// <summary>
        /// Return solution filter (slnf) name(!) if slnf is opened, otherwise return solution (sln) path(!).
        /// </summary>
        public static async Task<string> GetSolutionPathAsync(this Solution solution)
        {
            return await GetSlnfFileNameAsync() ?? solution.FullPath;
        }

        private static async Task<string> GetSlnfFileNameAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            //check for opened solution filter (slnf)
            //if slnf exists, use it instead of solution name
            IVsSolution vss = await VS.Services.GetSolutionAsync();
            ErrorHandler.ThrowOnFailure(vss.GetSolutionInfo(out _, out _, out string optionsFile));

            if (!string.IsNullOrEmpty(optionsFile) && optionsFile.Contains(_dotslnf))
            {
                //cut slnf file name from
                int right = optionsFile.LastIndexOf(_dotslnf);
                optionsFile = optionsFile.Substring(0, right + _dotslnf.Length);
                if (optionsFile.Contains("\\"))
                {
                    int left = optionsFile.LastIndexOf("\\");
                    optionsFile = optionsFile.Substring(left + 1);
                }
                else if (optionsFile.Contains("/")) //visual studio is not a crossplatform app, but for additional sure check the different separator
                {
                    int left = optionsFile.LastIndexOf("/");
                    optionsFile = optionsFile.Substring(left + 1);
                }

                return optionsFile;//here we use only a slnf NAME, not a slnf PATH, due to: https://developercommunity.visualstudio.com/t/no-way-to-get-path-to-solution-filter-fi/1520237
            }

            return null;
        }
    }
}
