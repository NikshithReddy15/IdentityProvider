using DatabaseAdapter.Dapper;
using Models.DataModels;

namespace Services.Repositories.User;

internal class UserRepository(IDapperDb dapperDb) : IUserRepository
{
    public async Task<bool> IsExistingUser(string email)
    {
        var sql =
            "SELECT 1 FROM [dbo].[User] " +
            "WHERE Email = @Email";
        return await dapperDb.ExecuteScalarAsync<bool>(sql, new { Email = email });
    }

    public async Task RegisterUser(UserDataModel user)
    {
        var sql =
            "INSERT INTO [dbo].[User](Id, FirstName, MiddleName, LastName, Email, CountryCode, MobileNumber, Password, DateCreated, DateModified) " +
            "VALUES (@Id, @FirstName, @MiddleName, @LastName, @Email, @CountryCode, @MobileNumber, @Password, @DateCreated, @DateModified)";
        await dapperDb.ExecuteAsync(sql, user);
    }

    public async Task<UserDataModel?> GetUserById(Guid userId)
    {
        var sql =
            "SELECT * FROM [dbo].[User] " +
            "WHERE Id = @UserId";
        return await dapperDb.QuerySingleOrDefaultAsync<UserDataModel>(sql, new { UserId = userId });
    }

    public async Task<UserDataModel?> GetUserByEmail(string email)
    {
        var sql =
            "SELECT * FROM [dbo].[User] " +
            "WHERE Email = @Email";
        return await dapperDb.QuerySingleOrDefaultAsync<UserDataModel>(sql, new { Email = email });
    }

    public async Task UpdateAccountStatusAsync(Guid userId, sbyte accountStatus)
    {
        var sql =
            "UPDATE [dbo].[User] SET AccountStatus = @AccountStatus, DateModified = @DateModified " +
            "WHERE Id = @Id";
        await dapperDb.ExecuteAsync(sql, new { Id = userId, AccountStatus = accountStatus, DateModified = DateTime.UtcNow });
    }
}