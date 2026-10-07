namespace CinemaGo.ConsoleApp.Configuration;


public class EmailSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string User { get; set; } = "vovam0809977@gmail.com";
    public string Password { get; set; } = "twsw yyyn oria hfdr";
    public bool Enabled { get; set; } = false;
}


public class AppSettings
{
    public EmailSettings Smtp { get; set; } = new();
}
