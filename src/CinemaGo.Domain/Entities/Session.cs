namespace CinemaGo.Domain.Entities;

public enum SessionStatus
{
    Active,
    Completed,
    Cancelled
}


public class Session
{
    public string SessionId { get; }
    public string MovieId { get; }
    public string HallId { get; }
    public DateTime StartDateTime { get; }
    public decimal StandardPrice { get; private set; }
    public decimal VipPrice { get; private set; }
    public SessionStatus Status { get; private set; }

    public Session(string sessionId, string movieId, string hallId, DateTime startDateTime,
        decimal standardPrice, decimal vipPrice, SessionStatus status)
    {
        if (standardPrice < 0 || vipPrice < 0)
            throw new ArgumentException("Prices cannot be negative.");

        SessionId = sessionId;
        MovieId = movieId;
        HallId = hallId;
        StartDateTime = startDateTime;
        StandardPrice = standardPrice;
        VipPrice = vipPrice;
        Status = status;
    }

    public bool IsBookable(DateTime now) => Status == SessionStatus.Active && StartDateTime >= now;

  
    public bool HasEnded(int movieDurationMinutes, DateTime now) =>
        StartDateTime.AddMinutes(movieDurationMinutes) < now;

    public void MarkCompleted() => Status = SessionStatus.Completed;

   
    public void UpdatePrices(decimal newStandardPrice, decimal newVipPrice)
    {
        if (newStandardPrice < 0 || newVipPrice < 0)
            throw new ArgumentException("Prices cannot be negative.");
        StandardPrice = newStandardPrice;
        VipPrice = newVipPrice;
    }
}
