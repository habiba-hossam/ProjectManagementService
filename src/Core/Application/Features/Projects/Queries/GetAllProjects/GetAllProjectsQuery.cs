using MediatR;
using ProjectManagementAPI.Core.Application.Common.Interfaces;
using ProjectManagementAPI.Core.Application.Common.Models;

namespace ProjectManagementAPI.Core.Application.Features.Projects.Queries.GetAllProjects;

public record GetAllProjectsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<ProjectDto>>;

public class GetAllProjectsQueryHandler : IRequestHandler<GetAllProjectsQuery, PaginatedList<ProjectDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    

    public GetAllProjectsQueryHandler(IProjectRepository projectRepository, ICurrentUserService currentUserService, ICacheService cacheService)
    {
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<PaginatedList<ProjectDto>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
    {
        var versionKey = $"projects:user:{_currentUserService.UserId}:version";
        var version = await _cacheService.GetVersionAsync(versionKey);

        var cacheKey = $"projects:user:{_currentUserService.UserId}:v{version}:page:{request.PageNumber}:size:{request.PageSize}";

        var cached = await _cacheService.GetAsync<PaginatedList<ProjectDto>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var pagination = new PaginationParams { PageNumber = request.PageNumber, PageSize = request.PageSize };
        var projects = await _projectRepository.GetProjectsByUserIdAsync(_currentUserService.UserId, pagination, cancellationToken);

        var result = new PaginatedList<ProjectDto>
        {
            Items = projects.Items.Select(p => new ProjectDto(p.Id, p.Name, p.Description, p.CreatedAt, p.Tasks.Count)),
            TotalCount = projects.TotalCount,
            PageNumber = projects.PageNumber,
            PageSize = projects.PageSize
        };

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);
        return result;
    }
}
