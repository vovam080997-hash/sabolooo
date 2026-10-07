using CinemaGo.Application.Services;
using CinemaGo.ConsoleApp.UI;
using CinemaGo.Domain.Entities;

namespace CinemaGo.ConsoleApp.Menus;


public class AppRunner
{
    private readonly AuthService authService;
    private readonly SessionStatusUpdater statusUpdater;
    private readonly AdminMenu adminMenu;
    private readonly CustomerMenu customerMenu;

    public AppRunner(AuthService authService, SessionStatusUpdater statusUpdater, AdminMenu adminMenu, CustomerMenu customerMenu)
    {
        this.authService = authService;
        this.statusUpdater = statusUpdater;
        this.adminMenu = adminMenu;
        this.customerMenu = customerMenu;
    }

    public void Run()
    {
        ConsoleUI.Title("Kidev erti Shedevri");

        bool exit = false;
        while (!exit)
        {
            statusUpdater.UpdateStatuses();

            ConsoleUI.MenuTitle("MAIN MENU");
            ConsoleUI.MenuOption("1", "Register");
            ConsoleUI.MenuOption("2", "Log in");
            ConsoleUI.MenuOption("3", "Exit program");
            Console.Write("Choose an option: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": Register(); break;
                case "2": Login(); break;
                case "3": exit = true; break;
                default: ConsoleUI.Error("Invalid choice."); break;
            }
        }

        ConsoleUI.Info("Program closed. Goodbye!");
    }

    private void Register()
    {
        ConsoleUI.Header("Registration");
        Console.Write("Username: ");
        string username = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Email: ");
        string email = Console.ReadLine()?.Trim() ?? "";

        ConsoleUI.Muted(AuthService.PasswordRules);
        Console.Write("Password: ");
        string password = ConsoleUI.ReadPassword();

        var result = authService.Register(username, email, password);
        ConsoleUI.Result(result.Success, result.Message);
    }

    private void Login()
    {
        ConsoleUI.Header("Login");
        Console.Write("Username: ");
        string username = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Password: ");
        string password = ConsoleUI.ReadPassword();

        var result = authService.Login(username, password);
        if (!result.Success || result.Value == null)
        {
            ConsoleUI.Error(result.Message);
            return;
        }

        ConsoleUI.Success(result.Message);


        User user = result.Value;
        switch (user.GetMenuKind())
        {
            case MenuKind.Admin: adminMenu.Run((Admin)user); break;
            case MenuKind.Customer: customerMenu.Run((Customer)user); break;
        }
    }
}
