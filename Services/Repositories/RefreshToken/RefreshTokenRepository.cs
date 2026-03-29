using DatabaseAdapter.Dapper;
using Models.DataModels;

namespace Services.Repositories.RefreshToken;

public class RefreshTokenRepository (IDapperDb dapperDb) : IRefreshTokenRepository
{
    public async Task CreateRefreshToken(RefreshTokenDataModel refreshToken)
    {
        var sql =
            "INSERT INTO [dbo].[RefreshToken](Id, UserId, SessionId, RefreshToken, ExpiresOn, DateCreated) " +
            "VALUES (@Id, @UserId, @SessionId, @RefreshToken, @ExpiresOn, @DateCreated)";
        await dapperDb.ExecuteAsync(sql, refreshToken);
    }

    public async Task<RefreshTokenDataModel?> GetRefreshToken(Guid userId, Guid sessionId)
    {
        var sql =
            "SELECT * FROM [dbo].[RefreshToken] " +
            "WHERE UserId = @UserId AND SessionId = @SessionId AND RevokedOn IS NULL";
        return await dapperDb.QuerySingleOrDefaultAsync<RefreshTokenDataModel>(sql, new { UserId = userId, SessionId = sessionId });
    }

    public async Task RevokeRefreshToken(string refreshToken)
    {
        var sql =
            "UPDATE [dbo].[RefreshToken] " +
            "SET RevokedOn = @RevokedOn " +
            "WHERE RefreshToken = @RefreshToken AND RevokedOn IS NULL";
        await dapperDb.ExecuteAsync(sql, new { RefreshToken = refreshToken, RevokedOn = DateTime.UtcNow });
    }

    public async Task RevokeRefreshTokenByIdAsync(Guid id)
    {
        var sql =
            "UPDATE [dbo].[RefreshToken] " +
            "SET RevokedOn = @RevokedOn " +
            "WHERE Id = @Id AND RevokedOn IS NULL";
        await dapperDb.ExecuteAsync(sql, new { Id = id, RevokedOn = DateTime.UtcNow });
    }

    public async Task<IEnumerable<Guid>> RevokeExpiredRefreshTokensAsync()
    {
        var sql =
            "UPDATE [dbo].[RefreshToken] " +
            "SET RevokedOn = @RevokedOn " +
            "OUTPUT INSERTED.SessionId " +
            "WHERE ExpiresOn < @Now AND RevokedOn IS NULL";
        return await dapperDb.QueryAsync<Guid>(sql, new { RevokedOn = DateTime.UtcNow, Now = DateTime.UtcNow });
    }
}
