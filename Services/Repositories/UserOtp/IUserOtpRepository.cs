using Models.DataModels;

namespace Services.Repositories.UserOtp;

public interface IUserOtpRepository
{
    Task CreateOtpAsync(UserOtpDataModel userOtp);

    Task<UserOtpDataModel?> GetActiveOtpAsync(Guid userId);

    Task MarkOtpAsUsedAsync(Guid otpId);
}
