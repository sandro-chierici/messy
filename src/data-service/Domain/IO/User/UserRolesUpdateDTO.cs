namespace DataService.Domain.IO.User;

public record UserRolesUpdateDTO
{
    /// <summary>
    /// The complete set of role type codes the user must have.
    /// </summary>
    public List<string>? RoleCodes { get; set; }
}
