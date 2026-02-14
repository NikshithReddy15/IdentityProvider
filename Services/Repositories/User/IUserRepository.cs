using Models.DataModels;

namespace Services.Repositories.User;

public interface IUserRepository
{
    Task<bool> IsExistingUser(string email);

    Task RegisterUser(UserDataModel user);

    Task<UserDataModel?> GetUserById(Guid userId);

    Task<UserDataModel?> GetUserByEmail(string email);
}