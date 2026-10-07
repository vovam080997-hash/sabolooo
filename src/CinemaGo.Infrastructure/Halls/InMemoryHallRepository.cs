using CinemaGo.Application.Abstractions;
using CinemaGo.Domain.Entities;

namespace CinemaGo.Infrastructure.Halls;


public class InMemoryHallRepository : IHallRepository
{
    private readonly List<Hall> halls = new()
    {
        new Hall("H1", rows: 6, seatsPerRow: 10, vipFromRow: 4), 
        new Hall("H2", rows: 5, seatsPerRow: 8, vipFromRow: 4)
    };

    public List<Hall> GetAll() => halls;

    public Hall? GetById(string hallId) => halls.FirstOrDefault(h => h.HallId == hallId);
}
