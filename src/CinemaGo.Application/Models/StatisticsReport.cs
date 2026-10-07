namespace CinemaGo.Application.Models;

public class MovieBookingCount
{
    public required string MovieTitle { get; init; }
    public int BookingCount { get; init; }
}

public class SessionFreeSeats
{
    public required string SessionId { get; init; }
    public int FreeSeats { get; init; }
    public int TotalSeats { get; init; }
}

public class GenreSessionCount
{
    public required string Genre { get; init; }
    public int SessionCount { get; init; }
}


public class StatisticsReport
{
    public required List<MovieBookingCount> PopularMovies { get; init; }
    public required List<SessionFreeSeats> FreeSeatsPerSession { get; init; }
    public required List<GenreSessionCount> SessionsByGenre { get; init; }
}
