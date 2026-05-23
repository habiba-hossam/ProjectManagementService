using Microsoft.EntityFrameworkCore;
using ProjectManagementAPI.Core.Application.Common.Interfaces;
using ProjectManagementAPI.Core.Application.Common.Models;
using ProjectManagementAPI.Core.Domain.Entities;
using ProjectManagementAPI.Infrastructure.Data;

namespace ProjectManagementAPI.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
}

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(AppDbContext context) : base(context) { }

    public async Task<PaginatedList<Project>> GetProjectsByUserIdAsync(Guid userId, PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(p => p.Tasks)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<Project>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    public async Task<Project?> GetProjectByIdAndUserIdAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default) =>
        await _dbSet
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId, cancellationToken);
}

public class TaskRepository : Repository<ProjectTask>, ITaskRepository
{
    public TaskRepository(AppDbContext context) : base(context) { }

    public async Task<PaginatedList<ProjectTask>> GetTasksByProjectIdAsync(Guid projectId, PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<ProjectTask>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    public async Task<ProjectTask?> GetTaskByIdAndProjectIdAsync(Guid taskId, Guid projectId, CancellationToken cancellationToken = default) =>
        await _dbSet.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId, cancellationToken);
}
