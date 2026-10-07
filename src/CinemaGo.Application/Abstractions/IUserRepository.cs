using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Abstractions;


public interface IUserRepository
{
    List<User> GetAll();
    void SaveAll(IReadOnlyList<User> users);
}
