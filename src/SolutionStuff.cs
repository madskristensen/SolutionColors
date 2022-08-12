using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace SolutionColors
{
    internal class SolutionStuff
    {
        private const string Dotslnf = ".slnf";

        /// <summary>
        /// Return solution filter (slnf) name if slnf is opened, otherwise return solution (sln) name.
        /// </summary>
        public static async Task<string> GetSolutionNameAsync()
        {
            //check for opened solution filter (slnf)
            //if slnf exists, use it instead of solution name
            IVsSolution vss = AsyncPackage.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            vss.GetSolutionInfo(out string a, out string b, out string c);
            if (!string.IsNullOrEmpty(c) && c.Contains(Dotslnf))
            {
                //cut slnf file name from
                int right = c.LastIndexOf(Dotslnf);
                c = c.Substring(0, right + Dotslnf.Length);
                if (c.Contains("\\"))
                {
                    int left = c.LastIndexOf("\\");
                    c = c.Substring(left + 1);
                }
                else if (c.Contains("/")) //visual studio is not a crossplatform app, but for additional sure check the different separator
                {
                    int left = c.LastIndexOf("/");
                    c = c.Substring(left + 1);
                }

                return c;
            }

            Solution sol = await VS.Solutions.GetCurrentSolutionAsync();
            return sol.Name;
        }

        /// <summary>
        /// Return solution filter (slnf) name(!) if slnf is opened, otherwise return solution (sln) path(!).
        /// </summary>
        public static async Task<string> GetSolutionPathAsync()
        {
            //check for opened solution filter (slnf)
            //if slnf exists, use it instead of solution name
            IVsSolution vss = AsyncPackage.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            vss.GetSolutionInfo(out string a, out string b, out string c);
            if (!string.IsNullOrEmpty(c) && c.Contains(Dotslnf))
            {
                //cut slnf file name from
                int right = c.IndexOf(Dotslnf);
                c = c.Substring(0, right + Dotslnf.Length);
                if (c.Contains("\\"))
                {
                    int left = c.LastIndexOf("\\");
                    c = c.Substring(left + 1);
                }
                else if (c.Contains("/")) //visual studio is not a crossplatform app, but for additional sure check the different separator
                {
                    int left = c.LastIndexOf("/");
                    c = c.Substring(left + 1);
                }

                return c; //here we use only a slnf NAME, not a slnf PATH, due to: https://developercommunity.visualstudio.com/t/no-way-to-get-path-to-solution-filter-fi/1520237
            }

            Solution sol = await VS.Solutions.GetCurrentSolutionAsync();
            return sol.FullPath;
        }
    }
}
