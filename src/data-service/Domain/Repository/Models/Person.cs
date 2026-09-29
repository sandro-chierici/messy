namespace DataService.Domain.Repository.Models;

/// <summary>
/// OFBiz Person: 1:1 subtype of <see cref="Party"/>, keyed by the party primary key.
/// </summary>
public class Person
{
    public int PartyPk { get; set; }
    public string? Salutation { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Nickname { get; set; }
    public string? PersonalTitle { get; set; }
    public string? Suffix { get; set; }

    /// <summary>
    /// Single letter gender code ("M", "F", ...).
    /// </summary>
    public string? Gender { get; set; }

    public DateTime? BirthDate { get; set; }
    public string? Comments { get; set; }
}
