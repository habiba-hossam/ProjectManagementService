using ProjectManagementAPI.Core.Domain.Common;
using ProjectManagementAPI.Core.Domain.Enums;

namespace ProjectManagementAPI.Core.Domain.Entities;

public class ProjectTask : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.Todo;
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;
}
