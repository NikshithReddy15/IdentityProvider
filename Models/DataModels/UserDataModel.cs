namespace Models.DataModels;

public class UserDataModel : IAuditFields
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? CountryCode { get; set; }
    public string? MobileNumber { get; set; }
    public required string Password { get; set; }
    public sbyte AccountStatus { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
}