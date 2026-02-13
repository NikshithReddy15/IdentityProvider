using System.ComponentModel.DataAnnotations.Schema;
using Models.Enums;

namespace Models.CoreModels;

public class User
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? CountryCode { get; set; }
    public string? MobileNumber { get; set; }
    public required string Password { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public string Name => string.IsNullOrEmpty(MiddleName) ? FirstName + " " + LastName : FirstName + " " + MiddleName + " " + LastName;
}