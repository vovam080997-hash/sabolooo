using System.Net;
using System.Net.Mail;
using CinemaGo.Application.Abstractions;

namespace CinemaGo.Infrastructure.Email;


public class SmtpEmailSender : IEmailSender
{
    private readonly string smtpHost;
    private readonly int smtpPort;
    private readonly string smtpUser;
    private readonly string smtpPassword;
    private readonly bool enabled;
    private readonly IAppLogger logger;

    public SmtpEmailSender(IAppLogger logger, string smtpHost = "smtp.gmail.com", int smtpPort = 587,
        string smtpUser = "vovam0809977@gmail.com", string smtpPassword = "twsw yyyn oria hfdr", bool enabled = true)
    {
        this.logger = logger;
        this.smtpHost = smtpHost;
        this.smtpPort = smtpPort;
        this.smtpUser = smtpUser;
        this.smtpPassword = smtpPassword;
        this.enabled = enabled;
    }

    public void SendBookingConfirmation(string toEmail, string subject, string body)
    {
        if (!enabled)
        {
            logger.Log($"EMAIL (simulated, SMTP disabled) -> {toEmail}: {subject}");
            return;
        }

        try
        {
            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = true
            };
            using var message = new MailMessage(smtpUser, toEmail, subject, body);
            client.Send(message);

            logger.Log($"EMAIL sent successfully -> {toEmail}: {subject}");
        }
        catch (Exception ex)
        {
   
            logger.Log($"EMAIL error -> {toEmail}: {ex.Message}");
        }
    }
}
