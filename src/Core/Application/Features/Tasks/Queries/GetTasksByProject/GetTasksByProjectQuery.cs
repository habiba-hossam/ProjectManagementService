using AutoMapper;
using MediatR;
using ProjectManagementAPI.Core.Application.Common.Exceptions;
using ProjectManagementAPI.Core.Application.Common.Interfaces;
using ProjectManagementAPI.Core.Application.Common.Models;
using ProjectManagementAPI.Core.Domain.Entities;

namespace ProjectManagementAPI.Core.Application.Features.Tasks.Queries.GetTasksByProject;

public record GetTasksByProjectQuery(Guid ProjectId, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<TaskDto>>;

public class GetTasksByProjectQueryHandler : IRequestHandler<GetTasksByProjectQuery, PaginatedList<TaskDto>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetTasksByProjectQueryHandler(ITaskRepository taskRepository, IProjectRepository projectRepository, ICurrentUserService currentUserService, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<PaginatedList<TaskDto>> Handle(GetTasksByProjectQuery request, CancellationToken cancellationToken)
    {
        // Validate that the project exists and belongs to the current user
        var project = await _projectRepository.GetProjectByIdAndUserIdAsync(request.ProjectId, _currentUserService.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var pagination = new PaginationParams { PageNumber = request.PageNumber, PageSize = request.PageSize };
        var tasks = await _taskRepository.GetTasksByProjectIdAsync(project.Id, pagination, cancellationToken);

        return new PaginatedList<TaskDto>
        {
            Items = tasks.Items.Select(t => _mapper.Map<TaskDto>(t)).ToList()            
        };
    }
}
