using System.Text.Json;

namespace CinemaGo.ConsoleApp.Configuration;

public static class AppSettingsLoader
{
    public static AppSettings Load()
    {
        string path = Path.Combine(FindSolutionRoot(), "appsettings.json");

        if (!File.Exists(path))
        {
            Console.WriteLine($"Note: appsettings.json not found at {path}.");
            Console.WriteLine("Copy appsettings.example.json to appsettings.json and fill in your SMTP details to enable real emails.");
            return new AppSettings();
        }

        try
        {
            string json = File.ReadAllText(path);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return settings ?? new AppSettings();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: could not read appsettings.json ({ex.Message}). Falling back to simulation mode.");
            return new AppSettings();
        }
    }

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (dir.GetFiles("*.sln").Length > 0) return dir.FullName;
            dir = dir.Parent;
        }
        return AppContext.BaseDirectory;
    }
}
