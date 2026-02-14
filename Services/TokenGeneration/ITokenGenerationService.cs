using Models.CoreModels;
using Models.DataModels;

namespace Services.TokenGeneration;

public interface ITokenGenerationService
{
    string GenerateJwtToken(User user);

    RefreshTokenDataModel GenerateRefreshToken(Guid userId);
}