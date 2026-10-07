using CinemaGo.Application.Abstractions;
using CinemaGo.Application.Models;
using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Services;


public class AdminService
{
    private readonly IUserRepository userRepository;
    private readonly IMovieRepository movieRepository;
    private readonly ISessionRepository sessionRepository;
    private readonly IBookingRepository bookingRepository;
    private readonly IHallRepository hallRepository;
    private readonly IAppLogger logger;
    private readonly IDataExporter exporter;
    private readonly SeatMapService seatMapService;

    public AdminService(IUserRepository userRepository, IMovieRepository movieRepository,
        ISessionRepository sessionRepository, IBookingRepository bookingRepository, IHallRepository hallRepository,
        IAppLogger logger, IDataExporter exporter, SeatMapService seatMapService)
    {
        this.userRepository = userRepository;
        this.movieRepository = movieRepository;
        this.sessionRepository = sessionRepository;
        this.bookingRepository = bookingRepository;
        this.hallRepository = hallRepository;
        this.logger = logger;
        this.exporter = exporter;
        this.seatMapService = seatMapService;
    }

    public List<User> ListUsers() => userRepository.GetAll();

    public OperationResult AddMovie(string movieId, string title, string genre, int durationMinutes, string ageRating)
    {
        var movies = movieRepository.GetAll();
        if (string.IsNullOrWhiteSpace(movieId) || movies.Any(m => m.MovieId.Equals(movieId, StringComparison.OrdinalIgnoreCase)))
            return OperationResult.Fail("Error: ID is empty or already taken.");
        if (durationMinutes <= 0)
            return OperationResult.Fail("Error: invalid duration.");

        movies.Add(new Movie(movieId, title, genre, durationMinutes, ageRating));
        movieRepository.SaveAll(movies);
        logger.Log($"Admin added a new movie: {movieId} - {title}");
        return OperationResult.Ok("Movie added successfully.");
    }

    public DeletionImpact PreviewDeleteMovie(string movieId)
    {
        var movies = movieRepository.GetAll();
        var movie = movies.FirstOrDefault(m => m.MovieId == movieId);
        if (movie == null) return new DeletionImpact { Found = false };

        var relatedSessions = sessionRepository.GetAll().Where(s => s.MovieId == movieId).ToList();
        int activeBookings = bookingRepository.GetAll()
            .Count(b => relatedSessions.Any(s => s.SessionId == b.SessionId) && b.Status == BookingStatus.Confirmed);

        return new DeletionImpact { Found = true, AffectedSessions = relatedSessions.Count, AffectedActiveBookings = activeBookings };
    }

    public OperationResult DeleteMovie(string movieId)
    {
        var movies = movieRepository.GetAll();
        var movie = movies.FirstOrDefault(m => m.MovieId == movieId);
        if (movie == null) return OperationResult.Fail("Movie not found.");

        var sessions = sessionRepository.GetAll();
        var relatedSessions = sessions.Where(s => s.MovieId == movieId).ToList();
        if (relatedSessions.Any())
        {
            var bookings = bookingRepository.GetAll();
            foreach (var s in relatedSessions)
                bookings.RemoveAll(b => b.SessionId == s.SessionId);
            bookingRepository.SaveAll(bookings);

            sessions.RemoveAll(s => s.MovieId == movieId);
            sessionRepository.SaveAll(sessions);
        }

        movies.Remove(movie);
        movieRepository.SaveAll(movies);
        logger.Log($"Admin deleted movie: {movieId} - {movie.Title} (together with its sessions and bookings).");
        return OperationResult.Ok("Movie deleted successfully.");
    }

    public OperationResult AddSession(string sessionId, string movieId, string hallId, DateTime start, decimal standardPrice, decimal vipPrice)
    {
        var sessions = sessionRepository.GetAll();
        if (string.IsNullOrWhiteSpace(sessionId) || sessions.Any(s => s.SessionId.Equals(sessionId, StringComparison.OrdinalIgnoreCase)))
            return OperationResult.Fail("Error: ID is empty or already taken.");
        if (movieRepository.GetAll().All(m => m.MovieId != movieId))
            return OperationResult.Fail("Error: no such movie exists.");
        if (hallRepository.GetById(hallId) == null)
            return OperationResult.Fail("Error: no such hall exists.");
        if (start < DateTime.Now)
            return OperationResult.Fail("Error: the date is in the past.");
        if (standardPrice < 0 || vipPrice < 0)
            return OperationResult.Fail("Error: invalid price.");

        sessions.Add(new Session(sessionId, movieId, hallId, start, standardPrice, vipPrice, SessionStatus.Active));
        sessionRepository.SaveAll(sessions);
        logger.Log($"Admin added a new session: {sessionId} ({movieId}, {hallId}, {start:yyyy-MM-dd HH:mm})");
        return OperationResult.Ok("Session added successfully.");
    }

    public DeletionImpact PreviewDeleteSession(string sessionId)
    {
        var session = sessionRepository.GetAll().FirstOrDefault(s => s.SessionId == sessionId);
        if (session == null) return new DeletionImpact { Found = false };

        int activeBookings = bookingRepository.GetAll().Count(b => b.SessionId == sessionId && b.Status == BookingStatus.Confirmed);
        return new DeletionImpact { Found = true, AffectedSessions = 1, AffectedActiveBookings = activeBookings };
    }

    public OperationResult DeleteSession(string sessionId)
    {
        var sessions = sessionRepository.GetAll();
        var session = sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session == null) return OperationResult.Fail("Session not found.");

        var bookings = bookingRepository.GetAll();
        var activeBookings = bookings.Where(b => b.SessionId == sessionId && b.Status == BookingStatus.Confirmed).ToList();
        foreach (var b in activeBookings)
        {
            b.Cancel();
            logger.Log($"Booking {b.BookingId} automatically cancelled due to deletion of session {sessionId}.");
        }
        if (activeBookings.Any()) bookingRepository.SaveAll(bookings);

        sessions.Remove(session);
        sessionRepository.SaveAll(sessions);
        logger.Log($"Admin deleted session: {sessionId}.");
        return OperationResult.Ok("Session deleted successfully.");
    }

    public OperationResult<Session> EditSessionPrice(string sessionId, decimal? newStandardPrice, decimal? newVipPrice)
    {
        var sessions = sessionRepository.GetAll();
        var session = sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session == null) return OperationResult<Session>.Fail("Session not found.");

        decimal standard = newStandardPrice ?? session.StandardPrice;
        decimal vip = newVipPrice ?? session.VipPrice;
        if (standard < 0 || vip < 0) return OperationResult<Session>.Fail("Error: invalid price.");

        session.UpdatePrices(standard, vip);
        sessionRepository.SaveAll(sessions);
        logger.Log($"Admin changed prices for session {sessionId} — Standard: {standard} GEL, VIP: {vip} GEL.");
        return OperationResult<Session>.Ok(session, "Price updated successfully. Already issued tickets keep their original price.");
    }

    public StatisticsReport GetStatistics()
    {
        var bookings = bookingRepository.GetAll();
        var sessions = sessionRepository.GetAll();
        var movies = movieRepository.GetAll();

        var popular = bookings
            .Where(b => b.Status == BookingStatus.Confirmed)
            .Join(sessions, b => b.SessionId, s => s.SessionId, (b, s) => s.MovieId)
            .GroupBy(movieId => movieId)
            .Select(g => new MovieBookingCount
            {
                MovieTitle = movies.FirstOrDefault(m => m.MovieId == g.Key)?.Title ?? g.Key,
                BookingCount = g.Count()
            })
            .OrderByDescending(x => x.BookingCount)
            .ToList();

        var freeSeats = sessions
            .Where(s => s.Status == SessionStatus.Active)
            .Select(s =>
            {
                var hall = hallRepository.GetById(s.HallId);
                int free = hall == null ? 0 : seatMapService.CountFreeSeats(s, hall);
                return new SessionFreeSeats { SessionId = s.SessionId, FreeSeats = free, TotalSeats = hall?.TotalSeats ?? 0 };
            })
            .ToList();

        var byGenre = sessions
            .Join(movies, s => s.MovieId, m => m.MovieId, (s, m) => m.Genre)
            .GroupBy(g => g)
            .Select(g => new GenreSessionCount { Genre = g.Key, SessionCount = g.Count() })
            .ToList();

        return new StatisticsReport { PopularMovies = popular, FreeSeatsPerSession = freeSeats, SessionsByGenre = byGenre };
    }

    public void ExportAll()
    {
        var users = userRepository.GetAll();
        var movies = movieRepository.GetAll();
        var sessions = sessionRepository.GetAll();
        var bookings = bookingRepository.GetAll();

        exporter.ExportCsv("Users_export.csv", "ID,Username,Email,Role",
            users.Select(u => $"{u.Id},{u.Username},{u.Email},{u.Role}"));

        exporter.ExportCsv("Movies_export.csv", "MovieID,Title,Genre,DurationMinutes,AgeRating",
            movies.Select(m => $"{m.MovieId},{m.Title},{m.Genre},{m.DurationMinutes},{m.AgeRating}"));

        exporter.ExportCsv("Sessions_export.csv", "SessionID,MovieID,HallID,StartDateTime,StandardPrice,VipPrice,Status",
            sessions.Select(s => $"{s.SessionId},{s.MovieId},{s.HallId},{s.StartDateTime:yyyy-MM-dd HH:mm},{s.StandardPrice},{s.VipPrice},{s.Status}"));

        exporter.ExportCsv("Bookings_export.csv", "BookingID,UserID,SessionID,Row,Seat,TicketType,Price,TicketCode,Status",
            bookings.Select(b => $"{b.BookingId},{b.UserId},{b.SessionId},{b.Row},{b.Seat},{b.TicketType},{b.Price},{b.TicketCode},{b.Status}"));

        logger.Log("Admin performed a full CSV data export.");
    }

    public List<string> GetRecentLogs(int count) => logger.ReadRecent(count);
}
