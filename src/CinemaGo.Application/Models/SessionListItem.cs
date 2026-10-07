using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Models;


public class SessionListItem
{
    public required Session Session { get; init; }
    public required Movie Movie { get; init; }
    public required int FreeSeats { get; init; }
}
