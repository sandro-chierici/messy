using DataService.Domain.IO;
using DataService.Domain.IO.User;
using DataService.Domain.Rules;
using DataService.Domain.Services.Events;

namespace DataService.Domain.Repository;

/// <summary>
/// Users of a tenant: Party (PERSON) + Person + UserLogin + roles + security groups.
/// All identifiers crossing this interface are external GUIDs.
/// </summary>
public interface IUserRepository : IRepository
{
    public Task<OkOrError<string>> CreateUserAsync(Guid tenantId, Guid partyId, Guid userLoginId, UserCreateDTO command, IReadOnlyCollection<Guid> securityGroupIds, string passwordHash, EventBase? successEvent = null);
    public Task<OkOrError<UserViewDTO>> GetUserAsync(Guid tenantId, Guid userId);
    public Task<OkOrError<QueryResult>> ListUsersAsync(Guid tenantId, int page, int pageSize);
    public Task<OkOrError<UserViewDTO>> UpdateUserAsync(Guid tenantId, Guid userId, UserUpdateDTO command, string? passwordHash);
    public Task<OkOrError<int>> DisableUserAsync(Guid tenantId, Guid userId);
    public Task<OkOrError<int>> SetUserRolesAsync(Guid tenantId, Guid userId, IReadOnlyCollection<string> roleCodes);
    public Task<OkOrError<int>> SetUserSecurityGroupsAsync(Guid tenantId, Guid userId, IReadOnlyCollection<Guid> securityGroupIds);
    public Task<OkOrError<int>> CountEnabledUsersAsync(Guid tenantId);
}
