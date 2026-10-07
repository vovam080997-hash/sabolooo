using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Abstractions;

public interface ISessionRepository
{
    List<Session> GetAll();
    void SaveAll(IReadOnlyList<Session> sessions);
}
