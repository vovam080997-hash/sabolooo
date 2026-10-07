using System.Globalization;
using CinemaGo.Application.Abstractions;
using CinemaGo.Domain.Entities;

namespace CinemaGo.Infrastructure.Persistence;

public class TextFileMovieRepository : IMovieRepository
{
    private const string FileName = "Movies.txt";
    private const string Header = "// MovieID | Title | Genre | DurationMinutes | AgeRating";
    private readonly IAppLogger logger;

    public TextFileMovieRepository(IAppLogger logger) => this.logger = logger;

    public List<Movie> GetAll()
    {
        var result = new List<Movie>();
        foreach (var line in TextFileHelper.ReadDataLines(FileName, logger))
        {
            var movie = ParseLine(line);
            if (movie != null) result.Add(movie);
            else logger.Log($"Corrupted record in {FileName} skipped: {line}");
        }
        return result;
    }

    public void SaveAll(IReadOnlyList<Movie> movies) =>
        TextFileHelper.WriteDataLines(FileName, Header, movies.Select(ToLine), logger);

    private static string ToLine(Movie m) => $"{m.MovieId}|{m.Title}|{m.Genre}|{m.DurationMinutes}|{m.AgeRating}";

    private static Movie? ParseLine(string line)
    {
        var p = line.Split('|');
        if (p.Length < 5) return null;
        if (!int.TryParse(p[3].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int duration)) return null;
        return new Movie(p[0].Trim(), p[1].Trim(), p[2].Trim(), duration, p[4].Trim());
    }
}
