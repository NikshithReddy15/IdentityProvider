using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Worker.Email;

internal class EmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var smtpSection = configuration.GetRequiredSection("Smtp");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(smtpSection["SenderName"], smtpSection["SenderEmail"]));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpSection["Host"], int.Parse(smtpSection["Port"]!), SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(smtpSection["Username"], smtpSection["Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
