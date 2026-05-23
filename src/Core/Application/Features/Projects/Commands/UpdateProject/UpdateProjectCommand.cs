using AutoMapper;
using FluentValidation;
using MediatR;
using ProjectManagementAPI.Core.Application.Common.Exceptions;
using ProjectManagementAPI.Core.Application.Common.Interfaces;
using ProjectManagementAPI.Core.Domain.Entities;

namespace ProjectManagementAPI.Core.Application.Features.Projects.Commands.UpdateProject;

public record UpdateProjectCommand(Guid Id, string Name, string Description) : IRequest<ProjectDto>;

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }
}

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public UpdateProjectCommandHandler(IProjectRepository projectRepository, ICurrentUserService currentUserService, ICacheService cacheService, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<ProjectDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetProjectByIdAndUserIdAsync(request.Id, _currentUserService.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.Id);

        project.Name = request.Name;
        project.Description = request.Description;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(project, cancellationToken);
        await _cacheService.RemoveAsync($"project:{request.Id}", cancellationToken);
        await _cacheService.IncrementVersionAsync($"projects:user:{_currentUserService.UserId}:version");

        return _mapper.Map<ProjectDto>(project);
    }
}
