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
        private static Border _solutionLabel;
        private static Brush _originalLabelColor;
        private static SolidColorBrush _originalLabelForeground;
        private static PropertyInfo _solutionLabelForegroundProperty;
        private static FileSystemWatcher _colorFileWatcher;
        private static DateTime _lastColorFileChange = DateTime.MinValue;

        // Cached border controls to avoid repeated visual tree traversal
        private static readonly Dictionary<BorderLocation, Border> _borderCache = [];

        //INFO:
        //colorMaster: the color of master branch
        //colorBranch: the color of branch (= master branch color when unitary coloration)

        public static async Task ResetInstanceAsync()
        {
            _colorEntries = null;
            _borderCache.Clear();
            DisposeFileWatcher();
            await RemoveUIAsync();
        }

        public static async Task SetColorAsync(string colorName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            General options = await General.GetLiveInstanceAsync();

            if (_colorEntries == null)
            {
                await LoadColorsAsync();
            }

            string branch = options.Coloration == Coloration.Unitary
                ? GitHelper.DefaultBranch
                : await GitHelper.GetBranchNameAsync();

            ColorEntry colorEntry = _colorEntries.FirstOrDefault(x => x.Branch == branch);
            if (colorEntry != null)
            {
                colorEntry.Color = colorName;
            }
            else
            {
                _colorEntries.Add(new ColorEntry { Branch = branch, Color = colorName });
            }

            string fileContent = string.Join(Environment.NewLine, _colorEntries.Select(e => e.ToString()));
            string fileName = await GetFileNameAsync();

            if (!string.IsNullOrEmpty(fileName))
            {
                File.WriteAllText(fileName, fileContent);
            }
        }

        public static async Task<bool> SolutionHasCustomColorAsync()
        {
            return File.Exists(await GetFileNameAsync());
        }

        public static async Task<string> GetColorAsync(string branch)
        {
            if (_colorEntries == null)
            {
                await LoadColorsAsync();
            }

            ColorEntry colorEntry = _colorEntries.FirstOrDefault(x => x.Branch == branch);
            if (colorEntry != null)
            {
                return colorEntry.Color;
            }
            else
            {
                if (branch == GitHelper.DefaultBranch)
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

        /// <summary>
        /// Attempts to colorize the UI with retry logic for when UI elements aren't ready.
        /// </summary>
        /// <returns>True if colorization was successful, false if UI elements weren't found after retries.</returns>
        public static async Task<bool> ColorizeWithRetryAsync(int maxRetries = 3, int delayMs = 500)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                await SetUiColorAsync();

                // Check if at least the main window borders were found
                if (Application.Current?.MainWindow != null)
                {
                    Border bottomBorder = Application.Current.MainWindow.FindChild<Border>("BottomDockBorder");
                    if (bottomBorder != null)
                    {
                        return true;
                    }
                }

                if (i < maxRetries - 1)
                {
                    await Task.Delay(delayMs);
                }
            }

            return false;
        }

        public static async Task ApplyIconAsync()
        {
            await SetIconAsync();
        }

        public static async Task LoadColorsAsync()
        {
            _colorEntries ??= [];

            string fileName = await GetFileNameAsync();

            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                try
                {
                    string fileContent = File.ReadAllText(fileName);
                    string[] lines = fileContent.Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries);

                    foreach (string line in lines)
                    {
                        ColorEntry entry = ColorEntry.Parse(line);
                        if (entry != null)
                        {
                            _colorEntries.Add(entry);
                        }
                    }

                    // Set up file watcher for auto-reload (Issue #45)
                    SetupFileWatcher(fileName);
                }
                catch (IOException)
                {
                    // File could not be read - continue with empty color entries
                }
                catch (UnauthorizedAccessException)
                {
                    // File access denied - continue with empty color entries
                }
            }
        }

#pragma warning disable VSTHRD100 // Avoid async void methods
        private static async void WindowEvents_WindowActivated(EnvDTE.Window GotFocus, EnvDTE.Window LostFocus)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            try
            {
                await SetUiColorAsync();
            }
            catch (Exception ex)
            {
                // Log but don't crash - this is an async void event handler
                await ex.LogAsync();
            }
        }

        public static async Task RemoveUIAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            DTE2 env = (DTE2)await ServiceProvider.GetGlobalServiceAsync(typeof(SDTE));
            if (env != null)
            {
                env.Events.WindowEvents.WindowActivated -= WindowEvents_WindowActivated;
            }

            if (VsShellUtilities.ShellIsShuttingDown)
            {
                return;
            }

            // Use cached borders if available, otherwise find them
            foreach (BorderLocation location in Enum.GetValues(typeof(BorderLocation)))
            {
                if (!_borderCache.TryGetValue(location, out Border border))
                {
                    string controlName = GetControlName(location);
                    border = Application.Current.MainWindow?.FindChild<Border>(controlName);
                }

                if (border != null)
                {
                    border.BorderThickness = new Thickness(0);
                    // Restore hit testing when removing borders (Issue #55)
                    // Skip Top border - we never disable hit testing on MainWindowTitleBar
                    if (location != BorderLocation.Top)
                    {
                        border.IsHitTestVisible = true;
                    }
                }
            }

            // Clear the cache since we're removing UI
            _borderCache.Clear();

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
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                DTE2 env = (EnvDTE80.DTE2)await ServiceProvider.GetGlobalServiceAsync(typeof(SDTE));
                if (env != null)
                {
                    env.Events.WindowEvents.WindowActivated += WindowEvents_WindowActivated;
                }
            }
        }

        public static async Task<string> GetFileNameAsync(bool isColor = true)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Solution solution = await VS.Solutions.GetCurrentSolutionAsync();
            string rootDir = solution?.GetRootDirectory();

            if (rootDir == null)
            {
                return null;
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
                    FileConstants.VsSettingsFolder,
                    Path.GetFileNameWithoutExtension(await solution.GetSolutionNameAsync()));

                if (!Directory.Exists(vsDir))
                {
                    DirectoryInfo di = Directory.CreateDirectory(vsDir);
                    di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                }
            }

            return Path.Combine(vsDir, isColor ? FileConstants.ColorFileName : FileConstants.IconFileName);
        }

        private static async Task SetIconAsync()
        {
            General options = await General.GetLiveInstanceAsync();

            if (options.ShowTaskBarThumbnails != Options.TaskBarOptions.None || options.ShowTaskBarOverlay)
            {
                await ShowInTaskBarAsync(Colors.Transparent, Colors.Transparent, options);
            }
        }

        private static async Task SetUiColorAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            string currentBranch = await GitHelper.GetBranchNameAsync();

            General options = await General.GetLiveInstanceAsync();

            // Cache this result to avoid multiple file system checks
            bool hasCustomColor = await SolutionHasCustomColorAsync();
            string masterColor = await GetColorAsync(GitHelper.DefaultBranch);

            if (string.IsNullOrEmpty(masterColor) && options.AutoMode == false)
            {
                await RemoveUIAsync();
                return;
            }

            Color colorMaster;
            if (options.AutoMode == true && !hasCustomColor)
            {
                Solution sol = await VS.Solutions.GetCurrentSolutionAsync();
                if (sol?.FullPath != null)
                {
                    string path = await sol.GetSolutionPathAsync();
                    if (!ColorCache.TryParseColor(ColorCache.GetColor(path), out colorMaster))
                    {
                        colorMaster = Colors.Black;
                    }
                }
                else
                {
                    colorMaster = Colors.Black;
                }
            }
            else
            {
                string masterColorCode = ColorCache.GetColorCode(masterColor);
                if (!ColorCache.TryParseColor(masterColorCode, out colorMaster))
                {
                    colorMaster = Colors.Black;
                }
            }

            Color colorBranch;
            if (currentBranch == GitHelper.DefaultBranch)
            {
                colorBranch = colorMaster;
            }
            else
            {
                if (options.AutoMode == true && !hasCustomColor)
                {
                    Solution sol = await VS.Solutions.GetCurrentSolutionAsync();
                    if (sol?.FullPath != null)
                    {
                        string path = await sol.GetSolutionPathAsync();
                        if (!ColorCache.TryParseColor(ColorCache.GetColor(path + currentBranch), out colorBranch))
                        {
                            colorBranch = Colors.Black;
                        }
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
                        string branchColorCode = ColorCache.GetColorCode(colorNameBranch);
                        if (!ColorCache.TryParseColor(branchColorCode, out colorBranch))
                        {
                            colorBranch = Colors.Black;
                        }
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
                await ShowInTaskBarAsync(colorMaster, colorBranch, options);
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

        private static async Task ShowInTaskBarAsync(Color colorMaster, Color colorBranch, General options)
        {
            ResetTaskbar();

            Brush brush = GetBrushForTaskbar(colorMaster, colorBranch, options);

            // Only add thumbnail buttons if the option is enabled (Issue #46)
            if (options.ShowTaskBarThumbnails != Options.TaskBarOptions.None)
            {
                ImageSource thumbImage = await brush.GetImageSourceAsync(16);

                switch (options.ShowTaskBarThumbnails)
                {
                    case Options.TaskBarOptions.MainWindowOnly:
                        EnsureTaskbarItemInfo(Application.Current.MainWindow);
                        Application.Current.MainWindow?.TaskbarItemInfo?.ThumbButtonInfos?.Add(
                            new ThumbButtonInfo { ImageSource = thumbImage, IsBackgroundVisible = false, IsInteractive = false });
                        break;

                    case Options.TaskBarOptions.AllWindows:
                        foreach (Window window in Application.Current.Windows)
                        {
                            EnsureTaskbarItemInfo(window);
                            window?.TaskbarItemInfo?.ThumbButtonInfos?.Add(
                                new ThumbButtonInfo { ImageSource = thumbImage, IsBackgroundVisible = false, IsInteractive = false });
                        }
                        break;
                }
            }

            // Apply overlay separately (Issue #37 - ensure TaskbarItemInfo exists)
            if (options.ShowTaskBarOverlay)
            {
                ImageSource overlayImage = await brush.GetImageSourceAsync(12);
                foreach (Window window in Application.Current.Windows)
                {
                    EnsureTaskbarItemInfo(window);
                    if (window?.TaskbarItemInfo != null)
                    {
                        window.TaskbarItemInfo.Overlay = overlayImage;
                    }
                }
            }
        }

        private static void EnsureTaskbarItemInfo(Window window)
        {
            if (window == null)
            {
                return;
            }

            window.TaskbarItemInfo ??= new TaskbarItemInfo();
            window.TaskbarItemInfo.ThumbButtonInfos ??= [];
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
                BorderLocation location = (BorderLocation)value;

                // Use cached border or find and cache it
                if (!_borderCache.TryGetValue(location, out Border border))
                {
                    string controlName = GetControlName(location);
                    border = Application.Current.MainWindow?.FindChild<Border>(controlName);
                    if (border != null)
                    {
                        _borderCache[location] = border;
                    }
                }

                if (border != null)
                {
                    if (options.Borders.BorderDetails.Locations.HasFlag(value))
                    {
                        // Enable border
                        border.BorderBrush = GetBrushForBorder(colorMaster, colorBranch, options, location);

                        // Prevent borders from stealing mouse clicks (Issue #23)
                        // But NOT for Top border - MainWindowTitleBar contains the menu bar,
                        // and disabling hit testing would block menu interaction (Issue #55)
                        if (location != BorderLocation.Top)
                        {
                            border.IsHitTestVisible = false;
                        }

                        switch (location)
                        {
                            case BorderLocation.Bottom:
                                border.BorderThickness = new Thickness(0, General.Instance.Borders.BorderDetails.WidthBottom, 0, 0);
                                break;
                            case BorderLocation.Left:
                                border.BorderThickness = new Thickness(General.Instance.Borders.BorderDetails.WidthLeft, 0, 0, 0);
                                break;
                            case BorderLocation.Right:
                                border.BorderThickness = new Thickness(General.Instance.Borders.BorderDetails.WidthRight, 0, 0, 0);
                                break;
                            case BorderLocation.Top:
                                border.BorderThickness = new Thickness(0, General.Instance.Borders.BorderDetails.WidthTop, 0, 0);
                                break;
                        }
                    }
                    else
                    {
                        // Disable border - reset thickness and restore hit testing (Issue #55)
                        border.BorderThickness = new Thickness(0);
                        if (location != BorderLocation.Top)
                        {
                            border.IsHitTestVisible = true;
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
                    GradientStopCollection gradientStopCollection =
                    [
                        new GradientStop() { Color = colorBranch, Offset = 0 },
                        new GradientStop() { Color = colorBranch, Offset = 0.75 },
                        new GradientStop() { Color = colorMaster, Offset = 1 }
                    ];
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

            // Retry finding the solution label if it wasn't found before (Issue #47)
            if (_solutionLabel == null)
            {
                Border partSolutionNameTextBlock = Application.Current.MainWindow?.FindChild<Border>("PART_SolutionNameTextBlock");
                _solutionLabel = partSolutionNameTextBlock?.Parent as Border;
                _originalLabelColor = _solutionLabel?.Background;
            }

            if (_solutionLabel != null)
            {
                _solutionLabelForegroundProperty ??= _solutionLabel.Child?.GetType().GetProperty("Foreground", BindingFlags.Public | BindingFlags.Instance);
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
                if (window == null)
                {
                    continue;
                }

                // Only reset if TaskbarItemInfo already exists - don't create new ones
                // This prevents empty thumbnail space when feature is disabled (Issue #46)
                if (window.TaskbarItemInfo != null)
                {
                    window.TaskbarItemInfo.ThumbButtonInfos?.Clear();
                    window.TaskbarItemInfo.Overlay = null;
                }
            }
        }

        private static string GetControlName(BorderLocation location) => location switch
        {
            BorderLocation.Left => "LeftDockBorder",
            BorderLocation.Right => "RightDockBorder",
            BorderLocation.Top => "MainWindowTitleBar",
            _ => "BottomDockBorder",
        };

        /// <summary>
        /// Sets up a FileSystemWatcher to monitor color.txt for external changes.
        /// </summary>
        private static void SetupFileWatcher(string filePath)
        {
            DisposeFileWatcher();

            try
            {
                string directory = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileName(filePath);

                _colorFileWatcher = new FileSystemWatcher(directory, fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                    EnableRaisingEvents = true
                };

                _colorFileWatcher.Changed += OnColorFileChanged;
            }
            catch (Exception ex)
            {
                // If we can't set up the watcher, just continue without it
                System.Diagnostics.Debug.WriteLine($"Failed to set up color file watcher: {ex.Message}");
            }
        }

        /// <summary>
        /// Disposes the FileSystemWatcher.
        /// </summary>
        private static void DisposeFileWatcher()
        {
            if (_colorFileWatcher != null)
            {
                _colorFileWatcher.EnableRaisingEvents = false;
                _colorFileWatcher.Changed -= OnColorFileChanged;
                _colorFileWatcher.Dispose();
                _colorFileWatcher = null;
            }
        }

#pragma warning disable VSTHRD100 // Avoid async void methods
        private static async void OnColorFileChanged(object sender, FileSystemEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            try
            {
                // Debounce: FileSystemWatcher can fire multiple events for a single change
                DateTime now = DateTime.Now;
                if ((now - _lastColorFileChange).TotalMilliseconds < 500)
                {
                    return;
                }
                _lastColorFileChange = now;

                // Small delay to ensure file is fully written
                await Task.Delay(100);

                // Reload colors and re-colorize
                _colorEntries = null;
                await ColorizeAsync();
            }
            catch (Exception ex)
            {
                // Log but don't crash - this is an async void event handler
                await ex.LogAsync();
            }
        }

        private static List<ColorEntry> _colorEntries = null;
    }
}
