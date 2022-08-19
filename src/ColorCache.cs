using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace SolutionColors
{
    public static class ColorCache
    {
        private static readonly Dictionary<string, string> _colorTranslator = new()
        {
            { "Burgundy", "Tomato" },
            { "Pumpkin", "OrangeRed" },
            { "Volt", "YellowGreen" },
            { "Mint", "MediumAquamarine" },
            { "DarkBrown", "SaddleBrown" },
            { "Lavender", "MediumPurple" },
        };

        private static Dictionary<string, string> ColorMap { get; } = new();

        public static int GetIndex(string name)
        {
            return ColorMap.Keys.ToList().IndexOf(name);
        }

        public static string GetColorCode(string name)
        {
            if (ColorMap.ContainsKey(name))
            {
                return ColorMap[name];
            }

            object fromHex = ColorConverter.ConvertFromString(name);

            if (fromHex is Color)
            {
                return name;
            }

            return null;
        }

        public static void AddColor(string name)
        {
            if (_colorTranslator.TryGetValue(name, out string code))
            {
                ColorMap.Add(name, code);
            }
            else
            {
                ColorMap.Add(name, name);
            }
        }

        public static string GetColor(string filePath)
        {
            int hash = Math.Abs(filePath.GetHashCode());
            int mod = hash % ColorMap.Count;

            return ColorMap.Keys.ElementAt(mod);
        }
    }
}
