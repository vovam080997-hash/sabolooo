using CinemaGo.Application.Services;
using CinemaGo.ConsoleApp.UI;
using CinemaGo.Domain.Entities;

namespace CinemaGo.ConsoleApp.Menus;


public class CustomerMenu
{
    private readonly AuthService authService;
    private readonly CatalogService catalogService;
    private readonly BookingService bookingService;

    public CustomerMenu(AuthService authService, CatalogService catalogService, BookingService bookingService)
    {
        this.authService = authService;
        this.catalogService = catalogService;
        this.bookingService = bookingService;
    }

    public void Run(Customer customer)
    {
        bool exit = false;
        while (!exit)
        {
            ConsoleUI.MenuTitle($"CUSTOMER MENU ({customer.Username})");
            ConsoleUI.MenuOption("1", "Browse movies & sessions (filter)");
            ConsoleUI.MenuOption("2", "View hall seating");
            ConsoleUI.MenuOption("3", "Book a ticket");
            ConsoleUI.MenuOption("4", "My bookings");
            ConsoleUI.MenuOption("5", "Cancel a booking");
            ConsoleUI.MenuOption("6", "Change password");
            ConsoleUI.MenuOption("7", "Log out");
            Console.Write("Choose an option: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": BrowseMoviesAndSessions(); break;
                case "2": ViewHall(); break;
                case "3": BookTicket(customer); break;
                case "4": ViewMyBookings(customer); break;
                case "5": CancelBooking(customer); break;
                case "6": ChangePassword(customer); break;
                case "7": exit = true; break;
                default: ConsoleUI.Error("Invalid choice."); break;
            }
        }
    }

    public void BrowseMoviesAndSessions()
    {
        ConsoleUI.Header("Movies & sessions");
        Console.Write("Filter by? (genre/hall/date) [Enter = show all]: ");
        string choice = Console.ReadLine()?.Trim().ToLower() ?? "";

        string? genre = null, hallId = null;
        DateTime? date = null;

        if (choice == "genre") { Console.Write("Genre: "); genre = Console.ReadLine()?.Trim(); }
        else if (choice == "hall") { Console.Write("Hall ID: "); hallId = Console.ReadLine()?.Trim(); }
        else if (choice == "date")
        {
            Console.Write("Date (yyyy-MM-dd): ");
            if (DateTime.TryParse(Console.ReadLine()?.Trim(), out var d)) date = d;
        }

        var list = catalogService.GetActiveSessions(genre, hallId, date);
        if (!list.Any()) { ConsoleUI.Warning("No active sessions found."); return; }

        foreach (var item in list)
        {
            Console.WriteLine(
                $"[{item.Movie.MovieId}] {item.Movie.Title} | Genre: {item.Movie.Genre} | " +
                $"Duration: {item.Movie.DurationMinutes} min | Age rating: {item.Movie.AgeRating} | " +
                $"[{item.Session.SessionId}] Hall: {item.Session.HallId} | Time: {item.Session.StartDateTime:yyyy-MM-dd HH:mm} | " +
                $"Standard: {item.Session.StandardPrice} GEL | VIP: {item.Session.VipPrice} GEL | Free seats: {item.FreeSeats}");
        }
    }

    public void ViewHall()
    {
        ConsoleUI.Header("Hall seating");
        Console.Write("Session ID: ");
        string sessionId = Console.ReadLine()?.Trim() ?? "";
        var session = catalogService.GetSession(sessionId);
        if (session == null) { ConsoleUI.Error("Session not found."); return; }
        var hall = bookingService.GetHall(session.HallId);
        if (hall == null) { ConsoleUI.Error("Hall not found."); return; }

        var seatMap = bookingService.GetSeatMap(session, hall);
        HallMapPrinter.Print(hall, seatMap);
    }

    private void BookTicket(Customer customer)
    {
        ConsoleUI.Header("Book a ticket");
        Console.Write("Session ID: ");
        string sessionId = Console.ReadLine()?.Trim() ?? "";
        var session = bookingService.GetSession(sessionId);
        if (session == null) { ConsoleUI.Error("Session not found."); return; }
        var hall = bookingService.GetHall(session.HallId);
        if (hall == null) { ConsoleUI.Error("Hall not found."); return; }

        var seatMap = bookingService.GetSeatMap(session, hall);
        HallMapPrinter.Print(hall, seatMap);

        Console.Write("Row number: ");
        if (!int.TryParse(Console.ReadLine(), out int row)) { ConsoleUI.Error("Invalid row number."); return; }
        Console.Write("Seat number: ");
        if (!int.TryParse(Console.ReadLine(), out int seatNum)) { ConsoleUI.Error("Invalid seat number."); return; }

        Console.Write("Discount code, if you have one (Enter — skip): ");
        string? code = Console.ReadLine()?.Trim();

        var result = bookingService.BookTicket(customer, sessionId, row, seatNum, code);
        if (!result.Success || result.Value == null)
        {
            ConsoleUI.Error(result.Message);
            return;
        }

        Console.WriteLine();
        TicketPrinter.Print(result.Value);
        ConsoleUI.Success(result.Message);
    }

    private void ViewMyBookings(Customer customer)
    {
        ConsoleUI.Header("My bookings");
        var items = bookingService.GetMyBookings(customer.Id);
        if (!items.Any()) { ConsoleUI.Warning("You have no bookings."); return; }

        foreach (var item in items)
        {
            string movieTitle = item.Movie?.Title ?? "Unknown movie";
            string when = item.Session != null ? item.Session.StartDateTime.ToString("yyyy-MM-dd HH:mm") : "-";
            string line = $"[{item.Booking.BookingId}] {movieTitle} | {when} | Row {item.Booking.Row}, Seat {item.Booking.Seat} | " +
                          $"{item.Booking.TicketType} | {item.Booking.Price} GEL | Code: {item.Booking.TicketCode} | Status: {item.Booking.Status}";

            if (item.Booking.Status == BookingStatus.Cancelled) ConsoleUI.Muted(line);
            else Console.WriteLine(line);
        }
    }

    private void CancelBooking(Customer customer)
    {
        ConsoleUI.Header("Cancel a booking");
        Console.Write("Booking ID to cancel: ");
        string bookingId = Console.ReadLine()?.Trim() ?? "";
        var result = bookingService.CancelBooking(customer, bookingId);
        ConsoleUI.Result(result.Success, result.Message);
    }

    private void ChangePassword(Customer customer) => MenuShared.ChangePassword(authService, customer);
}
