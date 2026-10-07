using CinemaGo.Application.Services;
using CinemaGo.ConsoleApp.UI;
using CinemaGo.Domain.Entities;

namespace CinemaGo.ConsoleApp.Menus;


public static class MenuShared
{
    public static void ChangePassword(AuthService authService, User user)
    {
        ConsoleUI.Header("Change password");
        Console.Write("Current password: ");
        string oldPassword = ConsoleUI.ReadPassword();

        ConsoleUI.Muted(AuthService.PasswordRules);
        Console.Write("New password: ");
        string newPassword = ConsoleUI.ReadPassword();

        Console.Write("Confirm new password: ");
        string confirmPassword = ConsoleUI.ReadPassword();

        var result = authService.ChangePassword(user, oldPassword, newPassword, confirmPassword);
        ConsoleUI.Result(result.Success, result.Message);
    }
}
