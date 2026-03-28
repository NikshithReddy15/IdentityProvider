namespace Models.Events;

public class RegistrationOtpEvent : BaseEvent
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string OtpCode { get; set; }
}
