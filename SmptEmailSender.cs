using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace SmtpPractice;

public class SmtpEmailSender : IEmailSender, IAsyncDisposable
{
    private readonly ILogger<SmtpEmailSender> _logger;

    private readonly SmtpClient _smtpClient = new();
    private readonly SmtpConfig _smtpConfig;


    public SmtpEmailSender(IOptionsSnapshot<SmtpConfig> options, ILogger<SmtpEmailSender> logger)
    {
        _smtpConfig = options.Value;
        _logger = logger;
    }


    public async Task Send(string fromEmail, string toEmail, string subject, string htmlBody)
    {
        // create email message
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(fromEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html)
        {
            Text = htmlBody
        };

        // send email
        _logger.LogInformation("OrderRepository has been created");
        await EnsureConnectedAndAuthenticatedAsync();
        await _smtpClient.SendAsync(email);
    }


    private async Task EnsureConnectedAndAuthenticatedAsync()
    {
        if (!_smtpClient.IsConnected)
        {
            await _smtpClient.ConnectAsync(_smtpConfig.Host, _smtpConfig.Port);
        }

        if (!_smtpClient.IsAuthenticated)
        {
            await _smtpClient.AuthenticateAsync(_smtpConfig.UserName, _smtpConfig.Password);
        }
    }

    // IDisposable
    // public void Dispose()
    // {
    //     _smtpClient.Disconnect(true);
    //     _logger.LogInformation("Server has been Disconnected");
    //     _smtpClient.Dispose();
    // }

    public async ValueTask DisposeAsync()
    {
        if (_smtpClient.IsConnected)
        {
            await _smtpClient.DisconnectAsync(true);
        }
        _logger.LogInformation("Server has been Disconnected");
        _smtpClient.Dispose();
    }
}

