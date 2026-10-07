using CinemaGo.Application.Abstractions;

namespace CinemaGo.Infrastructure.Persistence;


internal static class TextFileHelper
{
    private static string PathOf(string fileName) => Path.Combine(DataPathResolver.ResolveDataDir(), fileName);

    public static List<string> ReadDataLines(string fileName, IAppLogger logger)
    {
        try
        {
            var dataDir = DataPathResolver.ResolveDataDir();
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

            var path = PathOf(fileName);
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "");
                logger.Log($"File {fileName} not found — created a new empty file.");
                return new List<string>();
            }

            return File.ReadAllLines(path)
                       .Where(l => !string.IsNullOrWhiteSpace(l) && !l.TrimStart().StartsWith("//"))
                       .ToList();
        }
        catch (Exception ex)
        {
            logger.Log($"File read error ({fileName}): {ex.Message}");
            return new List<string>();
        }
    }

    public static void WriteDataLines(string fileName, string header, IEnumerable<string> lines, IAppLogger logger)
    {
        try
        {
            var dataDir = DataPathResolver.ResolveDataDir();
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

            var content = new List<string> { header };
            content.AddRange(lines);
            File.WriteAllLines(PathOf(fileName), content);
        }
        catch (Exception ex)
        {
            logger.Log($"File write error ({fileName}): {ex.Message}");
        }
    }
}
