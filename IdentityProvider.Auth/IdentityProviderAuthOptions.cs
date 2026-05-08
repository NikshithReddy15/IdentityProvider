namespace IdentityProvider.Auth;

public class IdentityProviderAuthOptions
{
    public required string Key { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public bool EnableRemoteRevocationCheck { get; set; } = false;
    public string? IdentityProviderBaseUrl { get; set; }
}
