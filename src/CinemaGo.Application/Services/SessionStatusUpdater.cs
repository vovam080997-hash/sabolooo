using CinemaGo.Application.Abstractions;
using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Services;


public class SessionStatusUpdater
{
    private readonly ISessionRepository sessionRepository;
    private readonly IMovieRepository movieRepository;
    private readonly IAppLogger logger;

    public SessionStatusUpdater(ISessionRepository sessionRepository, IMovieRepository movieRepository, IAppLogger logger)
    {
        this.sessionRepository = sessionRepository;
        this.movieRepository = movieRepository;
        this.logger = logger;
    }

    public void UpdateStatuses()
    {
        var sessions = sessionRepository.GetAll();
        var movies = movieRepository.GetAll();
        bool changed = false;

        foreach (var session in sessions.Where(s => s.Status == SessionStatus.Active))
        {
            var movie = movies.FirstOrDefault(m => m.MovieId == session.MovieId);
            int duration = movie?.DurationMinutes ?? 120;
            if (session.HasEnded(duration, DateTime.Now))
            {
                session.MarkCompleted();
                changed = true;
                logger.Log($"Session {session.SessionId} automatically marked as completed.");
            }
        }

        if (changed) sessionRepository.SaveAll(sessions);
    }
}
