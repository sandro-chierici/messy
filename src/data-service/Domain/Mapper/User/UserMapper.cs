using DataService.Domain.IO.User;
using DataService.Domain.Repository.Models;
using DataService.Domain.Tools;

namespace DataService.Domain.Mapper.User;

public class UserMapper(
    SwissKnife swissKnife)
{
    public UserViewDTO MapUserViewFrom(UserDetail user, IEnumerable<string> roleCodes, IEnumerable<Guid> securityGroupIds) =>
        new UserViewDTO
        {
            UserId = $"{user.PartyId}",
            TenantId = $"{user.TenantId}",
            UserLoginId = $"{user.UserLoginId}",
            Username = user.Username,
            Enabled = user.Enabled,
            IsSystem = user.IsSystem,
            RequirePasswordChange = user.RequirePasswordChange,
            LastLocale = user.LastLocale,
            LastTimeZone = user.LastTimeZone,
            Salutation = user.Salutation,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            LastName = user.LastName,
            Nickname = user.Nickname,
            PersonalTitle = user.PersonalTitle,
            Suffix = user.Suffix,
            Gender = user.Gender,
            BirthDate = user.BirthDate,
            Comments = user.Comments,
            ExternalId = user.ExternalId,
            Description = user.Description,
            IsActive = user.IsActive,
            CreatedUTCDate = user.CreatedUTCDate,
            UpdatedUTCDate = user.UpdatedUTCDate,
            CreatedBy = user.CreatedBy,
            RoleCodes = roleCodes.ToList(),
            SecurityGroupIds = securityGroupIds.Select(g => $"{g}").ToList(),
            ExtProps = swissKnife.DeserializeExtProps(user.ExtProps)
        };

    /// <summary>
    /// Maps to a new Party. TenantPk and PartyTypePk are resolved by the repository.
    /// </summary>
    public Party MapPartyFrom(UserCreateDTO user, Guid partyId) =>
        new Party
        {
            PartyId = partyId,
            ExternalId = user.ExternalId,
            Description = user.Description,
            IsActive = user.Enabled,
            CreatedUTCDate = DateTime.UtcNow,
            ExtProps = swissKnife.SerializeExtProps(user.ExtProps)
        };

    public Party MapPartyFrom(UserUpdateDTO user, Guid partyId) =>
        new Party
        {
            PartyId = partyId,
            ExternalId = user.ExternalId,
            Description = user.Description,
            IsActive = user.Enabled,
            UpdatedUTCDate = DateTime.UtcNow,
            ExtProps = swissKnife.SerializeExtProps(user.ExtProps)
        };

    /// <summary>
    /// Maps to a new Person. PartyPk is resolved by the repository.
    /// </summary>
    public Person MapPersonFrom(UserCreateDTO user) =>
        new Person
        {
            Salutation = user.Salutation,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            LastName = user.LastName,
            Nickname = user.Nickname,
            PersonalTitle = user.PersonalTitle,
            Suffix = user.Suffix,
            Gender = user.Gender,
            BirthDate = user.BirthDate,
            Comments = user.Comments
        };

    public Person MapPersonFrom(UserUpdateDTO user) =>
        new Person
        {
            Salutation = user.Salutation,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            LastName = user.LastName,
            Nickname = user.Nickname,
            PersonalTitle = user.PersonalTitle,
            Suffix = user.Suffix,
            Gender = user.Gender,
            BirthDate = user.BirthDate,
            Comments = user.Comments
        };

    /// <summary>
    /// Maps to a new UserLogin. TenantPk and PartyPk are resolved by the repository.
    /// </summary>
    public UserLogin MapUserLoginFrom(UserCreateDTO user, Guid userLoginId, string passwordHash) =>
        new UserLogin
        {
            UserLoginId = userLoginId,
            Username = user.Username!,
            PasswordHash = passwordHash,
            Enabled = user.Enabled,
            RequirePasswordChange = user.RequirePasswordChange,
            LastLocale = user.LastLocale,
            LastTimeZone = user.LastTimeZone,
            DisabledUTCDate = user.Enabled ? null : DateTime.UtcNow,
            CreatedUTCDate = DateTime.UtcNow
        };
}
