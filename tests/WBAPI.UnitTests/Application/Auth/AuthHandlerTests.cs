using FluentAssertions;
using Moq;
using WBAPI.Application.Commands.Auth;
using WBAPI.Application.Interfaces;
using WBAPI.Domain.Entities;
using WBAPI.Domain.Ports;

namespace WBAPI.UnitTests.Application.Auth;

public class RegisterHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IJwtService> _jwt = new();
    private readonly RegisterHandler _sut;

    public RegisterHandlerTests()
    {
        _sut = new RegisterHandler(_userRepo.Object, _uow.Object, _jwt.Object);
    }

    [Fact]
    public async Task Handle_NewUser_ReturnsAuthResponse()
    {
        // Arrange
        _userRepo.Setup(r => r.GetByUsernameAsync("alice", default))
                 .ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>(), default))
                 .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("mocked-jwt-token");

        var cmd = new RegisterCommand("alice", "P@ssw0rd", "User");

        // Act
        var result = await _sut.Handle(cmd, default);

        // Assert
        result.AccessToken.Should().Be("mocked-jwt-token");
        result.Username.Should().Be("alice");
        result.Role.Should().Be("User");
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateUsername_ThrowsInvalidOperationException()
    {
        // Arrange
        var existing = User.Create("alice", "hashedpw", "User");
        _userRepo.Setup(r => r.GetByUsernameAsync("alice", default))
                 .ReturnsAsync(existing);

        var cmd = new RegisterCommand("alice", "P@ssw0rd", "User");

        // Act
        var act = () => _sut.Handle(cmd, default);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*alice*");
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
}

public class LoginHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IJwtService> _jwt = new();
    private readonly LoginHandler _sut;

    public LoginHandlerTests()
    {
        _sut = new LoginHandler(_userRepo.Object, _jwt.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var hash = BCrypt.Net.BCrypt.HashPassword("secret");
        var user = User.Create("bob", hash, "Admin");
        _userRepo.Setup(r => r.GetByUsernameAsync("bob", default)).ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateToken(user)).Returns("token-for-bob");

        var cmd = new LoginCommand("bob", "secret");

        // Act
        var result = await _sut.Handle(cmd, default);

        // Assert
        result.AccessToken.Should().Be("token-for-bob");
        result.Username.Should().Be("bob");
        result.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorized()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync("unknown", default))
                 .ReturnsAsync((User?)null);

        var act = () => _sut.Handle(new LoginCommand("unknown", "pw"), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials.");
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsUnauthorized()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("correct");
        var user = User.Create("carol", hash, "User");
        _userRepo.Setup(r => r.GetByUsernameAsync("carol", default)).ReturnsAsync(user);

        var act = () => _sut.Handle(new LoginCommand("carol", "wrong"), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials.");
    }
}
