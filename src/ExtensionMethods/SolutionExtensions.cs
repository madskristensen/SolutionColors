using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace SolutionColors
{
    internal static class SolutionExtensions
    {
        private const string _dotslnf = ".slnf";
        private const string _workspaceExtension = ".wsp";

        /// <summary>
        /// Gets the root directory of the solution or folder workspace.
        /// </summary>
        public static string GetRootDirectory(this Solution solution)
        {
            if (solution?.FullPath == null)
            {
                return null;
            }

            // .wsp is Open Folder and not regular .sln solutions
            if (solution.Name?.EndsWith(_workspaceExtension) == true)
            {
                return solution.FullPath;
            }

            return Path.GetDirectoryName(solution.FullPath);
        }

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
                //cut slnf file name from path
                int right = optionsFile.LastIndexOf(_dotslnf);
                optionsFile = optionsFile.Substring(0, right + _dotslnf.Length);
                return Path.GetFileName(optionsFile);
            }

            return null;
        }
    }
}
