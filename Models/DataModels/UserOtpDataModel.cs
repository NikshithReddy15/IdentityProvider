namespace Models.DataModels;

public class UserOtpDataModel : ICreatedAuditFields
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string OtpCode { get; set; }
    public DateTime ExpiresOn { get; set; }
    public bool IsUsed { get; set; }
    public DateTime DateCreated { get; set; }
}
