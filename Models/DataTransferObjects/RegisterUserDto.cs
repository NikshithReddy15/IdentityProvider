namespace Models.DataTransferObjects;

public class RegisterUserDto
{
    public required string Firstname { get; set; }
    public string Middlename { get; set; }
    public required string Lastname { get; set; }
    public required string Email { get; set; }
    public string CountryCode { get; set; }
    public string MobileNumber { get; set; }
    public required string Password { get; set; }
}