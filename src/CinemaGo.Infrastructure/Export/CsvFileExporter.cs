using CinemaGo.Application.Abstractions;
using CinemaGo.Infrastructure.Persistence;

namespace CinemaGo.Infrastructure.Export;


public class CsvFileExporter : IDataExporter
{
    private readonly IAppLogger logger;

    public CsvFileExporter(IAppLogger logger) => this.logger = logger;

    public void ExportCsv(string fileName, string header, IEnumerable<string> lines)
    {
        try
        {
            var dataDir = DataPathResolver.ResolveDataDir();
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

            var content = new List<string> { header };
            content.AddRange(lines);
            File.WriteAllLines(Path.Combine(dataDir, fileName), content);

            logger.Log($"CSV export completed: {fileName}");
        }
        catch (Exception ex)
        {
            logger.Log($"CSV export error ({fileName}): {ex.Message}");
        }
    }
}
