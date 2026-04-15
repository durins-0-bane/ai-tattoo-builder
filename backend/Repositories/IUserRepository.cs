using TattooShop.Api.Models;

namespace TattooShop.Api.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByGoogleSubjectAsync(string googleSubject);
    Task UpsertAsync(User user);
}
