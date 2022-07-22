using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EnvDTE;
using EnvDTE80;

namespace SolutionColors
{
    public class ColorHelper
    {
        private static Border _border;

        private static readonly Dictionary<string, string> _colorMap = new()
        {
            { "Blue", "CornflowerBlue" },
            { "Red", "OrangeRed" },
            { "Green", "OliveDrab" },
            { "Teal", "Teal" },
            { "Purple", "Purple" },
        };

        public static async Task SetColorAsync(string colorName)
        {
            if (!_colorMap.TryGetValue(colorName, out string trueColor))
            {
                return;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE2 dte = await VS.GetServiceAsync<DTE, DTE2>();

            string existingColor = await GetColorAsync();

            if (colorName != existingColor)
            {
                dte.Solution.Globals["color"] = colorName;
                dte.Solution.Globals.VariablePersists["color"] = true;
                dte.Solution.SaveAs(dte.Solution.FullName);
            }

            PropertyInfo property = typeof(Brushes).GetProperty(trueColor, BindingFlags.Static | BindingFlags.Public);

            if (property?.GetValue(null, null) is Brush color)
            {
                SetColor(color);
            }
        }

        public static async Task<string> GetColorAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE2 dte = await VS.GetServiceAsync<DTE, DTE2>();

            if (dte.Solution.Globals.VariableExists["color"])
            {
                return dte.Solution.Globals["color"] as string;
            }

            return null;
        }

        internal static async Task RemoveBorderAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            SetColor(null);
        }

        private static void SetColor(Brush color)
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
