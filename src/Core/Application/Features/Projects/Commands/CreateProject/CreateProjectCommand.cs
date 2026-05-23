using AutoMapper;
using FluentValidation;
using MediatR;
using ProjectManagementAPI.Core.Application.Common.Interfaces;
using ProjectManagementAPI.Core.Domain.Entities;

namespace ProjectManagementAPI.Core.Application.Features.Projects.Commands.CreateProject;

public record CreateProjectCommand(string Name, string Description) : IRequest<ProjectDto>;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }
}

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public CreateProjectCommandHandler(IProjectRepository projectRepository, ICurrentUserService currentUserService, ICacheService cacheService, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            UserId = _currentUserService.UserId
        };

        await _projectRepository.AddAsync(project, cancellationToken);
        await _cacheService.IncrementVersionAsync($"projects:user:{_currentUserService.UserId}:version");
        return _mapper.Map<ProjectDto>(project);
    }
}
