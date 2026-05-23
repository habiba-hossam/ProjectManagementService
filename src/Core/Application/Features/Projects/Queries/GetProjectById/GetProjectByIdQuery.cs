using AutoMapper;
using MediatR;
using ProjectManagementAPI.Core.Application.Common.Exceptions;
using ProjectManagementAPI.Core.Application.Common.Interfaces;
using ProjectManagementAPI.Core.Domain.Entities;

namespace ProjectManagementAPI.Core.Application.Features.Projects.Queries.GetProjectById;

public record GetProjectByIdQuery(Guid Id) : IRequest<ProjectDto>;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public GetProjectByIdQueryHandler(IProjectRepository projectRepository, ICurrentUserService currentUserService, ICacheService cacheService, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<ProjectDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"project:{request.Id}";
        var cached = await _cacheService.GetAsync<ProjectDto>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var project = await _projectRepository.GetProjectByIdAndUserIdAsync(request.Id, _currentUserService.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.Id);

        var dto = _mapper.Map<ProjectDto>(project);
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10), cancellationToken);
        return dto;
    }
}
