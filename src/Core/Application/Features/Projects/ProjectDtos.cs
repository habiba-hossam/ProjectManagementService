namespace ProjectManagementAPI.Core.Application.Features.Projects;

public record ProjectDto(Guid Id, string Name, string Description, DateTime CreatedAt, int TaskCount);
public record CreateProjectRequest(string Name, string Description);
public record UpdateProjectRequest(string Name, string Description);
