namespace SolutionColors
{
    /// <summary>
    /// Represents a color assignment for a specific branch.
    /// </summary>
    public class ColorEntry
    {
        public string Branch { get; set; }
        public string Color { get; set; }

        public override string ToString()
        {
            return $"{Branch}:{Color}";
        }

        public static ColorEntry Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            string[] segments = line.Split(':');
            if (segments.Length == 1)
            {
                // Legacy format: just a color (assumed to be master branch)
                return new ColorEntry { Branch = GitHelper.DefaultBranch, Color = segments[0] };
            }
            else if (segments.Length >= 2)
            {
                return new ColorEntry { Branch = segments[0], Color = segments[1] };
            }

            return null;
        }
    }
}
