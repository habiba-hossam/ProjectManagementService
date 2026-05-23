using FluentAssertions;
using Moq;
using ProjectManagementAPI.Core.Application.Common.Exceptions;
using ProjectManagementAPI.Core.Application.Common.Interfaces;
using ProjectManagementAPI.Core.Application.Features.Tasks.Commands.CreateTask;
using ProjectManagementAPI.Core.Application.Features.Tasks.Commands.UpdateTaskStatus;
using ProjectManagementAPI.Core.Domain.Entities;
using ProjectManagementAPI.Core.Domain.Enums;
using Xunit;
using TaskStatus = ProjectManagementAPI.Core.Domain.Enums.TaskStatus;

namespace ProjectManagementAPI.Application.UnitTests.Features.Tasks;

public class CreateTaskCommandHandlerTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateTaskCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _projectId = Guid.NewGuid();

    public CreateTaskCommandHandlerTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(_userId);
        _handler = new CreateTaskCommandHandler(_taskRepositoryMock.Object, _projectRepositoryMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesTask()
    {
        // Arrange
        var project = new Project { Id = _projectId, UserId = _userId, Name = "Test Project" };
        _projectRepositoryMock.Setup(r => r.GetProjectByIdAndUserIdAsync(_projectId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        _taskRepositoryMock.Setup(r => r.AddAsync(It.IsAny<ProjectTask>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProjectTask t, CancellationToken _) => t);

        var command = new CreateTaskCommand(_projectId, "Fix Bug", "Description", null, TaskPriority.High);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Fix Bug");
        result.Status.Should().Be(TaskStatus.Todo);
        result.Priority.Should().Be(TaskPriority.High);
    }

    [Fact]
    public async Task Handle_ProjectNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _projectRepositoryMock.Setup(r => r.GetProjectByIdAndUserIdAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var command = new CreateTaskCommand(_projectId, "Fix Bug", "Desc", null, TaskPriority.Low);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}

public class UpdateTaskStatusCommandHandlerTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateTaskStatusCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public UpdateTaskStatusCommandHandlerTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(_userId);
        _handler = new UpdateTaskStatusCommandHandler(_taskRepositoryMock.Object, _projectRepositoryMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsUpdatedTask()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var project = new Project { Id = projectId, UserId = _userId };
        var task = new ProjectTask { Id = taskId, ProjectId = projectId, Status = TaskStatus.Todo, Title = "Task" };

        _projectRepositoryMock.Setup(r => r.GetProjectByIdAndUserIdAsync(projectId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        _taskRepositoryMock.Setup(r => r.GetTaskByIdAndProjectIdAsync(taskId, projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _taskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ProjectTask>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateTaskStatusCommand(projectId, taskId, TaskStatus.InProgress);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(TaskStatus.InProgress);
    }
}
