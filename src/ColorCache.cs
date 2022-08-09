using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

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

        public static string GetColor(string solutionPath)
        {
            int hash = Math.Abs(solutionPath.GetHashCode());
            int mod = hash % ColorMap.Count;

            return ColorMap.Keys.ElementAt(mod - 1);
        }
    }
}
