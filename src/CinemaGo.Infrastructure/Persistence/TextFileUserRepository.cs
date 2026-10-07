using CinemaGo.Application.Abstractions;
using CinemaGo.Domain.Entities;

namespace CinemaGo.Infrastructure.Persistence;


public class TextFileUserRepository : IUserRepository
{
    private const string FileName = "Users.txt";
    private const string Header = "// ID | Username | PasswordHash | Email | Role";
    private readonly IAppLogger logger;

    public TextFileUserRepository(IAppLogger logger) => this.logger = logger;

    public List<User> GetAll()
    {
        var result = new List<User>();
        foreach (var line in TextFileHelper.ReadDataLines(FileName, logger))
        {
            var user = ParseLine(line);
            if (user != null) result.Add(user);
            else logger.Log($"Corrupted record in {FileName} skipped: {line}");
        }
        return result;
    }

    public void SaveAll(IReadOnlyList<User> users) =>
        TextFileHelper.WriteDataLines(FileName, Header, users.Select(ToLine), logger);

    private static string ToLine(User u) => $"{u.Id}|{u.Username}|{u.PasswordHash}|{u.Email}|{u.Role}";

    private static User? ParseLine(string line)
    {
        var p = line.Split('|');
        if (p.Length < 5) return null;
        if (!int.TryParse(p[0].Trim(), out int id)) return null;

        string username = p[1].Trim();
        string passwordHash = p[2].Trim();
        string email = p[3].Trim();
        string role = p[4].Trim().ToLowerInvariant();

        return role switch
        {
            "admin" => new Admin(id, username, passwordHash, email),
            "customer" => new Customer(id, username, passwordHash, email),
            _ => null
        };
    }
}
