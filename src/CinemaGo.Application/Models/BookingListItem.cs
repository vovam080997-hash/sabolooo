using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Models;


public class BookingListItem
{
    public required Booking Booking { get; init; }
    public Movie? Movie { get; init; }
    public Session? Session { get; init; }
}
