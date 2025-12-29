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

        private static Dictionary<string, string> ColorMap { get; } = [];
        
        // Cache for key list to avoid repeated ToList() allocations
        private static List<string> _keyListCache;
        private static int _keyListCacheVersion;
        private static int _colorMapVersion;

        public static int GetIndex(string name)
        {
            EnsureKeyListCache();
            return _keyListCache.IndexOf(name);
        }

        public static string GetColorCode(string name)
        {
            if (ColorMap.TryGetValue(name, out string colorCode))
            {
                return colorCode;
            }

            // Try to parse as hex color
            if (TryParseColor(name, out _))
            {
                return name;
            }

            return null;
        }

        /// <summary>
        /// Attempts to parse a color string (name or hex) to a Color.
        /// </summary>
        public static bool TryParseColor(string colorString, out Color color)
        {
            color = default;

            if (string.IsNullOrWhiteSpace(colorString))
            {
                return false;
            }

            try
            {
                object result = ColorConverter.ConvertFromString(colorString);
                if (result is Color parsedColor)
                {
                    color = parsedColor;
                    return true;
                }
            }
            catch (FormatException)
            {
                // Invalid color format
            }

            return false;
        }

        public static void AddColor(string name)
        {
            if (ColorMap.ContainsKey(name))
            {
                return; // Already added
            }

            if (_colorTranslator.TryGetValue(name, out string code))
            {
                ColorMap.Add(name, code);
            }
            else
            {
                ColorMap.Add(name, name);
            }
            
            // Invalidate cache when map changes
            _colorMapVersion++;
        }

        public static string GetColor(string filePath)
        {
            if (ColorMap.Count <= 1)
            {
                return Colors.Gray.ToString();
            }

            EnsureKeyListCache();
            
            int hash = Math.Abs(filePath.GetHashCode());
            int mod = hash % (ColorMap.Count - 1);    //last one is "None" which is not a valid color

            return ColorMap[_keyListCache[mod]];
        }
        
        private static void EnsureKeyListCache()
        {
            if (_keyListCache == null || _keyListCacheVersion != _colorMapVersion)
            {
                _keyListCache = [.. ColorMap.Keys];
                _keyListCacheVersion = _colorMapVersion;
            }
        }
    }
}
