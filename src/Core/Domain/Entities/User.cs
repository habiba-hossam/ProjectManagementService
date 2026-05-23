using ProjectManagementAPI.Core.Domain.Common;

namespace ProjectManagementAPI.Core.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";

    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
