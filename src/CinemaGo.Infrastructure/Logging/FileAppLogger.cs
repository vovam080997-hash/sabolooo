using CinemaGo.Application.Abstractions;
using CinemaGo.Infrastructure.Persistence;

namespace CinemaGo.Infrastructure.Logging;


public class FileAppLogger : IAppLogger
{
    private readonly string logPath = Path.Combine(DataPathResolver.ResolveDataDir(), "log.txt");

    public void Log(string message)
    {
        try
        {
            var dir = Path.GetDirectoryName(logPath);
            if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            File.AppendAllLines(logPath, new[] { line });
        }
        catch
        {
            
        }
    }

    public List<string> ReadRecent(int count)
    {
        try
        {
            if (!File.Exists(logPath)) return new List<string>();
            return File.ReadAllLines(logPath).TakeLast(count).ToList();
        }
        catch
        {
            return new List<string>();
        }
    }
}
