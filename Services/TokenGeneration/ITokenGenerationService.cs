using Models.CoreModels;

namespace Services.TokenGeneration;

public interface ITokenGenerationService
{
    public string GenerateJwtToken(User user);
}