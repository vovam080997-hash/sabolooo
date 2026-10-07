using System.Globalization;
using CinemaGo.Application.Services;
using CinemaGo.ConsoleApp.UI;
using CinemaGo.Domain.Entities;

namespace CinemaGo.ConsoleApp.Menus;


public class AdminMenu
{
    private readonly AuthService authService;
    private readonly AdminService adminService;
    private readonly CatalogService catalogService;
    private readonly CustomerMenu sharedBrowse; 

    public AdminMenu(AuthService authService, AdminService adminService, CatalogService catalogService, CustomerMenu sharedBrowse)
    {
        this.authService = authService;
        this.adminService = adminService;
        this.catalogService = catalogService;
        this.sharedBrowse = sharedBrowse;
    }

    public void Run(Admin admin)
    {
        bool exit = false;
        while (!exit)
        {
            ConsoleUI.MenuTitle("ADMIN MENU");
            ConsoleUI.MenuOption("1", "List users");
            ConsoleUI.MenuOption("2", "Add movie");
            ConsoleUI.MenuOption("3", "Delete movie");
            ConsoleUI.MenuOption("4", "Add session");
            ConsoleUI.MenuOption("5", "Delete session");
            ConsoleUI.MenuOption("6", "Edit session price");
            ConsoleUI.MenuOption("7", "Browse movies & sessions (list/filter)");
            ConsoleUI.MenuOption("8", "Statistics (LINQ)");
            ConsoleUI.MenuOption("9", "Export data (CSV)");
            ConsoleUI.MenuOption("10", "View logs");
            ConsoleUI.MenuOption("11", "Change password");
            ConsoleUI.MenuOption("12", "Log out");
            Console.Write("Choose an option: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": ListUsers(); break;
                case "2": AddMovie(); break;
                case "3": DeleteMovie(); break;
                case "4": AddSession(); break;
                case "5": DeleteSession(); break;
                case "6": EditSessionPrice(); break;
                case "7": sharedBrowse.BrowseMoviesAndSessions(); break;
                case "8": ShowStatistics(); break;
                case "9": ExportData(); break;
                case "10": ShowLogs(); break;
                case "11": MenuShared.ChangePassword(authService, admin); break;
                case "12": exit = true; break;
                default: ConsoleUI.Error("Invalid choice."); break;
            }
        }
    }

    private void ListUsers()
    {
        ConsoleUI.Header("User list");
        foreach (var u in adminService.ListUsers())
            Console.WriteLine($"[{u.Id}] {u.Username} | {u.Email} | Role: {u.Role}");
    }

    private void AddMovie()
    {
        ConsoleUI.Header("Add movie");
        Console.Write("MovieID (e.g. M03): ");
        string id = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Title: ");
        string title = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Genre: ");
        string genre = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Duration (minutes): ");
        if (!int.TryParse(Console.ReadLine(), out int duration)) { ConsoleUI.Error("Error: invalid duration."); return; }
        Console.Write("Age rating (e.g. 12+): ");
        string age = Console.ReadLine()?.Trim() ?? "";

        var result = adminService.AddMovie(id, title, genre, duration, age);
        ConsoleUI.Result(result.Success, result.Message);
    }

    private void DeleteMovie()
    {
        ConsoleUI.Header("Delete movie");
        Console.Write("MovieID: ");
        string id = Console.ReadLine()?.Trim() ?? "";

        var impact = adminService.PreviewDeleteMovie(id);
        if (!impact.Found) { ConsoleUI.Error("Movie not found."); return; }

        if (impact.AffectedSessions > 0)
        {
            ConsoleUI.Warning($"This movie has {impact.AffectedSessions} session(s) (including {impact.AffectedActiveBookings} active booking(s)).");
            ConsoleUI.Warning("Deleting the movie will also delete all its sessions and bookings.");
            if (!ConsoleUI.Confirm("Are you sure you want to continue?")) { ConsoleUI.Info("Deletion cancelled."); return; }
        }

        var result = adminService.DeleteMovie(id);
        ConsoleUI.Result(result.Success, result.Message);
    }

    private void AddSession()
    {
        ConsoleUI.Header("Add session");
        Console.Write("SessionID (e.g. S102): ");
        string id = Console.ReadLine()?.Trim() ?? "";
        Console.Write("MovieID: ");
        string movieId = Console.ReadLine()?.Trim() ?? "";
        Console.Write("HallID (H1/H2): ");
        string hallId = Console.ReadLine()?.Trim() ?? "";

        Console.Write("Start date/time (yyyy-MM-dd HH:mm): ");
        if (!DateTime.TryParseExact(Console.ReadLine()?.Trim(), "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
        {
            ConsoleUI.Error("Error: invalid date format.");
            return;
        }

        Console.Write("Standard price: ");
        if (!decimal.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out var stdPrice))
        { ConsoleUI.Error("Error: invalid price."); return; }

        Console.Write("VIP price: ");
        if (!decimal.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out var vipPrice))
        { ConsoleUI.Error("Error: invalid price."); return; }

        var result = adminService.AddSession(id, movieId, hallId, start, stdPrice, vipPrice);
        ConsoleUI.Result(result.Success, result.Message);
    }

    private void DeleteSession()
    {
        ConsoleUI.Header("Delete session");
        Console.Write("SessionID: ");
        string id = Console.ReadLine()?.Trim() ?? "";

        var impact = adminService.PreviewDeleteSession(id);
        if (!impact.Found) { ConsoleUI.Error("Session not found."); return; }

        if (impact.AffectedActiveBookings > 0)
        {
            ConsoleUI.Warning($"This session has {impact.AffectedActiveBookings} active booking(s).");
            ConsoleUI.Warning("Deleting the session will automatically cancel these bookings.");
            if (!ConsoleUI.Confirm("Are you sure you want to continue?")) { ConsoleUI.Info("Deletion cancelled."); return; }
        }

        var result = adminService.DeleteSession(id);
        ConsoleUI.Result(result.Success, result.Message);
    }

    private void EditSessionPrice()
    {
        ConsoleUI.Header("Edit session price");
        Console.Write("SessionID: ");
        string id = Console.ReadLine()?.Trim() ?? "";
        var session = catalogService.GetSession(id);
        if (session == null) { ConsoleUI.Error("Session not found."); return; }

        ConsoleUI.Info($"Current prices — Standard: {session.StandardPrice} GEL | VIP: {session.VipPrice} GEL");

        Console.Write("New standard price (Enter — keep unchanged): ");
        string stdInput = Console.ReadLine()?.Trim() ?? "";
        decimal? newStandard = null;
        if (!string.IsNullOrEmpty(stdInput))
        {
            if (!decimal.TryParse(stdInput, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
            { ConsoleUI.Error("Error: invalid price. Change cancelled."); return; }
            newStandard = v;
        }

        Console.Write("New VIP price (Enter — keep unchanged): ");
        string vipInput = Console.ReadLine()?.Trim() ?? "";
        decimal? newVip = null;
        if (!string.IsNullOrEmpty(vipInput))
        {
            if (!decimal.TryParse(vipInput, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
            { ConsoleUI.Error("Error: invalid price. Change cancelled."); return; }
            newVip = v;
        }

        var result = adminService.EditSessionPrice(id, newStandard, newVip);
        ConsoleUI.Result(result.Success, result.Message);
    }

    private void ShowStatistics()
    {
        ConsoleUI.Header("Statistics (LINQ)");
        var report = adminService.GetStatistics();

        ConsoleUI.Info("Most popular movies (by number of bookings):");
        if (!report.PopularMovies.Any()) Console.WriteLine("  No bookings exist yet.");
        foreach (var p in report.PopularMovies)
            Console.WriteLine($"  {p.MovieTitle}: {p.BookingCount} booking(s)");

        Console.WriteLine();
        ConsoleUI.Info("Free seats on active sessions:");
        foreach (var s in report.FreeSeatsPerSession)
            Console.WriteLine($"  [{s.SessionId}] {s.FreeSeats} / {s.TotalSeats} free");

        Console.WriteLine();
        ConsoleUI.Info("Number of sessions by genre:");
        foreach (var g in report.SessionsByGenre)
            Console.WriteLine($"  {g.Genre}: {g.SessionCount} session(s)");
    }

    private void ExportData()
    {
        ConsoleUI.Header("Export data to CSV");
        adminService.ExportAll();
        ConsoleUI.Success("All data exported successfully (see the Data/ folder).");
    }

    private void ShowLogs()
    {
        ConsoleUI.Header("Recent logs");
        var logs = adminService.GetRecentLogs(30);
        if (!logs.Any()) { ConsoleUI.Warning("No logs yet."); return; }
        foreach (var line in logs) ConsoleUI.Muted(line);
    }
}
