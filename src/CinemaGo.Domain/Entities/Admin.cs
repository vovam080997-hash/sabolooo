namespace CinemaGo.Domain.Entities;


public sealed class Admin : User
{
    public Admin(int id, string username, string password, string email)
        : base(id, username, password, email, "admin") { }

    public override MenuKind GetMenuKind() => MenuKind.Admin;
}
