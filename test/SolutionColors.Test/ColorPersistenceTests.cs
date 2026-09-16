namespace SolutionColors.Test;

[TestClass]
public class ColorPersistenceTests
{
    [TestMethod]
    public void ReadColorEntries_WithMixedLineEndings_ParsesValidEntries()
    {
        RunWithTempFile(
            "main:Tomato\r\nfeature:Gold\nlegacy-color\rmalformed:entry:ignored",
            fileName =>
            {
                List<ColorEntry> entries = ColorHelper.ReadColorEntries(fileName);

                Assert.HasCount(3, entries);
                AssertEntry(entries[0], "main", "Tomato");
                AssertEntry(entries[1], "feature", "Gold");
                AssertEntry(entries[2], GitHelper.DefaultBranch, "legacy-color");
            });
    }

    [TestMethod]
    public void ReadColorEntries_WithDuplicateBranch_KeepsFirstValue()
    {
        RunWithTempFile(
            "feature:Tomato\nfeature:Gold",
            fileName =>
            {
                List<ColorEntry> entries = ColorHelper.ReadColorEntries(fileName);

                Assert.HasCount(1, entries);
                AssertEntry(entries[0], "feature", "Tomato");
            });
    }

    [TestMethod]
    public void WriteAndReadColorEntries_RoundTripsAssignments()
    {
        RunWithTempFile(
            string.Empty,
            fileName =>
            {
                List<ColorEntry> expected =
                [
                    new() { Branch = "master", Color = "Tomato" },
                    new() { Branch = "feature/test", Color = string.Empty }
                ];

                ColorHelper.WriteColorEntries(fileName, expected);
                List<ColorEntry> actual = ColorHelper.ReadColorEntries(fileName);

                Assert.AreEqual($"master:Tomato{Environment.NewLine}feature/test:", File.ReadAllText(fileName));
                Assert.HasCount(2, actual);
                AssertEntry(actual[0], "master", "Tomato");
                AssertEntry(actual[1], "feature/test", string.Empty);
            });
    }

    [TestMethod]
    public void SetColor_WithExistingDuplicates_UpdatesAndDeduplicatesBranch()
    {
        List<ColorEntry> entries =
        [
            new() { Branch = "feature", Color = "Tomato" },
            new() { Branch = "master", Color = "Gold" },
            new() { Branch = "feature", Color = "Mint" }
        ];

        ColorHelper.SetColor(entries, "feature", "Blue");

        Assert.HasCount(2, entries);
        AssertEntry(entries[0], "feature", "Blue");
        AssertEntry(entries[1], "master", "Gold");
    }

    [TestMethod]
    public void SetColor_WithNewBranch_AddsAssignment()
    {
        List<ColorEntry> entries = [];

        ColorHelper.SetColor(entries, "feature", "Blue");

        Assert.HasCount(1, entries);
        AssertEntry(entries[0], "feature", "Blue");
    }

    private static void AssertEntry(ColorEntry entry, string branch, string color)
    {
        Assert.AreEqual(branch, entry.Branch);
        Assert.AreEqual(color, entry.Color);
    }

    private static void RunWithTempFile(string content, Action<string> test)
    {
        string directory = Path.Combine(Path.GetTempPath(), "SolutionColors.Tests", Guid.NewGuid().ToString("N"));
        string fileName = Path.Combine(directory, FileConstants.ColorFileName);
        Directory.CreateDirectory(directory);
        File.WriteAllText(fileName, content);

        try
        {
            test(fileName);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
