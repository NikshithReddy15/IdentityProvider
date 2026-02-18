using DatabaseAdapter.Dapper;
using Models.DataModels;

namespace Services.Repositories.LoginSession;

public class LoginSessionRepository (IDapperDb dapperDb) : ILoginSessionRepository
{
    public async Task CreateLoginSessionAsync(LoginSessionDataModel loginSessionDataModel)
    {
        var sql =
            "INSERT INTO [dbo].[LoginSession] (Id, UserId, DeviceId, DeviceName, UserAgent, IpAddress, IsActive, LastSeenOn, DateCreated) " +
            "VALUES (@Id, @UserId, @DeviceId, @DeviceName, @UserAgent, @IpAddress, @IsActive, @LastSeenOn, @DateCreated)";
        await dapperDb.ExecuteAsync(sql, loginSessionDataModel);
    }

    public async Task<LoginSessionDataModel?> GetLoginSessionAsync(Guid sessionId)
    {
        var sql =
            "SELECT * FROM [dbo].[LoginSession] " +
            "WHERE Id = @Id";
        return await dapperDb.QuerySingleOrDefaultAsync<LoginSessionDataModel>(sql, new { Id = sessionId });
    }

    public async Task DeactivateLoginSessionAsync(Guid sessionId)
    {
        var sql =
            "UPDATE [dbo].[LoginSession] " +
            "SET IsActive = 0, LastSeenOn = @LastSeenOn " +
            "WHERE Id = @Id AND IsActive = 1";
        await dapperDb.ExecuteAsync(sql, new { Id = sessionId, LastSeenOn = DateTime.UtcNow });
    }

    public async Task<bool> IsValidLoginSessionAsync(Guid userId, Guid sessionId)
    {
        var sql =
            "SELECT 1 FROM [dbo].[LoginSession] " +
            "WHERE Id = @SessionId AND UserId = @UserId AND IsActive = 1";
        return await dapperDb.ExecuteScalarAsync<bool>(sql, new { Id = sessionId, UserId = userId });
    }
}