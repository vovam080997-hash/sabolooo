using CinemaGo.Domain.Security;

namespace CinemaGo.Domain.Entities;


public enum MenuKind
{
    Admin,
    Customer
}


public abstract class User
{
    public int Id { get; }
    public string Username { get; }
    public string Email { get; }
    public string Role { get; }

    private string passwordHash;


    internal string PasswordHash => passwordHash;

    protected User(int id, string username, string passwordHash, string email, string role)
    {
        Id = id;
        Username = username;
        this.passwordHash = passwordHash;
        Email = email;
        Role = role;
    }

    public bool CheckPassword(string inputPassword) => PasswordHasher.Verify(inputPassword, passwordHash);


    public bool TryChangePassword(string oldPassword, string newPassword)
    {
        if (!CheckPassword(oldPassword)) return false;
        passwordHash = PasswordHasher.Hash(newPassword);
        return true;
    }

    
    public abstract MenuKind GetMenuKind();
}
