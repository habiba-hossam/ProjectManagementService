using FluentAssertions;
using Moq;
using ProjectManagementAPI.Core.Application.Common.Exceptions;
using ProjectManagementAPI.Core.Application.Common.Interfaces;
using ProjectManagementAPI.Core.Application.Features.Auth.Commands.Register;
using ProjectManagementAPI.Core.Domain.Entities;
using Xunit;

namespace ProjectManagementAPI.Application.UnitTests.Features.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _jwtServiceMock = new Mock<IJwtService>();
        _handler = new RegisterCommandHandler(_userRepositoryMock.Object, _passwordServiceMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsAuthResponseDto()
    {
        // Arrange
        var command = new RegisterCommand("John Doe", "john@example.com", "Password1!");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _passwordServiceMock.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashed_password");
        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);
        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("john@example.com");
        result.FullName.Should().Be("John Doe");
        result.Token.Should().Be("jwt_token");
        result.Role.Should().Be("User");
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsConflictException()
    {
        // Arrange
        var command = new RegisterCommand("John Doe", "john@example.com", "Password1!");
        var existingUser = new User { Email = "john@example.com" };
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*john@example.com*");
    }

    [Fact]
    public async Task Handle_ValidCommand_HashesPassword()
    {
        // Arrange
        var command = new RegisterCommand("Jane Doe", "jane@example.com", "Password1!");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _passwordServiceMock.Setup(p => p.HashPassword("Password1!")).Returns("hashed").Verifiable();
        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);
        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("token");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _passwordServiceMock.Verify(p => p.HashPassword("Password1!"), Times.Once);
    }
}
