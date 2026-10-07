using System.Text.RegularExpressions;
using CinemaGo.Application.Abstractions;
using CinemaGo.Application.Models;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Security;

namespace CinemaGo.Application.Services;


public class AuthService
{
    private readonly IUserRepository userRepository;
    private readonly IAppLogger logger;

    public const string PasswordRules =
        "Password requirements: 8-20 characters, at least one uppercase letter (A-Z) and one special character (e.g. !@#$%^&*).";

    public AuthService(IUserRepository userRepository, IAppLogger logger)
    {
        this.userRepository = userRepository;
        this.logger = logger;
    }

    public OperationResult<User> Register(string username, string email, string password)
    {
        var users = userRepository.GetAll();

        if (string.IsNullOrWhiteSpace(username) ||
            users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            return OperationResult<User>.Fail("Error: username is empty or already taken.");

        if (!IsValidEmail(email) || users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            return OperationResult<User>.Fail("Error: invalid email format or already in use.");

        if (!IsValidPassword(password))
            return OperationResult<User>.Fail("Error: password does not meet the requirements.");

        int newId = users.Count == 0 ? 1001 : users.Max(u => u.Id) + 1;
        var customer = new Customer(newId, username, PasswordHasher.Hash(password), email);
        users.Add(customer);
        userRepository.SaveAll(users);

        logger.Log($"New user registered: {username} (ID {newId})");
        return OperationResult<User>.Ok(customer, $"Registration successful! Your ID: {newId}");
    }

    public OperationResult<User> Login(string username, string password)
    {
        var users = userRepository.GetAll();
        var user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (user == null || !user.CheckPassword(password))
        {
            logger.Log($"Failed login attempt: {username}");
            return OperationResult<User>.Fail("Incorrect username or password.");
        }

        logger.Log($"Login successful: {user.Username} (role: {user.Role})");
        return OperationResult<User>.Ok(user, $"Welcome, {user.Username}!");
    }

    public OperationResult ChangePassword(User user, string oldPassword, string newPassword, string confirmPassword)
    {
        if (!user.CheckPassword(oldPassword))
            return OperationResult.Fail("Incorrect current password.");

        if (!IsValidPassword(newPassword))
            return OperationResult.Fail("Error: password does not meet the requirements.");

        if (newPassword != confirmPassword)
            return OperationResult.Fail("Error: passwords do not match.");

        if (!user.TryChangePassword(oldPassword, newPassword))
            return OperationResult.Fail("Failed to change the password.");

        var users = userRepository.GetAll();
        var stored = users.First(u => u.Id == user.Id);


        if (!ReferenceEquals(stored, user))
            stored.TryChangePassword(oldPassword, newPassword);

        userRepository.SaveAll(users);
        logger.Log($"User {user.Username} changed their password.");
        return OperationResult.Ok("Password changed successfully.");
    }

    public static bool IsValidEmail(string email) =>
        !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    public static bool IsValidPassword(string pw) =>
        !string.IsNullOrEmpty(pw)
        && pw.Length >= 8 && pw.Length <= 20
        && pw.Any(char.IsUpper)
        && pw.Any(c => !char.IsLetterOrDigit(c));
}
