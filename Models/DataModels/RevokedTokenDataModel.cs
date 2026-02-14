namespace Models.DataModels;

public class RevokedTokenDataModel : ICreatedAuditFields
{
    public Guid Id  { get; set; }
    public required string HashedToken { get; set; }
    public DateTime ExpiresOn { get; set; }
    public DateTime DateCreated { get; set; }
}