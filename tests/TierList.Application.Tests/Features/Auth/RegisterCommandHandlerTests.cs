using FluentAssertions;
using Moq;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.Auth.Commands.Register;
using Xunit;

namespace TierList.Application.Tests.Features.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(_identityServiceMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldReturnAuthResultDto()
    {
        // Arrange
        var command = new RegisterCommand("john_doe", "john@example.com", "Password123!");
        var userId = Guid.NewGuid();
        var roles = new List<string> { "User" };

        _identityServiceMock
            .Setup(s => s.RegisterUserAsync(command.UserName, command.Email, command.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, userId, Array.Empty<string>()));

        _identityServiceMock
            .Setup(s => s.GetUserRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        _tokenServiceMock
            .Setup(s => s.GenerateAccessToken(userId, command.UserName, roles))
            .Returns("fake_jwt_token");

        _identityServiceMock
            .Setup(s => s.GenerateAndSaveRefreshTokenAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("fake_refresh_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.UserName.Should().Be("john_doe");
        result.AccessToken.Should().Be("fake_jwt_token");
        result.RefreshToken.Should().Be("fake_refresh_token");
        result.Roles.Should().Contain("User");
    }

    [Fact]
    public async Task Handle_WhenRegistrationFails_ShouldThrowValidationException()
    {
        // Arrange
        var command = new RegisterCommand("john_doe", "john@example.com", "short");

        _identityServiceMock
            .Setup(s => s.RegisterUserAsync(command.UserName, command.Email, command.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Guid.Empty, new[] { "Password is too short." }));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }
}
