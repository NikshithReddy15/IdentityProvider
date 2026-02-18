using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models.CoreModels;
using Models.DataModels;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Services.TokenGeneration;

public class TokenGenerationService(IConfiguration configuration) : ITokenGenerationService
{
    public string GenerateJwtToken(User user, Guid sessionId)
    {
        var claims = new Claim[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Sid, sessionId.ToString()),
        };

        var jwtSettings = configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(jwtSettings["ExpiryInMinutes"])),
            signingCredentials: signingCredentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshTokenDataModel GenerateRefreshToken(Guid userId, Guid sessionId)
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var jwtSettings = configuration.GetSection("JwtSettings");
        if (!Int32.TryParse(jwtSettings["RefreshTokenExpiryInDays"], out var refreshTokenExpiryInDays))
        {
            throw new InvalidOperationException("RefreshTokenExpiryInDays must be an integer");
        }

        var refreshTokenExpirationDate = DateTime.UtcNow.AddDays(refreshTokenExpiryInDays);
        var refreshToken = new RefreshTokenDataModel
        {
            UserId =  userId,
            SessionId = sessionId,
            RefreshToken = Convert.ToBase64String(bytes),
            ExpiresOn = refreshTokenExpirationDate,
            IsRevoked = false,
        };

        refreshToken.SetAuditFieldsOnCreate();
        return refreshToken;
    }
}