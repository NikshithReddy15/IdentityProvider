using System.ComponentModel.DataAnnotations.Schema;
using Models.Enums;

namespace Models.CoreModels;

public class User
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Firstname { get; set; }
    public string Middlename { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
    public string CountryCode { get; set; }
    public string MobileNumber { get; set; }
    public string Password { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public string Name => string.IsNullOrEmpty(Middlename) ? Firstname + " " + Lastname : Firstname + " " + Middlename + " " + Lastname;
}