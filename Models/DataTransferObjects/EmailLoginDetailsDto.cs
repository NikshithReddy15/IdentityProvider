namespace Models.DataTransferObjects;

public class EmailLoginDetailsDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public Guid DeviceId { get; set; }
}