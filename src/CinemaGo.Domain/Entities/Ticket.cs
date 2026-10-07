namespace CinemaGo.Domain.Entities;


public abstract class Ticket
{
    public string TicketCode { get; }
    public decimal Price { get; protected set; }

    protected Ticket(string ticketCode)
    {
        TicketCode = ticketCode;
    }

    
    public abstract bool IsVip { get; }

    public abstract string ReceiptTitle { get; }


    public abstract decimal CalculatePrice(decimal basePrice, decimal discountPercent);

    
    public abstract IReadOnlyList<string> BuildReceiptLines(Booking booking, Movie movie, Session session, Hall hall);

    protected static decimal ApplyDiscount(decimal price, decimal discountPercent)
    {
        if (discountPercent < 0) discountPercent = 0;
        if (discountPercent > 100) discountPercent = 100;
        decimal result = price - (price * discountPercent / 100m);
        return result < 0 ? 0 : result;
    }
}


public class StandardTicket : Ticket
{
    public StandardTicket(string ticketCode) : base(ticketCode) { }

    public override bool IsVip => false;
    public override string ReceiptTitle => "CinemaGo — Standard Ticket";

    public override decimal CalculatePrice(decimal basePrice, decimal discountPercent)
    {
        Price = ApplyDiscount(basePrice, discountPercent);
        return Price;
    }

    public override IReadOnlyList<string> BuildReceiptLines(Booking booking, Movie movie, Session session, Hall hall) => new[]
    {
        $"Ticket code : {TicketCode}",
        $"Movie       : {movie.Title} ({movie.AgeRating})",
        $"Hall        : {hall.HallId}",
        $"Seat        : Row {booking.Row}, Seat {booking.Seat}",
        $"Showtime    : {session.StartDateTime:yyyy-MM-dd HH:mm}",
        $"Price       : {Price} GEL"
    };
}


public class VipTicket : Ticket
{
    public VipTicket(string ticketCode) : base(ticketCode) { }

    public override bool IsVip => true;
    public override string ReceiptTitle => "CinemaGo — VIP Ticket";

    public override decimal CalculatePrice(decimal basePrice, decimal discountPercent)
    {
        Price = ApplyDiscount(basePrice, discountPercent);
        return Price;
    }

    public override IReadOnlyList<string> BuildReceiptLines(Booking booking, Movie movie, Session session, Hall hall) => new[]
    {
        $"Ticket code : {TicketCode}",
        $"Movie       : {movie.Title} ({movie.AgeRating})",
        $"Hall        : {hall.HallId}",
        $"VIP seat    : Row {booking.Row}, Seat {booking.Seat}",
        $"Showtime    : {session.StartDateTime:yyyy-MM-dd HH:mm}",
        $"Price       : {Price} GEL"
    };
}
