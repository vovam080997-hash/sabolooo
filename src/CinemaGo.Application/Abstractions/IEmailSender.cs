namespace CinemaGo.Application.Abstractions;


public interface IEmailSender
{
    void SendBookingConfirmation(string toEmail, string subject, string body);
}
