namespace Models.DataModels;

public class RevokedTokenDataModel
{
    public Guid Id  { get; set; }
    public required string HashedToken { get; set; }
    public DateTime GeneratedOn { get; set; }
    public DateTime ExpiresOn { get; set; }
}