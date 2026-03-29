using DatabaseAdapter.Dapper;
using Models.DataModels;

namespace Services.Repositories.LoginSession;

public class LoginSessionRepository (IDapperDb dapperDb) : ILoginSessionRepository
{
    public async Task CreateLoginSessionAsync(LoginSessionDataModel loginSessionDataModel)
    {
        var sql =
            "INSERT INTO [dbo].[LoginSession] (Id, UserId, DeviceId, UserAgent, IpAddress, LastSeenOn, DateCreated) " +
            "VALUES (@Id, @UserId, @DeviceId, @UserAgent, @IpAddress, @LastSeenOn, @DateCreated)";
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
            "SET DeactivatedOn = @DeactivatedOn " +
            "WHERE Id = @Id AND DeactivatedOn IS NULL";
        await dapperDb.ExecuteAsync(sql, new { Id = sessionId, DeactivatedOn = DateTime.UtcNow });
    }

    public async Task<bool> IsValidLoginSessionAsync(Guid userId, Guid sessionId)
    {
        var sql =
            "SELECT 1 FROM [dbo].[LoginSession] " +
            "WHERE Id = @SessionId AND UserId = @UserId AND DeactivatedOn IS NULL";
        return await dapperDb.ExecuteScalarAsync<bool>(sql, new { SessionId = sessionId, UserId = userId });
    }
}
