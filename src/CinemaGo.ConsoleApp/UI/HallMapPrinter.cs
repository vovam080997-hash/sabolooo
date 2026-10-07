using CinemaGo.Domain.Entities;

namespace CinemaGo.ConsoleApp.UI;


public static class HallMapPrinter
{
    private const int RowLabelWidth = 9;   
    private const int SeatColumnWidth = 4; 

    public static void Print(Hall hall, List<Seat> seats)
    {
        int gridWidth = hall.SeatsPerRow * SeatColumnWidth;

        ConsoleUI.Muted($"  Hall {hall.HallId}");
        PrintScreen(gridWidth);
        Console.WriteLine();

     
        Console.Write(new string(' ', RowLabelWidth));
        for (int s = 1; s <= hall.SeatsPerRow; s++)
            Console.Write($"{s,2}  ");
        Console.WriteLine();

        for (int r = 1; r <= hall.Rows; r++)
        {
            Console.Write($"Row {r,2} │ ");
            for (int s = 1; s <= hall.SeatsPerRow; s++)
            {
                var seat = seats.First(x => x.Row == r && x.Number == s);
                SeatBlock(seat.StatusSymbol);
            }
            Console.WriteLine();
        }

        Console.WriteLine();
        PrintLegend();
    }


    private static void PrintScreen(int gridWidth)
    {
        const string label = " SCREEN ";
        int pad = Math.Max(0, (gridWidth - label.Length) / 2);
        int rightPad = Math.Max(0, gridWidth - pad - label.Length);

        Console.Write(new string(' ', RowLabelWidth));
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(new string('▁', pad));
        Console.ForegroundColor = ConsoleColor.Black;
        Console.BackgroundColor = ConsoleColor.Gray;
        Console.Write(label);
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(new string('▁', rightPad));
        Console.ForegroundColor = previous;
        Console.WriteLine();
    }

   
    private static void SeatBlock(char symbol)
    {
        var color = symbol switch
        {
            'X' => ConsoleColor.Red,    
            'V' => ConsoleColor.Yellow, 
            _ => ConsoleColor.Green     
        };

        Console.BackgroundColor = color;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write($" {symbol} ");
        Console.ResetColor();
        Console.Write(" ");
    }

    private static void PrintLegend()
    {
        Console.Write("  ");
        SeatBlock('O');
        Console.Write(" Free    ");
        SeatBlock('X');
        Console.Write(" Booked    ");
        SeatBlock('V');
        Console.Write(" VIP");
        Console.WriteLine();
    }
}
