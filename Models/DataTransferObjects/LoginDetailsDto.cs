namespace Models.DataTransferObjects;

public class LoginDetailsDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public Guid DeviceId { get; set; }
    public required string DeviceName { get; set; }
}