namespace Models.DataModels;

public class UserDataModel : IAuditFields
{
    public int Id { get; set; }
    public string Firstname { get; set; }
    public string Middlename { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
    public string CountryCode { get; set; }
    public string MobileNumber { get; set; }
    public string Password { get; set; }
    public sbyte AccountStatus { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
}