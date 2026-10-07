using System.Globalization;
using CinemaGo.Application.Abstractions;
using CinemaGo.Domain.Entities;

namespace CinemaGo.Infrastructure.Persistence;

public class TextFileSessionRepository : ISessionRepository
{
    private const string FileName = "Sessions.txt";
    private const string Header = "// SessionID | MovieID | HallID | StartDateTime | StandardPrice | VipPrice | Status";
    private const string DateFormat = "yyyy-MM-dd HH:mm";
    private readonly IAppLogger logger;

    public TextFileSessionRepository(IAppLogger logger) => this.logger = logger;

    public List<Session> GetAll()
    {
        var result = new List<Session>();
        foreach (var line in TextFileHelper.ReadDataLines(FileName, logger))
        {
            var session = ParseLine(line);
            if (session != null) result.Add(session);
            else logger.Log($"Corrupted record in {FileName} skipped: {line}");
        }
        return result;
    }

    public void SaveAll(IReadOnlyList<Session> sessions) =>
        TextFileHelper.WriteDataLines(FileName, Header, sessions.Select(ToLine), logger);

    private static string ToLine(Session s) =>
        $"{s.SessionId}|{s.MovieId}|{s.HallId}|{s.StartDateTime.ToString(DateFormat)}|{s.StandardPrice}|{s.VipPrice}|{s.Status}";

    private static Session? ParseLine(string line)
    {
        var p = line.Split('|');
        if (p.Length < 7) return null;
        if (!DateTime.TryParseExact(p[3].Trim(), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
            return null;
        if (!decimal.TryParse(p[4].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var stdPrice)) return null;
        if (!decimal.TryParse(p[5].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var vipPrice)) return null;
        if (!Enum.TryParse<SessionStatus>(p[6].Trim(), true, out var status)) status = SessionStatus.Active;

        try
        {
            return new Session(p[0].Trim(), p[1].Trim(), p[2].Trim(), start, stdPrice, vipPrice, status);
        }
        catch (ArgumentException)
        {
            return null; 
        }
    }
}
