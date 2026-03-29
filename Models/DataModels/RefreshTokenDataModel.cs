namespace Models.DataModels;

public class RefreshTokenDataModel : ICreatedAuditFields
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime ExpiresOn { get; set; }
    public DateTime? RevokedOn { get; set; }
    public DateTime DateCreated { get; set; }
}
