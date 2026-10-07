namespace CinemaGo.Domain.Entities;

public enum BookingStatus
{
    Confirmed,
    Cancelled
}

public enum TicketTypeKind
{
    Standard,
    VIP
}


public class Booking
{
    public string BookingId { get; }
    public int UserId { get; }
    public string SessionId { get; }
    public int Row { get; }
    public int Seat { get; }
    public TicketTypeKind TicketType { get; }
    public decimal Price { get; }
    public string TicketCode { get; }
    public BookingStatus Status { get; private set; }

    public Booking(string bookingId, int userId, string sessionId, int row, int seat,
        TicketTypeKind ticketType, decimal price, string ticketCode, BookingStatus status)
    {
        if (price < 0) throw new ArgumentException("Price cannot be negative.");

        BookingId = bookingId;
        UserId = userId;
        SessionId = sessionId;
        Row = row;
        Seat = seat;
        TicketType = ticketType;
        Price = price;
        TicketCode = ticketCode;
        Status = status;
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("This booking is already cancelled.");
        Status = BookingStatus.Cancelled;
    }
}
