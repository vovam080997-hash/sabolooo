using CinemaGo.Application.Abstractions;
using CinemaGo.Application.Models;
using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Services;


public class BookingService
{
    private readonly ISessionRepository sessionRepository;
    private readonly IMovieRepository movieRepository;
    private readonly IBookingRepository bookingRepository;
    private readonly IEmailSender emailSender;
    private readonly IAppLogger logger;
    private readonly SeatMapService seatMapService;
    private readonly Random rng = new();

    private readonly Dictionary<string, decimal> discountCodes = new()
    {
        { "STUDENT10", 10m },
        { "PROMO20", 20m }
    };

    public BookingService(ISessionRepository sessionRepository, IMovieRepository movieRepository,
        IBookingRepository bookingRepository, IEmailSender emailSender, IAppLogger logger, SeatMapService seatMapService)
    {
        this.sessionRepository = sessionRepository;
        this.movieRepository = movieRepository;
        this.bookingRepository = bookingRepository;
        this.emailSender = emailSender;
        this.logger = logger;
        this.seatMapService = seatMapService;
    }

    public Session? GetSession(string sessionId) => sessionRepository.GetAll().FirstOrDefault(s => s.SessionId == sessionId);
    public Hall? GetHall(string hallId) => seatMapService.GetHall(hallId);
    public Movie? GetMovie(string movieId) => movieRepository.GetAll().FirstOrDefault(m => m.MovieId == movieId);
    public List<Seat> GetSeatMap(Session session, Hall hall) => seatMapService.BuildSeatMap(session, hall);


    public bool TryApplyDiscountCode(string? code, out decimal discountPercent)
    {
        discountPercent = 0;
        if (string.IsNullOrWhiteSpace(code)) return true; // no code entered — not an error
        return discountCodes.TryGetValue(code.Trim().ToUpperInvariant(), out discountPercent);
    }

    public OperationResult<BookingReceipt> BookTicket(Customer customer, string sessionId, int row, int seatNumber, string? discountCode)
    {
        var session = GetSession(sessionId);
        if (session == null) return OperationResult<BookingReceipt>.Fail("Session not found.");

        if (!session.IsBookable(DateTime.Now))
            return OperationResult<BookingReceipt>.Fail("This session cannot be booked (it is in the past or not active).");

        var hall = GetHall(session.HallId);
        if (hall == null) return OperationResult<BookingReceipt>.Fail("Hall not found.");

        var movie = GetMovie(session.MovieId);
        if (movie == null) return OperationResult<BookingReceipt>.Fail("Movie not found.");

        if (row < 1 || row > hall.Rows) return OperationResult<BookingReceipt>.Fail("Invalid row number.");
        if (seatNumber < 1 || seatNumber > hall.SeatsPerRow) return OperationResult<BookingReceipt>.Fail("Invalid seat number.");

        var seatMap = seatMapService.BuildSeatMap(session, hall);
        var seat = seatMap.First(s => s.Row == row && s.Number == seatNumber);
        if (seat.IsBooked) return OperationResult<BookingReceipt>.Fail("This seat is already booked.");

        bool codeValid = TryApplyDiscountCode(discountCode, out decimal discountPercent);
        if (!codeValid) discountPercent = 0; 

        bool isVip = seat.IsVip;
        decimal basePrice = isVip ? session.VipPrice : session.StandardPrice;

        string ticketCode = GenerateTicketCode();
        Ticket ticket = isVip ? new VipTicket(ticketCode) : new StandardTicket(ticketCode);
        decimal finalPrice = ticket.CalculatePrice(basePrice, discountPercent);

        seat.Book(); 

        var bookings = bookingRepository.GetAll();
        var booking = new Booking(
            GenerateBookingId(bookings), customer.Id, session.SessionId, row, seatNumber,
            isVip ? TicketTypeKind.VIP : TicketTypeKind.Standard, finalPrice, ticketCode, BookingStatus.Confirmed);

        bookings.Add(booking);
        bookingRepository.SaveAll(bookings);

        logger.Log($"New booking: user {customer.Username} (ID {customer.Id}), session {session.SessionId}, " +
                   $"seat [{row},{seatNumber}], type {booking.TicketType}, price {finalPrice} GEL, code {ticketCode}");

        emailSender.SendBookingConfirmation(
            customer.Email,
            "CinemaGo - Booking confirmation",
            $"Hello, {customer.Username}!\n\n" +
            $"Your booking has been confirmed.\n" +
            $"Movie: {movie.Title}\nHall: {hall.HallId}\nSeat: Row {row}, Seat {seatNumber}\n" +
            $"Showtime: {session.StartDateTime:yyyy-MM-dd HH:mm}\nPrice: {finalPrice} GEL\nTicket code: {ticketCode}\n\n" +
            $"Thank you for using CinemaGo!");

        var receipt = new BookingReceipt { Booking = booking, Ticket = ticket, Movie = movie, Session = session, Hall = hall };
        string message = codeValid && discountPercent > 0
            ? $"Booking confirmed with a {discountPercent}% discount applied."
            : discountCode is { Length: > 0 } && !codeValid
                ? "Booking confirmed. Note: the discount code was invalid, so no discount was applied."
                : "Booking confirmed.";

        return OperationResult<BookingReceipt>.Ok(receipt, message);
    }

    public List<BookingListItem> GetMyBookings(int userId)
    {
        var sessions = sessionRepository.GetAll();
        var movies = movieRepository.GetAll();

        return bookingRepository.GetAll()
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingId)
            .Select(b =>
            {
                var session = sessions.FirstOrDefault(s => s.SessionId == b.SessionId);
                var movie = session != null ? movies.FirstOrDefault(m => m.MovieId == session.MovieId) : null;
                return new BookingListItem { Booking = b, Session = session, Movie = movie };
            })
            .ToList();
    }

    public OperationResult CancelBooking(Customer customer, string bookingId)
    {
        var bookings = bookingRepository.GetAll();
        var booking = bookings.FirstOrDefault(b => b.BookingId.Equals(bookingId, StringComparison.OrdinalIgnoreCase));

        if (booking == null || booking.UserId != customer.Id)
            return OperationResult.Fail("No such booking found under your account.");

        if (booking.Status != BookingStatus.Confirmed)
            return OperationResult.Fail("This booking is already cancelled.");

        booking.Cancel();
        bookingRepository.SaveAll(bookings);
        logger.Log($"Booking cancelled: {booking.BookingId} (user {customer.Username}) — seat released.");
        return OperationResult.Ok("Booking successfully cancelled. The seat is free again.");
    }

    private string GenerateTicketCode()
    {
        var existing = bookingRepository.GetAll();
        string code;
        do
        {
            code = "CG-" + string.Concat(Enumerable.Range(0, 6)
                .Select(_ => "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"[rng.Next(32)]));
        } while (existing.Any(b => b.TicketCode == code));
        return code;
    }

    private static string GenerateBookingId(List<Booking> bookings)
    {
        int max = bookings.Count == 0
            ? 500
            : bookings.Select(b => int.TryParse(b.BookingId.TrimStart('B'), out int n) ? n : 500).Max();
        return $"B{max + 1}";
    }
}
