namespace CinemaGo.Domain.Entities;


public sealed class Customer : User
{
    public Customer(int id, string username, string password, string email)
        : base(id, username, password, email, "customer") { }

    public override MenuKind GetMenuKind() => MenuKind.Customer;
}
