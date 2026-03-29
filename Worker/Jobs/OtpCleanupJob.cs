using Services.Repositories.UserOtp;

namespace Worker.Jobs;

public class OtpCleanupJob(IUserOtpRepository userOtpRepository, ILogger<OtpCleanupJob> logger)
{
    public async Task ExecuteAsync()
    {
        logger.LogInformation($"OTP cleanup job started at {DateTime.UtcNow}");
        try
        {
            await userOtpRepository.DeleteExpiredAndUsedOtpsAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while executing OTP cleanup job");
        }

        logger.LogInformation($"OTP cleanup job completed at {DateTime.UtcNow}");
    }
}
