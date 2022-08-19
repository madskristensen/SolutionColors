using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Telemetry;

namespace SolutionColors
{
    public class ColorHelper
    {
        public const string ColorFileName = "color.txt";

        private static Border _border;
        private static Border _solutionLabel;
        private static Brush _originalLabelColor;
        private static SolidColorBrush _originalLabelForeground;
        private static PropertyInfo _solutionLabelForegroundProperty;

        public static async Task SetColorAsync(string colorName)
        {
            if (colorName == null)
            {
                await ClearSolutionAsync();
                await SetUiColorAsync(null);
            }
            else
            {
                string trueColor = ColorCache.GetColorCode(colorName);

                PropertyInfo property = typeof(Brushes).GetProperty(trueColor, BindingFlags.Static | BindingFlags.Public);

                if (property?.GetValue(null, null) is SolidColorBrush color)
                {
                    await SetUiColorAsync(color, colorName);
                }
                else if (ColorConverter.ConvertFromString(trueColor) is Color hexColor)
                {
                    SolidColorBrush brush = new(hexColor);
                    await SetUiColorAsync(brush, "custom");
                }
            }
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
                string path = await sol.GetSolutionPathAsync();
                return ColorCache.GetColor(path);
            }

            return null;
        }

        public static async Task RemoveUIAsync()
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

            if (_solutionLabel != null)
            {
                _solutionLabel.Background = _originalLabelColor;
                _solutionLabelForegroundProperty.SetValue(_solutionLabel.Child, _originalLabelForeground);
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
            await RemoveUIAsync();

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

            string vsDir = Path.Combine(
                rootDir,
                ".vs",
                Path.GetFileNameWithoutExtension(await solution.GetSolutionNameAsync()));

            if (!Directory.Exists(vsDir))
            {
                DirectoryInfo di = Directory.CreateDirectory(vsDir);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }

            return Path.Combine(vsDir, ColorFileName);
        }

        private static async Task SetUiColorAsync(SolidColorBrush brush, string colorName = null)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (brush == null)
            {
                await RemoveUIAsync();
            }
            else
            {
                General options = await General.GetLiveInstanceAsync();

                if (options.ShowBorder)
                {
                    ShowInBorder(brush, options);
                }

                if (options.ShowTitleBar)
                {
                    ShowInTitleBar(brush);
                }

                if (options.ShowTaskBarThumbnails || options.ShowTaskBarOverlay)
                {
                    ShowInTaskBar(brush, options);
                }

                TelemetryEvent tel = Telemetry.CreateEvent("ColorApplied");
                tel.Properties["Color"] = colorName;
                tel.Properties[nameof(options.ShowBorder)] = options.ShowBorder;
                tel.Properties[nameof(options.ShowTaskBarOverlay)] = options.ShowTaskBarOverlay;
                tel.Properties[nameof(options.ShowTaskBarThumbnails)] = options.ShowTaskBarThumbnails;
                tel.Properties[nameof(options.ShowTitleBar)] = options.ShowTitleBar;
                tel.Properties[nameof(options.AutoMode)] = options.AutoMode;
                tel.Properties["borderlocation"] = options.Location.ToString();
                tel.Properties["borderwidth"] = options.Width;
                Telemetry.TrackEvent(tel);
            }
        }

        private static void ShowInTaskBar(Brush brush, General options)
        {
            ResetTaskbar();

            if (options.ShowTaskBarThumbnails)
            {
                Application.Current.MainWindow.TaskbarItemInfo.ThumbButtonInfos.Add(new ThumbButtonInfo() { ImageSource = brush.GetImageSource(16), IsBackgroundVisible = false, IsInteractive = false });
            }

            if (options.ShowTaskBarOverlay)
            {
                Application.Current.MainWindow.TaskbarItemInfo.Overlay = brush.GetImageSource(12);
            }
        }

        private static void ShowInBorder(SolidColorBrush color, General options)
        {
            BorderLocation location = options.Location;
            string controlName = GetControlName(location);
            _border = Application.Current.MainWindow.FindChild<Border>(controlName);

            if (_border != null)
            {
                _border.BorderBrush = color;

                if (location == BorderLocation.Bottom || location == BorderLocation.Top)
                {
                    _border.BorderThickness = new Thickness(0, General.Instance.Width, 0, 0);
                }
                else
                {
                    _border.BorderThickness = new Thickness(General.Instance.Width, 0, 0, 0);
                }
            }
        }

        private static void ShowInTitleBar(SolidColorBrush color)
        {
            if (_solutionLabel == null)
            {
                _solutionLabel = Application.Current.MainWindow.FindChild<Border>("TextBorder");
                _originalLabelColor = _solutionLabel?.Background;
            }

            if (_solutionLabel != null)
            {
                _solutionLabelForegroundProperty ??= _solutionLabel.Child.GetType().GetProperty("Foreground", BindingFlags.Public | BindingFlags.Instance);
                _originalLabelForeground ??= _solutionLabelForegroundProperty?.GetValue(_solutionLabel.Child) as SolidColorBrush;

                if (_solutionLabelForegroundProperty != null)
                {
                    _solutionLabel.Background = color;
                    ContrastComparisonResult contrast = ColorUtilities.CompareContrastWithBlackAndWhite(color.Color);
                    _solutionLabelForegroundProperty.SetValue(_solutionLabel.Child, contrast == ContrastComparisonResult.ContrastHigherWithWhite ? Brushes.White : Brushes.Black);
                }
            }
        }

        private static void ResetTaskbar()
        {
            Application.Current.MainWindow.TaskbarItemInfo ??= new();
            Application.Current.MainWindow.TaskbarItemInfo.ThumbButtonInfos ??= new ThumbButtonInfoCollection();
            Application.Current.MainWindow.TaskbarItemInfo.ThumbButtonInfos.Clear();
            Application.Current.MainWindow.TaskbarItemInfo.Overlay = null;
        }

        private static string GetControlName(BorderLocation location) => location switch
        {
            BorderLocation.Left => "LeftDockBorder",
            BorderLocation.Right => "RightDockBorder",
            BorderLocation.Top => "MainWindowTitleBar",
            _ => "BottomDockBorder",
        };
    }
}
