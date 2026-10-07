namespace CinemaGo.Application.Abstractions;


public interface IDataExporter
{
    void ExportCsv(string fileName, string header, IEnumerable<string> lines);
}
