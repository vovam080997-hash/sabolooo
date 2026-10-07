namespace CinemaGo.ConsoleApp.UI;


public static class ConsoleUI
{
    private static void WithColor(ConsoleColor color, Action action)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        try { action(); }
        finally { Console.ForegroundColor = previous; }
    }

    public static void Title(string text)
    {
        int width = Math.Max(text.Length + 6, 45);
        string border = new string('═', width);
        WithColor(ConsoleColor.Cyan, () =>
        {
            Console.WriteLine("╔" + border + "╗");
            Console.WriteLine("║" + Center(text, width) + "║");
            Console.WriteLine("╚" + border + "╝");
        });
    }

    public static void Header(string text)
    {
        Console.WriteLine();
        WithColor(ConsoleColor.Yellow, () =>
        {
            string line = $" {text} ";
            int pad = Math.Max(0, 50 - line.Length);
            Console.WriteLine("── " + line + new string('─', pad));
        });
    }

    public static void MenuTitle(string text)
    {
        Console.WriteLine();
        WithColor(ConsoleColor.Magenta, () => Console.WriteLine($"====== {text} ======"));
    }

    public static void Success(string text) => WithColor(ConsoleColor.Green, () => Console.WriteLine(text));
    public static void Error(string text) => WithColor(ConsoleColor.Red, () => Console.WriteLine(text));
    public static void Warning(string text) => WithColor(ConsoleColor.DarkYellow, () => Console.WriteLine(text));
    public static void Info(string text) => WithColor(ConsoleColor.Cyan, () => Console.WriteLine(text));
    public static void Muted(string text) => WithColor(ConsoleColor.DarkGray, () => Console.WriteLine(text));

  
    public static void Result(bool success, string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        if (success) Success(message); else Error(message);
    }

    public static void MenuOption(string number, string text)
    {
        WithColor(ConsoleColor.White, () => Console.Write($"  {number}. "));
        Console.WriteLine(text);
    }

    public static void Box(IEnumerable<string> lines, ConsoleColor borderColor, string? title = null)
    {
        var list = lines.ToList();
        int innerWidth = Math.Max(list.Count == 0 ? 0 : list.Max(l => l.Length), title?.Length ?? 0) + 2;

        WithColor(borderColor, () => Console.WriteLine("╔" + new string('═', innerWidth) + "╗"));

        if (!string.IsNullOrEmpty(title))
        {
            WithColor(borderColor, () => Console.Write("║ "));
            WithColor(ConsoleColor.White, () => Console.Write(title.PadRight(innerWidth - 1)));
            WithColor(borderColor, () => Console.WriteLine("║"));
            WithColor(borderColor, () => Console.WriteLine("╠" + new string('═', innerWidth) + "╣"));
        }

        foreach (var line in list)
        {
            WithColor(borderColor, () => Console.Write("║ "));
            Console.Write(line.PadRight(innerWidth - 1));
            WithColor(borderColor, () => Console.WriteLine("║"));
        }

        WithColor(borderColor, () => Console.WriteLine("╚" + new string('═', innerWidth) + "╝"));
    }

    private static string Center(string text, int width)
    {
        if (text.Length >= width) return text.PadRight(width);
        int padLeft = (width - text.Length) / 2;
        int padRight = width - text.Length - padLeft;
        return new string(' ', padLeft) + text + new string(' ', padRight);
    }

    
    public static string ReadPassword()
    {
        if (Console.IsInputRedirected) return Console.ReadLine() ?? "";

        var password = new System.Text.StringBuilder();
        ConsoleKeyInfo key;
        while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b");
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        Console.WriteLine();
        return password.ToString();
    }

    public static bool Confirm(string prompt)
    {
        Console.Write($"{prompt} (yes/no): ");
        string answer = Console.ReadLine()?.Trim().ToLower() ?? "";
        return answer is "yes" or "y";
    }
}
