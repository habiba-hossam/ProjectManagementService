using MediatR;
using ProjectManagementAPI.Core.Application.Common.Exceptions;
using ProjectManagementAPI.Core.Application.Common.Interfaces;
using ProjectManagementAPI.Core.Domain.Entities;

namespace ProjectManagementAPI.Core.Application.Features.Projects.Commands.DeleteProject;

public record DeleteProjectCommand(Guid Id) : IRequest<Unit>;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Unit>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public DeleteProjectCommandHandler(IProjectRepository projectRepository, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<Unit> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetProjectByIdAndUserIdAsync(request.Id, _currentUserService.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.Id);

        await _projectRepository.DeleteAsync(project, cancellationToken);
        await _cacheService.RemoveAsync($"project:{request.Id}", cancellationToken);
        await _cacheService.IncrementVersionAsync($"projects:user:{_currentUserService.UserId}:version");

        return Unit.Value;
    }
}
