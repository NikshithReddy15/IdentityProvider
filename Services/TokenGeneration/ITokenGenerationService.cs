using Models.CoreModels;
using Models.DataModels;

namespace Services.TokenGeneration;

public interface ITokenGenerationService
{
    string GenerateJwtToken(User user, Guid sessionId);

    RefreshTokenDataModel GenerateRefreshToken(Guid userId, Guid sessionId);
}