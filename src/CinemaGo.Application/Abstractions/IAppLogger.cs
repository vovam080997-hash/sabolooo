namespace CinemaGo.Application.Abstractions;


public interface IAppLogger
{
    void Log(string message);
    List<string> ReadRecent(int count);
}
