using CinemaGo.Application.Abstractions;
using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Services;


public class SeatMapService
{
    private readonly IBookingRepository bookingRepository;
    private readonly IHallRepository hallRepository;

    public SeatMapService(IBookingRepository bookingRepository, IHallRepository hallRepository)
    {
        this.bookingRepository = bookingRepository;
        this.hallRepository = hallRepository;
    }

    public Hall? GetHall(string hallId) => hallRepository.GetById(hallId);

    public List<Seat> BuildSeatMap(Session session, Hall hall)
    {
        var seats = hall.CreateSeats();
        var activeBookings = bookingRepository.GetAll()
            .Where(b => b.SessionId == session.SessionId && b.Status == BookingStatus.Confirmed);

        foreach (var b in activeBookings)
        {
            var seat = seats.FirstOrDefault(s => s.Row == b.Row && s.Number == b.Seat);
            if (seat != null && !seat.IsBooked) seat.Book();
        }
        return seats;
    }

    public int CountFreeSeats(Session session, Hall hall) => BuildSeatMap(session, hall).Count(s => !s.IsBooked);
}
