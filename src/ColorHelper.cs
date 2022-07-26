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
                SetBorderColor(null);
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
                    SetBorderColor(color);
                }
            }
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
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            SetBorderColor(null);
        }

        private static async Task<string> GetFileNameAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Solution solution = await VS.Solutions.GetCurrentSolutionAsync();
            string solutionDir = Path.GetDirectoryName(solution.FullPath);
            string vsDir = Path.Combine(solutionDir, ".vs");

            if (!Directory.Exists(vsDir))
            {
                DirectoryInfo di = Directory.CreateDirectory(vsDir);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }

            return Path.Combine(vsDir, "color.txt");
        }

        private static void SetBorderColor(Brush color)
        {
            if (color == null)
            {
                _border.BorderThickness = new Thickness(0);
            }
            else
            {
                _border ??= FindChild(Application.Current.MainWindow, "BottomDockBorder") as Border;
                _border.BorderBrush = color;
                _border.BorderThickness = new Thickness(0, General.Instance.Width, 0, 0);
            }
        }

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
