using AutoMapper;
using FluentValidation;
using MediatR;
using ProjectManagementAPI.Core.Application.Common.Exceptions;
using ProjectManagementAPI.Core.Application.Common.Interfaces;
using ProjectManagementAPI.Core.Domain.Entities;
using ProjectManagementAPI.Core.Domain.Enums;

namespace ProjectManagementAPI.Core.Application.Features.Tasks.Commands.UpdateTaskStatus;

public record UpdateTaskStatusCommand(Guid ProjectId, Guid TaskId, ProjectTaskStatus Status) : IRequest<TaskDto>;

public class UpdateTaskStatusCommandValidator : AbstractValidator<UpdateTaskStatusCommand>
{
    public UpdateTaskStatusCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum().WithMessage("Invalid task status.");
    }
}

public class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, TaskDto>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public UpdateTaskStatusCommandHandler(ITaskRepository taskRepository, IProjectRepository projectRepository, ICurrentUserService currentUserService, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<TaskDto> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        // Validate that the project exists and belongs to the current user
        var project = await _projectRepository.GetProjectByIdAndUserIdAsync(request.ProjectId, _currentUserService.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        // Validate that the task exists within the project
        var task = await _taskRepository.GetTaskByIdAndProjectIdAsync(request.TaskId, project.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ProjectTask), request.TaskId);

        task.Status = request.Status;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task, cancellationToken);

        return _mapper.Map<TaskDto>(task);
    }
}
