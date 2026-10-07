namespace CinemaGo.Domain.Entities;


public class Movie
{
    public string MovieId { get; }
    public string Title { get; }
    public string Genre { get; }
    public int DurationMinutes { get; }
    public string AgeRating { get; }

    public Movie(string movieId, string title, string genre, int durationMinutes, string ageRating)
    {
        MovieId = movieId;
        Title = title;
        Genre = genre;
        DurationMinutes = durationMinutes;
        AgeRating = ageRating;
    }
}
