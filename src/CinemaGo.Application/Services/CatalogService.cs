using CinemaGo.Application.Abstractions;
using CinemaGo.Application.Models;
using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Services;


public class CatalogService
{
    private readonly ISessionRepository sessionRepository;
    private readonly IMovieRepository movieRepository;
    private readonly SeatMapService seatMapService;

    public CatalogService(ISessionRepository sessionRepository, IMovieRepository movieRepository, SeatMapService seatMapService)
    {
        this.sessionRepository = sessionRepository;
        this.movieRepository = movieRepository;
        this.seatMapService = seatMapService;
    }

    public List<SessionListItem> GetActiveSessions(string? genre = null, string? hallId = null, DateTime? date = null)
    {
        var sessions = sessionRepository.GetAll();
        var movies = movieRepository.GetAll();

        var query = from s in sessions
                    join m in movies on s.MovieId equals m.MovieId
                    where s.Status == SessionStatus.Active
                    select new { Session = s, Movie = m };

        if (!string.IsNullOrWhiteSpace(genre))
            query = query.Where(x => x.Movie.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(hallId))
            query = query.Where(x => x.Session.HallId.Equals(hallId, StringComparison.OrdinalIgnoreCase));
        if (date.HasValue)
            query = query.Where(x => x.Session.StartDateTime.Date == date.Value.Date);

        return query
            .OrderBy(x => x.Session.StartDateTime)
            .Select(x =>
            {
                var hall = seatMapService.GetHall(x.Session.HallId);
                int free = hall == null ? 0 : seatMapService.CountFreeSeats(x.Session, hall);
                return new SessionListItem { Session = x.Session, Movie = x.Movie, FreeSeats = free };
            })
            .ToList();
    }

    public Session? GetSession(string sessionId) => sessionRepository.GetAll().FirstOrDefault(s => s.SessionId == sessionId);
    public Movie? GetMovie(string movieId) => movieRepository.GetAll().FirstOrDefault(m => m.MovieId == movieId);
}
