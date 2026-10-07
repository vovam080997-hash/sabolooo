using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Abstractions;

public interface IMovieRepository
{
    List<Movie> GetAll();
    void SaveAll(IReadOnlyList<Movie> movies);
}
