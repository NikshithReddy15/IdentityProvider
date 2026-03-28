using DatabaseAdapter.Dapper;
using Models.DataModels;

namespace Services.Repositories.UserOtp;

internal class UserOtpRepository(IDapperDb dapperDb) : IUserOtpRepository
{
    public async Task CreateOtpAsync(UserOtpDataModel userOtp)
    {
        var sql =
            "INSERT INTO [dbo].[UserOtp](Id, UserId, OtpCode, ExpiresOn, IsUsed, DateCreated) " +
            "VALUES (@Id, @UserId, @OtpCode, @ExpiresOn, @IsUsed, @DateCreated)";
        await dapperDb.ExecuteAsync(sql, userOtp);
    }

    public async Task<UserOtpDataModel?> GetActiveOtpAsync(Guid userId)
    {
        var sql =
            "SELECT TOP 1 * FROM [dbo].[UserOtp] " +
            "WHERE UserId = @UserId AND IsUsed = 0 AND ExpiresOn > @Now " +
            "ORDER BY DateCreated DESC";
        return await dapperDb.QuerySingleOrDefaultAsync<UserOtpDataModel>(sql, new { UserId = userId, Now = DateTime.UtcNow });
    }

    public async Task MarkOtpAsUsedAsync(Guid otpId)
    {
        var sql = "UPDATE [dbo].[UserOtp] SET IsUsed = 1 WHERE Id = @Id";
        await dapperDb.ExecuteAsync(sql, new { Id = otpId });
    }
}
