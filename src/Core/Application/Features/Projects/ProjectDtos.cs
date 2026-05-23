namespace ProjectManagementAPI.Core.Application.Features.Projects;

public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TaskCount { get; set; }
}

public class CreateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
