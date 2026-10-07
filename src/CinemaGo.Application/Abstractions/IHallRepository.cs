using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Abstractions;


public interface IHallRepository
{
    List<Hall> GetAll();
    Hall? GetById(string hallId);
}
