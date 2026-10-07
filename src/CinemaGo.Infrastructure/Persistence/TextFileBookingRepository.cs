using System.Globalization;
using CinemaGo.Application.Abstractions;
using CinemaGo.Domain.Entities;

namespace CinemaGo.Infrastructure.Persistence;

public class TextFileBookingRepository : IBookingRepository
{
    private const string FileName = "Bookings.txt";
    private const string Header = "// BookingID | UserID | SessionID | Row | Seat | TicketType | Price | TicketCode | Status";
    private readonly IAppLogger logger;

    public TextFileBookingRepository(IAppLogger logger) => this.logger = logger;

    public List<Booking> GetAll()
    {
        var result = new List<Booking>();
        foreach (var line in TextFileHelper.ReadDataLines(FileName, logger))
        {
            var booking = ParseLine(line);
            if (booking != null) result.Add(booking);
            else logger.Log($"Corrupted record in {FileName} skipped: {line}");
        }
        return result;
    }

    public void SaveAll(IReadOnlyList<Booking> bookings) =>
        TextFileHelper.WriteDataLines(FileName, Header, bookings.Select(ToLine), logger);

    private static string ToLine(Booking b) =>
        $"{b.BookingId}|{b.UserId}|{b.SessionId}|{b.Row}|{b.Seat}|{b.TicketType}|{b.Price.ToString(CultureInfo.InvariantCulture)}|{b.TicketCode}|{b.Status}";

    private static Booking? ParseLine(string line)
    {
        var p = line.Split('|');
        if (p.Length < 9) return null;
        if (!int.TryParse(p[1].Trim(), out int userId)) return null;
        if (!int.TryParse(p[3].Trim(), out int row)) return null;
        if (!int.TryParse(p[4].Trim(), out int seat)) return null;
        if (!Enum.TryParse<TicketTypeKind>(p[5].Trim(), true, out var type)) return null;
        if (!decimal.TryParse(p[6].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price)) return null;
        if (!Enum.TryParse<BookingStatus>(p[8].Trim(), true, out var status)) status = BookingStatus.Confirmed;

        try
        {
            return new Booking(p[0].Trim(), userId, p[2].Trim(), row, seat, type, price, p[7].Trim(), status);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
