namespace SmtpPractice;

public interface IEmailSender
{
    Task Send(string fromEmail, string toEmail, string subject, string htmlBody);
}