using ProjectManagementAPI.Core.Domain.Enums;

namespace ProjectManagementAPI.Core.Application.Features.Tasks;

public record TaskDto(
    Guid Id,
    string Title,
    string Description,
    ProjectTaskStatus Status,
    string StatusLabel,
    DateTime? DueDate,
    TaskPriority Priority,
    string PriorityLabel,
    Guid ProjectId,
    DateTime CreatedAt);

public record CreateTaskRequest(string Title, string Description, DateTime? DueDate, TaskPriority Priority);
public record UpdateTaskStatusRequest(ProjectTaskStatus Status);
