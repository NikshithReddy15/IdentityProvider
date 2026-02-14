using DatabaseAdapter.Dapper;
using Models.DataModels;

namespace Services.Repositories.RefreshToken;

public class RefreshTokenRepository (IDapperDb dapperDb) : IRefreshTokenRepository
{
    public async Task CreateRefreshToken(RefreshTokenDataModel refreshToken)
    {
        var sql =
            "INSERT INTO [dbo].[RefreshToken](UserId, RefreshToken, ExpiresOn, IsRevoked, DateCreated) " +
            "VALUES (@UserId, @RefreshToken, @ExpiresOn, @IsRevoked, @DateCreated)";
        await dapperDb.ExecuteAsync(sql, refreshToken);
    }

    public async Task<RefreshTokenDataModel?> GetRefreshToken(Guid userId)
    {
        var sql =
            "SELECT * FROM [dbo].[RefreshToken] " +
            "WHERE UserId = @UserId AND IsRevoked = 0";
        return await dapperDb.QuerySingleOrDefaultAsync<RefreshTokenDataModel>(sql, new { UserId = userId });
    }
    
    public async Task RevokeRefreshToken(string refreshToken)
    {
        var sql =
            "UPDATE [dbo].[RefreshToken] " +
            "SET IsRevoked = 1 " +
            "WHERE RefreshToken = @RefreshToken AND IsRevoked = 0";
        await dapperDb.ExecuteAsync(sql, new { RefreshToken = refreshToken });
    }
}