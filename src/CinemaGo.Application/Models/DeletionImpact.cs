namespace CinemaGo.Application.Models;


public class DeletionImpact
{
    public bool Found { get; init; }
    public int AffectedSessions { get; init; }
    public int AffectedActiveBookings { get; init; }
}
