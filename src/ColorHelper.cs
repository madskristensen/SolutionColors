using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;
using Microsoft.VisualStudio.Imaging.Interop;

namespace SolutionColors
{
    public class ColorHelper
    {
        private static Border _border;

        public static async Task SetColorAsync(string colorName)
        {
            if (colorName == null)
            {
                await ClearSolutionAsync();
                await SetBorderColorAsync(null);
            }
            else
            {
                string trueColor = ColorCache.GetColorCode(colorName);

                PropertyInfo property = typeof(Brushes).GetProperty(trueColor, BindingFlags.Static | BindingFlags.Public);

                if (property?.GetValue(null, null) is Brush color)
                {
                    await SetBorderColorAsync(color, colorName);
                }
            }

            Telemetry.TrackOperation("ColorApplied", colorName);
        }

        public static async Task<bool> SolutionHasCustomColorAsync()
        {
            return File.Exists(await GetFileNameAsync());
        }

        public static async Task<string> GetColorAsync()
        {
            string fileName = await GetFileNameAsync();

            if (File.Exists(fileName))
            {
                return File.ReadAllText(fileName).Trim();
            }

            Solution sol = await VS.Solutions.GetCurrentSolutionAsync();

            if (sol != null && General.Instance.AutoMode)
            {
                return ColorCache.GetColor(sol.FullPath);
            }

            return null;
        }

        public static async Task RemoveBorderAsync()
        {
            if (VsShellUtilities.ShellIsShuttingDown)
            {
                return;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (_border != null)
            {
                _border.BorderThickness = new Thickness(0);
            }

            ResetTaskbar();
        }

        public static async Task ClearSolutionAsync()
        {
            string fileName = await GetFileNameAsync();

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        public static async Task ResetAsync()
        {
            await RemoveBorderAsync();

            string color = await GetColorAsync();
            await SetColorAsync(color);
        }

        public static async Task<string> GetFileNameAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Solution solution = await VS.Solutions.GetCurrentSolutionAsync();
            string rootDir;

            if (solution?.Name?.EndsWith(".wsp") == true)
            {
                // .wsp is Open Folder and not regular .sln solutions
                rootDir = solution.FullPath;
            }
            else
            {
                rootDir = Path.GetDirectoryName(solution.FullPath);
            }

            string vsDir = Path.Combine(rootDir, ".vs");

            if (!Directory.Exists(vsDir))
            {
                DirectoryInfo di = Directory.CreateDirectory(vsDir);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }

            return Path.Combine(vsDir, "color.txt");
        }

        private static async Task SetBorderColorAsync(Brush color, string colorName = null)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            General options = await General.GetLiveInstanceAsync();

            BorderLocation location = options.Location;
            string controlName = GetControlName(location);
            _border = FindChild(Application.Current.MainWindow, controlName) as Border;

            if (color == null)
            {
                await RemoveBorderAsync();
            }
            else
            {
                if (options.ShowBorder)
                {
                    _border.BorderBrush = color;
                    if (location == BorderLocation.Bottom)
                    {
                        _border.BorderThickness = new Thickness(0, General.Instance.Width, 0, 0);
                    }
                    else
                    {
                        _border.BorderThickness = new Thickness(General.Instance.Width, 0, 0, 0);
                    }
                }

                if (options.ShowTaskBarIcon)
                {
                    int index = ColorCache.GetIndex(colorName);

                    if (index > -1)
                    {
                        ResetTaskbar();
                        ImageMoniker moniker = new() { Guid = new Guid("A1FA08E5-519B-4810-BDB0-89F586AF37E9"), Id = index + 1 };
                        Application.Current.MainWindow.TaskbarItemInfo.ThumbButtonInfos.Add(new ThumbButtonInfo() { ImageSource = await moniker.ToBitmapSourceAsync(16), Description = colorName, IsBackgroundVisible = false, IsInteractive = false });
                    }
                }
            }
        }

        private static void ResetTaskbar()
        {
            Application.Current.MainWindow.TaskbarItemInfo ??= new();
            Application.Current.MainWindow.TaskbarItemInfo.ThumbButtonInfos ??= new ThumbButtonInfoCollection();
            Application.Current.MainWindow.TaskbarItemInfo.ThumbButtonInfos.Clear();
        }

        private static string GetControlName(BorderLocation location) => location switch
        {
            BorderLocation.Left => "LeftDockBorder",
            BorderLocation.Right => "RightDockBorder",
            _ => "BottomDockBorder",
        };

        private static DependencyObject FindChild(DependencyObject parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                {
                    return frameworkElement;
                }
            }

            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                child = FindChild(child, childName);

                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }
    }
}
