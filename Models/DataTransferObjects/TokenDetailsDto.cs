namespace Models.DataTransferObjects;

public class TokenDetailsDto (string jwtToken, string refreshToken)
{
    public string JwtToken { get; set; } = jwtToken;
    public string RefreshToken { get; set; } = refreshToken;
}