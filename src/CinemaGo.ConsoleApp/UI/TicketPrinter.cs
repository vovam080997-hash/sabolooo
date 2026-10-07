using CinemaGo.Application.Models;

namespace CinemaGo.ConsoleApp.UI;


public static class TicketPrinter
{
    public static void Print(BookingReceipt receipt)
    {
        var color = receipt.Ticket.IsVip ? ConsoleColor.Yellow : ConsoleColor.Cyan;
        var lines = receipt.Ticket.BuildReceiptLines(receipt.Booking, receipt.Movie, receipt.Session, receipt.Hall);
        ConsoleUI.Box(lines, color, receipt.Ticket.ReceiptTitle);
    }
}
