using Services.Repositories.UserOtp;

namespace Worker.Jobs;

public class OtpCleanupJob(IUserOtpRepository userOtpRepository, ILogger<OtpCleanupJob> logger)
{
    public async Task ExecuteAsync()
    {
        logger.LogInformation("OTP cleanup job started at {Time}", DateTime.UtcNow);
        await userOtpRepository.DeleteExpiredAndUsedOtpsAsync();
        logger.LogInformation("OTP cleanup job completed at {Time}", DateTime.UtcNow);
    }
}
