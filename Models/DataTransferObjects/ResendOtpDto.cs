using System.ComponentModel.DataAnnotations;

namespace Models.DataTransferObjects;

public class ResendOtpDto
{
    [Required]
    public required string Email { get; set; }
}
