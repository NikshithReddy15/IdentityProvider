using System.Net;
using Models.Events;
using Worker.Email;

namespace Worker.EventHandlers;

internal class RegistrationOtpEventHandler(IEmailService emailService) : IEventHandler<RegistrationOtpEvent>
{
    public async Task HandleAsync(RegistrationOtpEvent @event)
    {
        var subject = "Your Registration OTP";
        var htmlBody =
            $"<p>Hi {WebUtility.HtmlEncode(@event.FirstName)},</p>" +
            $"<p>Your OTP for registration is: <strong>{WebUtility.HtmlEncode(@event.OtpCode)}</strong></p>" +
            "<p>This OTP is valid for 10 minutes.</p>";

        await emailService.SendEmailAsync(@event.Email, @event.FirstName, subject, htmlBody);
    }
}
