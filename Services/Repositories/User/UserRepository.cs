using DatabaseAdapter.Dapper;
using Models.DataModels;

namespace Services.Repositories.User;

public class UserRepository(IDapperDb dapperDb) : IUserRepository
{
    public async Task<bool> IsExistingUser(string email)
    {
        var sql = "SELECT 1 FROM [dbo].[User] WHERE Email = @Email";
        return await dapperDb.QueryFirstOrDefaultAsync<bool>(sql, new { Email = email });
    }

    public async Task RegisterUser(UserDataModel user)
    {
        var sql =
            "INSERT INTO [dbo].[User](FirstName, MiddleName, LastName, Email, CountryCode, MobileNumber, Password, DateCreated, DateModified) " +
            "VALUES (@FirstName, @MiddleName, @LastName, @Email, @CountryCode, @MobileNumber, @Password, @DateCreated, @DateModified)";
        await dapperDb.ExecuteAsync<UserDataModel>(sql, user);
    }

    public async Task<UserDataModel?> GetUserByEmail(string email)
    {
        var sql = "SELECT * FROM [dbo].[User] WHERE Email = @Email";
        return await dapperDb.QueryFirstOrDefaultAsync<UserDataModel>(sql, new { Email = email });
    }
}