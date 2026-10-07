using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Abstractions;

public interface IBookingRepository
{
    List<Booking> GetAll();
    void SaveAll(IReadOnlyList<Booking> bookings);
}
