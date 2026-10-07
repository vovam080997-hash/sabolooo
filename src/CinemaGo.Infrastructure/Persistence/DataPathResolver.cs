namespace CinemaGo.Infrastructure.Persistence;


public static class DataPathResolver
{
    public static string ResolveDataDir()
    {
        try
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (dir.GetFiles("*.sln").Length > 0)
                    return Path.Combine(dir.FullName, "Data");
                dir = dir.Parent;
            }
        }
        catch
        {
            
        }
        return Path.Combine(AppContext.BaseDirectory, "Data");
    }
}
