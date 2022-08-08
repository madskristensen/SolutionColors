using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SolutionColors
{
    public class ColorHelper
    {
        private static Border _border;

        private static readonly Dictionary<string, string> _colorMap = new()
        {
            { "Burgundy", "Tomato" },
            { "Pumpkin", "OrangeRed" },
            { "Volt", "YellowGreen" },
            { "Mint", "MediumAquamarine" },
            { "DarkBrown", "SaddleBrown" },
        };

        public static async Task SetColorAsync(string colorName)
        {
            string fileName = await GetFileNameAsync();

            if (colorName == null && File.Exists(fileName))
            {
                File.Delete(fileName);
                await SetBorderColorAsync(null);
            }
            else
            {
                string existingColor = await GetColorAsync();

                if (colorName != existingColor)
                {
                    File.WriteAllText(fileName, colorName);
                }

                if (!_colorMap.TryGetValue(colorName, out string trueColor))
                {
                    trueColor = colorName;
                }

                PropertyInfo property = typeof(Brushes).GetProperty(trueColor, BindingFlags.Static | BindingFlags.Public);

                if (property?.GetValue(null, null) is Brush color)
                {
                    await SetBorderColorAsync(color);
                }                
            }
            
            Telemetry.TrackOperation("ColorApplied", colorName);
        }

        public static async Task<string> GetColorAsync()
        {
            string fileName = await GetFileNameAsync();

            if (File.Exists(fileName))
            {
                return File.ReadAllText(fileName).Trim();
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
            await SetBorderColorAsync(null);
        }

        private static async Task<string> GetFileNameAsync()
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

        private static async Task SetBorderColorAsync(Brush color)
        {
            General options = await General.GetLiveInstanceAsync();
            BorderLocation location = options.Location;
            string controlName = GetControlName(location);
            _border ??= FindChild(Application.Current.MainWindow, controlName) as Border;

            if (color == null)
            {
                _border.BorderThickness = new Thickness(0);
            }
            else
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
