using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Models;


public class BookingReceipt
{
    public required Booking Booking { get; init; }
    public required Ticket Ticket { get; init; }
    public required Movie Movie { get; init; }
    public required Session Session { get; init; }
    public required Hall Hall { get; init; }
}
