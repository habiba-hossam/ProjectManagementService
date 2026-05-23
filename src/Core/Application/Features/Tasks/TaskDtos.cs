using ProjectManagementAPI.Core.Domain.Enums;

namespace ProjectManagementAPI.Core.Application.Features.Tasks;

public class TaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProjectTaskStatus Status { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; }
    public string PriorityLabel { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; }
}

public class UpdateTaskStatusRequest
{
    public ProjectTaskStatus Status { get; set; }
}
