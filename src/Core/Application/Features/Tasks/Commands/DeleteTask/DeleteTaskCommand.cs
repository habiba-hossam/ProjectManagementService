using MediatR;
using ProjectManagementAPI.Core.Application.Common.Exceptions;
using ProjectManagementAPI.Core.Application.Common.Interfaces;
using ProjectManagementAPI.Core.Domain.Entities;

namespace ProjectManagementAPI.Core.Application.Features.Tasks.Commands.DeleteTask;

public record DeleteTaskCommand(Guid ProjectId, Guid TaskId) : IRequest<Unit>;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Unit>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteTaskCommandHandler(ITaskRepository taskRepository, IProjectRepository projectRepository, ICurrentUserService currentUserService)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        // Validate that the project exists and belongs to the current user
        var project = await _projectRepository.GetProjectByIdAndUserIdAsync(request.ProjectId, _currentUserService.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        // Validate that the task exists within the project
        var task = await _taskRepository.GetTaskByIdAndProjectIdAsync(request.TaskId, project.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ProjectTask), request.TaskId);

        await _taskRepository.DeleteAsync(task, cancellationToken);
        return Unit.Value;
    }
}
