using DataService.Domain.IO;
using DataService.Domain.IO.User;
using DataService.Domain.Rules;

namespace DataService.Domain.Services;

public interface IUserService : IBackendService
{
    Task<OkOrError<string>> CreateUserAsync(string tenantId, UserCreateDTO command);
    Task<OkOrError<UserViewDTO>> GetUserAsync(string tenantId, string userId);
    Task<OkOrError<QueryResult>> ListUsersAsync(string tenantId, int page, int pageSize);
    Task<OkOrError<UserViewDTO>> UpdateUserAsync(string tenantId, string userId, UserUpdateDTO command);
    Task<OkOrError<int>> DisableUserAsync(string tenantId, string userId);
    Task<OkOrError<int>> SetUserRolesAsync(string tenantId, string userId, UserRolesUpdateDTO command);
    Task<OkOrError<int>> SetUserSecurityGroupsAsync(string tenantId, string userId, UserSecurityGroupsUpdateDTO command);
}
