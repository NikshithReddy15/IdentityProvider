using Models.DataModels;

namespace Services.Repositories.LoginSession;

public interface ILoginSessionRepository
{
    Task CreateLoginSessionAsync(LoginSessionDataModel loginSessionDataModel);

    Task<LoginSessionDataModel?> GetLoginSessionAsync(Guid sessionId);

    Task DeactivateLoginSessionAsync(Guid sessionId);

    Task<bool> IsValidLoginSessionAsync(Guid userId, Guid sessionId);
}