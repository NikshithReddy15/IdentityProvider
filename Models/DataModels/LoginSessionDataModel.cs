namespace Models.DataModels;

public class LoginSessionDataModel : ICreatedAuditFields
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid DeviceId { get; set; }
    public required string DeviceName { get; set; }
    public required string UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public DateTime LastSeenOn { get; set; }
    public DateTime? DeactivatedOn { get; set; }
    public DateTime DateCreated { get; set; }
}
