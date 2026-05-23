using FluentValidation;
using MediatR;
using ProjectManagementAPI.Core.Application.Common.Exceptions;
using ProjectManagementAPI.Core.Application.Common.Interfaces;
using ProjectManagementAPI.Core.Domain.Entities;
using ProjectManagementAPI.Core.Domain.Enums;

namespace ProjectManagementAPI.Core.Application.Features.Tasks.Commands.CreateTask;

public record CreateTaskCommand(Guid ProjectId, string Title, string Description, DateTime? DueDate, TaskPriority Priority) : IRequest<TaskDto>;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.DueDate).GreaterThan(DateTime.UtcNow).When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be in the future.");
    }
}

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateTaskCommandHandler(ITaskRepository taskRepository, IProjectRepository projectRepository, ICurrentUserService currentUserService)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        // Validate that the project exists and belongs to the current user
        var project = await _projectRepository.GetProjectByIdAndUserIdAsync(request.ProjectId, _currentUserService.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var task = new ProjectTask
        {
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            Priority = request.Priority,
            ProjectId = project.Id,
            Status = ProjectTaskStatus.Todo
        };

        await _taskRepository.AddAsync(task, cancellationToken);

        return MapToDto(task);
    }

    private static TaskDto MapToDto(ProjectTask task) =>
        new(task.Id, task.Title, task.Description, task.Status,
            task.Status.ToString(), task.DueDate, task.Priority,
            task.Priority.ToString(), task.ProjectId, task.CreatedAt);
}
