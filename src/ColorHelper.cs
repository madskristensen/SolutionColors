using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;
using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Telemetry;

namespace SolutionColors
{
    public class ColorHelper
    {
        public const string ColorFileName = "color.txt";
        public const string IconFileName = "icon.img"; // This could be one of many filetypes supported by BitmapImage

        private static Border _solutionLabel;
        private static Brush _originalLabelColor;
        private static SolidColorBrush _originalLabelForeground;
        private static PropertyInfo _solutionLabelForegroundProperty;

        //INFO:
        //colorMaster: the color of master branch
        //colorBranch: the color of branch (= master branch color when unitary coloration)

        public static async Task ResetInstanceAsync()
        {
            _colorEntries = null;
            await RemoveUIAsync();
        }

        public static async Task SetColorAsync(string colorName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            General options = await General.GetLiveInstanceAsync();

            if (_colorEntries == null)
            {
                await LoadColorsAsync(await GitHelper.GetBranchNameAsync());
            }

            string fileName = await GetFileNameAsync();
            string _branch = string.Empty;

            if (options.Coloration == Coloration.Unitary)   //use only master color
            {
                _branch = "master";
            }
            else
            {
                _branch = await GitHelper.GetBranchNameAsync();
            }

            ColorEntry colorEntry = _colorEntries.FirstOrDefault(x => x.branch == _branch);
            if (colorEntry != null)
            {
                colorEntry.color = colorName;
            }
            else
            {
                _colorEntries.Add(new() { branch = _branch, color = colorName });
            }

            string fileContent = string.Empty;
            foreach (ColorEntry colorEntryToSave in _colorEntries)
            {
                fileContent += (fileContent.Length == 0 ? "" : Environment.NewLine) + colorEntryToSave.branch + ":" + colorEntryToSave.color;
            }
            File.WriteAllText(await GetFileNameAsync(), fileContent);
        }

        public static async Task<bool> SolutionHasCustomColorAsync()
        {
            return File.Exists(await GetFileNameAsync());
        }

        public static async Task<string> GetColorAsync(string branch)
        {
            if (_colorEntries == null)
            {
                await LoadColorsAsync(branch);
            }

            ColorEntry colorEntry = _colorEntries.FirstOrDefault(x => x.branch == branch);
            if (colorEntry != null)
            {
                return colorEntry.color;
            }
            else
            {
                if (branch == "master")
                {
                    return string.Empty;
                }
                else
                {
                    return Colors.DarkGray.ToString();
                }
            }
        }

        public static async Task ColorizeAsync()
        {
            await SetUiColorAsync();
        }

        public static async Task ApplyIconAsync()
        {
            await SetIconAsync();
        }

        public static async Task LoadColorsAsync(string branch)
        {
            _colorEntries ??= new();

            string fileName = await GetFileNameAsync();

            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                //Compatibility to older version
                string fileContent = File.ReadAllText(fileName);
                string[] lines = fileContent.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Count() == 1 && lines[0].Split(':').Count() == 1)
                {
                    _colorEntries.Add(new ColorEntry() { branch = "master", color = lines[0] });
                }
                else
                {
                    foreach (string line in lines)
                    {
                        string[] lineSegments = line.Split(':');
                        _colorEntries.Add(new() { branch = lineSegments[0], color = lineSegments[1] });
                    }
                }
            }
        }

        static private async void WindowEvents_WindowActivated(EnvDTE.Window GotFocus, EnvDTE.Window LostFocus)
        {
            WindowCollection allWindows = Application.Current.Windows;
            int count = allWindows.Count;
            await SetUiColorAsync();
        }

        public static async Task RemoveUIAsync()
        {
            DTE2 env = (EnvDTE80.DTE2)await ServiceProvider.GetGlobalServiceAsync(typeof(SDTE));
            if (env != null)
            {
                env.Events.WindowEvents.WindowActivated -= WindowEvents_WindowActivated;
            }

            if (VsShellUtilities.ShellIsShuttingDown)
            {
                return;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            foreach (Enum value in Enum.GetValues(typeof(BorderLocation)))
            {
                string controlName = GetControlName((BorderLocation)value);
                Border _border = Application.Current.MainWindow.FindChild<Border>(controlName);
                if (_border != null)
                {
                    _border.BorderThickness = new Thickness(0);
                }
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
            await SetUiColorAsync();

            General options = await General.GetLiveInstanceAsync();
            if (options.ShowTaskBarThumbnails != Options.TaskBarOptions.None || options.ShowTaskBarOverlay)
            {
                DTE2 env = (EnvDTE80.DTE2)await ServiceProvider.GetGlobalServiceAsync(typeof(SDTE));
                if (env != null)
                {
                    env.Events.WindowEvents.WindowActivated += WindowEvents_WindowActivated;
                }
            }
        }

        public static string GetFileName(bool isColor = true)
        {
            // This is bad practice but doesn't introduce any noticable hitch and is much easier than reengineering everything
            Task<string> iconNameTask = GetFileNameAsync(false);
            iconNameTask.Wait();
            return iconNameTask.Result;
        }

        public static async Task<string> GetFileNameAsync(bool isColor = true)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Solution solution = await VS.Solutions.GetCurrentSolutionAsync();

            if (solution?.FullPath == null)
            {
                return null;
            }

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

            General options = await General.GetLiveInstanceAsync();

            string vsDir;

            if (options.SaveInRoot)
            {
                vsDir = rootDir;
            }
            else
            {
                vsDir = Path.Combine(
                    rootDir,
                    ".vs",
                    Path.GetFileNameWithoutExtension(await solution.GetSolutionNameAsync()));

                if (!Directory.Exists(vsDir))
                {
                    DirectoryInfo di = Directory.CreateDirectory(vsDir);
                    di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                }
            }

            return Path.Combine(vsDir, isColor ? ColorFileName : IconFileName);
        }

        private static async Task SetIconAsync()
        {
            General options = await General.GetLiveInstanceAsync();

            if (options.ShowTaskBarThumbnails != Options.TaskBarOptions.None || options.ShowTaskBarOverlay)
            {
                ShowInTaskBar(Colors.Transparent, Colors.Transparent, options);
            }
        }

        private static async Task SetUiColorAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            string currentBranch = await GitHelper.GetBranchNameAsync();

            General options = await General.GetLiveInstanceAsync();

            if (string.IsNullOrEmpty(await GetColorAsync("master")) && options.AutoMode == false)
            {
                await RemoveUIAsync();
                return;
            }

            Color colorMaster;
            if (options.AutoMode == true && !await SolutionHasCustomColorAsync())
            {
                Solution sol = await VS.Solutions.GetCurrentSolutionAsync();
                if (sol?.FullPath != null)
                {
                    string path = await sol.GetSolutionPathAsync();
                    colorMaster = (Color)ColorConverter.ConvertFromString(ColorCache.GetColor(path));
                }
                else
                {
                    colorMaster = Colors.Black;
                }
            }
            else
            {
                colorMaster = (Color)ColorConverter.ConvertFromString(ColorCache.GetColorCode(await GetColorAsync("master")));
            }

            Color colorBranch;
            if (currentBranch == "master")
            {
                colorBranch = colorMaster;
            }
            else
            {
                if (options.AutoMode == true && !await SolutionHasCustomColorAsync())
                {
                    Solution sol = await VS.Solutions.GetCurrentSolutionAsync();
                    if (sol?.FullPath != null)
                    {
                        string path = await sol.GetSolutionPathAsync();
                        colorBranch = (Color)ColorConverter.ConvertFromString(ColorCache.GetColor(path + currentBranch));
                    }
                    else
                    {
                        colorBranch = Colors.Black;
                    }
                }
                else
                {
                    string colorNameBranch = await GetColorAsync(currentBranch);
                    if (string.IsNullOrEmpty(colorNameBranch))
                    {
                        if (options.Coloration == Coloration.Branch)
                        {
                            await RemoveUIAsync();
                            return;
                        }
                        else
                        {
                            colorBranch = Colors.Black;
                        }
                    }
                    else
                    {
                        colorBranch = (Color)ColorConverter.ConvertFromString(ColorCache.GetColorCode(await GetColorAsync(currentBranch)));
                    }
                }
            }



            ShowInBorders(colorMaster, colorBranch, options);

            if (options.ShowTitleBar)
            {
                ShowInTitleBar(colorMaster, colorBranch, options);
            }

            if (options.ShowTaskBarThumbnails != Options.TaskBarOptions.None || options.ShowTaskBarOverlay)
            {
                ShowInTaskBar(colorMaster, colorBranch, options);
            }

            TelemetryEvent tel = Telemetry.CreateEvent("ColorApplied");
            tel.Properties[nameof(options.ShowTaskBarOverlay)] = options.ShowTaskBarOverlay;
            tel.Properties[nameof(options.ShowTaskBarThumbnails)] = options.ShowTaskBarThumbnails;
            tel.Properties[nameof(options.ShowTitleBar)] = options.ShowTitleBar;
            tel.Properties[nameof(options.AutoMode)] = options.AutoMode;
            tel.Properties[nameof(options.SaveInRoot)] = options.SaveInRoot;
            tel.Properties[nameof(options.Borders)] = options.Borders;
            tel.Properties[nameof(options.Coloration)] = options.Coloration;
            tel.Properties[nameof(options.BaseColor)] = options.BaseColor;
            tel.Properties[nameof(options.UseGradientTaskbar)] = options.UseGradientTaskbar;
            tel.Properties[nameof(options.UseGradientTitlebar)] = options.UseGradientTitlebar;
            tel.Properties[nameof(options.GradientBorders)] = options.GradientBorders;
            Telemetry.TrackEvent(tel);
        }

        private static void ShowInTaskBar(Color colorMaster, Color colorBranch, General options)
        {
            ResetTaskbar();

            //LinearGradientBrush brush = new LinearGradientBrush(colorMaster, colorBranch, 0);
            Brush brush = GetBrushForTaskbar(colorMaster, colorBranch, options);

            switch(options.ShowTaskBarThumbnails)
            {
                case Options.TaskBarOptions.MainWindowOnly:
                    Application.Current.MainWindow.TaskbarItemInfo.ThumbButtonInfos.Add(new ThumbButtonInfo() { ImageSource = brush.GetImageSource(16), IsBackgroundVisible = false, IsInteractive = false });
                    break;
                case Options.TaskBarOptions.AllWindows:
                    foreach (System.Windows.Window window in Application.Current.Windows)
                    {
                        if (window.TaskbarItemInfo == null)
                            window.TaskbarItemInfo = new TaskbarItemInfo();

                        if (window.TaskbarItemInfo.ThumbButtonInfos == null)
                            window.TaskbarItemInfo.ThumbButtonInfos = new ThumbButtonInfoCollection();

                        window.TaskbarItemInfo.ThumbButtonInfos.Add(new ThumbButtonInfo() { ImageSource = brush.GetImageSource(16), IsBackgroundVisible = false, IsInteractive = false });
                    }
                    break;
                default:
                    break;
            }

            if (options.ShowTaskBarOverlay)
            {
                foreach (System.Windows.Window window in Application.Current.Windows)
                {
                    window.TaskbarItemInfo.Overlay = brush.GetImageSource(12);
                }
            }
        }

        private static Brush GetBrushForTaskbar(Color colorMaster, Color colorBranch, General options)
        {
            Brush brush = null;
            if (options.Coloration == Coloration.Unitary)
            {
                brush = new SolidColorBrush(colorMaster);
            }
            else if (options.Coloration == Coloration.Branch)
            {
                brush = new SolidColorBrush(colorBranch);
            }
            else if (options.Coloration == Coloration.Combined)
            {
                if (options.UseGradientTaskbar == false)
                {
                    if (options.BaseColor == BaseColor.MasterColor)
                    {
                        brush = new SolidColorBrush(colorMaster);
                    }
                    else
                    {
                        brush = new SolidColorBrush(colorBranch);
                    }
                }
                else
                {
                    brush = new LinearGradientBrush(colorMaster, colorBranch, new Point(0.3, 0), new Point(0.7, 0));
                }
            }
            return brush;
        }

        private static void ShowInBorders(Color colorMaster, Color colorBranch, General options)
        {
            foreach (Enum value in Enum.GetValues(options.Borders.BorderDetails.Locations.GetType()))
            {

                if (options.Borders.BorderDetails.Locations.HasFlag(value))
                {
                    string controlName = GetControlName((BorderLocation)value);
                    Border _border = Application.Current.MainWindow.FindChild<Border>(controlName);

                    if (_border != null)
                    {
                        _border.BorderBrush = GetBrushForBorder(colorMaster, colorBranch, options, (BorderLocation)value);

                        switch ((BorderLocation)value)
                        {
                            case BorderLocation.Bottom:
                                _border.BorderThickness = new Thickness(0, General.Instance.Borders.BorderDetails.WidthBottom, 0, 0);
                                break;
                            case BorderLocation.Left:
                                _border.BorderThickness = new Thickness(General.Instance.Borders.BorderDetails.WidthLeft, 0, 0, 0);
                                break;
                            case BorderLocation.Right:
                                _border.BorderThickness = new Thickness(General.Instance.Borders.BorderDetails.WidthRight, 0, 0, 0);
                                break;
                            case BorderLocation.Top:
                                _border.BorderThickness = new Thickness(0, General.Instance.Borders.BorderDetails.WidthTop, 0, 0);
                                break;
                        }
                    }
                }
            }
        }

        private static Brush GetBrushForBorder(Color colorMaster, Color colorBranch, General options, BorderLocation borderLocation)
        {
            Brush brush = null;
            if (options.Coloration == Coloration.Unitary)
            {
                brush = new SolidColorBrush(colorMaster);
            }
            else if (options.Coloration == Coloration.Branch)
            {
                brush = new SolidColorBrush(colorBranch);
            }
            else if (options.Coloration == Coloration.Combined)
            {
                if (options.GradientBorders == Gradient.RadialGradient)
                {
                    //brush = new RadialGradientBrush(colorBranch, colorMaster);
                    GradientStopCollection gradientStopCollection = new()
                    {
                        new GradientStop() { Color = colorBranch, Offset = 0 },
                        new GradientStop() { Color = colorBranch, Offset = 0.75 },
                        new GradientStop() { Color = colorMaster, Offset = 1 }
                    };
                    brush = new RadialGradientBrush(gradientStopCollection);
                }
                else if (options.GradientBorders == Gradient.LinearGradient)
                {
                    if (borderLocation == BorderLocation.Bottom)
                    {
                        brush = new LinearGradientBrush(colorBranch, colorMaster, new Point(0.3, 0), new Point(0.7, 0));
                    }
                    if (borderLocation == BorderLocation.Left)
                    {
                        brush = new LinearGradientBrush(colorMaster, colorBranch, new Point(0, 0.3), new Point(0, 0.7));
                    }
                    if (borderLocation == BorderLocation.Right)
                    {
                        brush = new LinearGradientBrush(colorBranch, colorMaster, new Point(0, 0.3), new Point(0, 0.7));
                    }
                    if (borderLocation == BorderLocation.Top)
                    {
                        brush = new LinearGradientBrush(colorMaster, colorBranch, new Point(0.3, 0), new Point(0.7, 0));
                    }
                }
            }
            return brush;
        }

        private static void ShowInTitleBar(Color colorMaster, Color colorBranch, General options)
        {
            Brush brush = GetBrushForTitlebar(colorMaster, colorBranch, options);

            if (_solutionLabel == null)
            {
                _solutionLabel = Application.Current.MainWindow.FindChild<Border>("PART_SolutionNameTextBlock").Parent as Border;
                _originalLabelColor = _solutionLabel?.Background;
            }

            if (_solutionLabel != null)
            {
                _solutionLabelForegroundProperty ??= _solutionLabel.Child.GetType().GetProperty("Foreground", BindingFlags.Public | BindingFlags.Instance);
                _originalLabelForeground ??= _solutionLabelForegroundProperty?.GetValue(_solutionLabel.Child) as SolidColorBrush;

                if (_solutionLabelForegroundProperty != null)
                {
                    _solutionLabel.Background = brush;
                    ContrastComparisonResult contrast = ContrastComparisonResult.ContrastHigherWithBlack;
                    if (options.Coloration == Coloration.Unitary || options.Coloration == Coloration.Combined)
                    {
                        contrast = ColorUtilities.CompareContrastWithBlackAndWhite(colorMaster);
                    }
                    else if (options.Coloration == Coloration.Branch)
                    {
                        contrast = ColorUtilities.CompareContrastWithBlackAndWhite(colorBranch);
                    }
                    _solutionLabelForegroundProperty.SetValue(_solutionLabel.Child, contrast == ContrastComparisonResult.ContrastHigherWithWhite ? Brushes.White : Brushes.Black);
                }
            }
        }

        private static Brush GetBrushForTitlebar(Color colorMaster, Color colorBranch, General options)
        {
            Brush brush = null;
            if (options.Coloration == Coloration.Unitary)
            {
                brush = new SolidColorBrush(colorMaster);
            }
            else if (options.Coloration == Coloration.Branch)
            {
                brush = new SolidColorBrush(colorBranch);
            }
            else if (options.Coloration == Coloration.Combined)
            {
                if (options.UseGradientTitlebar == false)
                {
                    if (options.BaseColor == BaseColor.MasterColor)
                    {
                        brush = new SolidColorBrush(colorMaster);
                    }
                    else
                    {
                        brush = new SolidColorBrush(colorBranch);
                    }
                }
                else
                {
                    brush = new LinearGradientBrush(colorMaster, colorBranch, new Point(0.8, 0), new Point(0.9, 0));
                }
            }
            return brush;
        }

        private static void ResetTaskbar()
        {
            foreach (Window window in Application.Current.Windows)
            {
                window.TaskbarItemInfo ??= new();
                window.TaskbarItemInfo.ThumbButtonInfos ??= new ThumbButtonInfoCollection();
                window.TaskbarItemInfo.ThumbButtonInfos.Clear();
                window.TaskbarItemInfo.Overlay = null;
            }
        }

        private static string GetControlName(BorderLocation location) => location switch
        {
            BorderLocation.Left => "LeftDockBorder",
            BorderLocation.Right => "RightDockBorder",
            BorderLocation.Top => "MainWindowTitleBar",
            _ => "BottomDockBorder",
        };



        private static List<ColorEntry> _colorEntries = null;

        public class ColorEntry
        {
            public string branch { get; set; }
            public string color { get; set; }
        }
    }
}
