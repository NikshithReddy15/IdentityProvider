using DatabaseAdapter.Dapper;
using Models.DataModels;

namespace Services.Repositories.RevokedToken;

public class RevokedTokenRepository(IDapperDb dapperDb) : IRevokedTokenRepository
{
    public async Task<bool> IsRevokedToken(string hashedToken)
    {
        var sql =
            "SELECT 1 FROM [dbo].[RevokedToken] " +
            "WHERE HashedToken = @HashedToken AND ExpiresOn > @CurrentDate";
        return await dapperDb.ExecuteScalarAsync<bool>(sql, new { HashedToken = hashedToken, CurrentDate = DateTime.UtcNow });
    }

    public async Task CreateRevokedToken(RevokedTokenDataModel revokedToken)
    {
        var sql =
            "INSERT INTO [dbo].[RevokedToken] (HashedToken, ExpiresOn, DateCreated) " +
            "VALUES (@HashedToken, @ExpiresOn, @DateCreated)";
        await dapperDb.ExecuteAsync(sql, revokedToken);
    }
}