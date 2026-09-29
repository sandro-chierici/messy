namespace DataService.Domain.IO.User;

public record UserSecurityGroupsUpdateDTO
{
    /// <summary>
    /// The complete set of security group external ids (GUID) the user must have.
    /// </summary>
    public List<string>? SecurityGroupIds { get; set; }
}
