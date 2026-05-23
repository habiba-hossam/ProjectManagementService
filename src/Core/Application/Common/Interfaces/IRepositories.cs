using ProjectManagementAPI.Core.Application.Common.Models;
using ProjectManagementAPI.Core.Domain.Entities;

namespace ProjectManagementAPI.Core.Application.Common.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}

public interface IProjectRepository : IRepository<Project>
{
    Task<PaginatedList<Project>> GetProjectsByUserIdAsync(Guid userId, PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<Project?> GetProjectByIdAndUserIdAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
}

public interface ITaskRepository : IRepository<ProjectTask>
{
    Task<PaginatedList<ProjectTask>> GetTasksByProjectIdAsync(Guid projectId, PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<ProjectTask?> GetTaskByIdAndProjectIdAsync(Guid taskId, Guid projectId, CancellationToken cancellationToken = default);
}
