using FluentAssertions;
using Moq;
using ProjectManagementAPI.Core.Application.Common.Exceptions;
using ProjectManagementAPI.Core.Application.Common.Interfaces;
using ProjectManagementAPI.Core.Application.Features.Projects.Commands.CreateProject;
using ProjectManagementAPI.Core.Application.Features.Projects.Commands.DeleteProject;
using ProjectManagementAPI.Core.Domain.Entities;
using Xunit;

namespace ProjectManagementAPI.Application.UnitTests.Features.Projects;

public class CreateProjectCommandHandlerTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateProjectCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public CreateProjectCommandHandlerTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(_userId);
        _handler = new CreateProjectCommandHandler(_projectRepositoryMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsProjectDto()
    {
        // Arrange
        var command = new CreateProjectCommand("My Project", "A test project");
        _projectRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project p, CancellationToken _) => p);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("My Project");
        result.Description.Should().Be("A test project");
        result.TaskCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsCorrectUserId()
    {
        // Arrange
        var command = new CreateProjectCommand("My Project", "A test project");
        Project? capturedProject = null;
        _projectRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .Callback<Project, CancellationToken>((p, _) => capturedProject = p)
            .ReturnsAsync((Project p, CancellationToken _) => p);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedProject.Should().NotBeNull();
        capturedProject!.UserId.Should().Be(_userId);
    }
}

public class DeleteProjectCommandHandlerTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly DeleteProjectCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public DeleteProjectCommandHandlerTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(_userId);
        _handler = new DeleteProjectCommandHandler(_projectRepositoryMock.Object, _currentUserServiceMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingProject_DeletesSuccessfully()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = new Project { Id = projectId, UserId = _userId, Name = "Test" };
        _projectRepositoryMock.Setup(r => r.GetProjectByIdAndUserIdAsync(projectId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _cacheServiceMock.Setup(c => c.RemoveByPatternAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(new DeleteProjectCommand(projectId), CancellationToken.None);

        // Assert
        _projectRepositoryMock.Verify(r => r.DeleteAsync(project, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsNotFoundException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _projectRepositoryMock.Setup(r => r.GetProjectByIdAndUserIdAsync(projectId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        // Act
        var act = async () => await _handler.Handle(new DeleteProjectCommand(projectId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
