using FluentAssertions;
using Moq;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.Auth.Commands.Login;
using Xunit;

namespace TierList.Application.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_identityServiceMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCredentialsValid_ShouldReturnAuthResultDto()
    {
        // Arrange
        var command = new LoginCommand("john_doe", "Password123!");
        var userId = Guid.NewGuid();
        var roles = new List<string> { "User" };

        _identityServiceMock
            .Setup(s => s.ValidateUserCredentialsAsync(command.UserName, command.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, userId, "john_doe", roles, Array.Empty<string>()));

        _tokenServiceMock
            .Setup(s => s.GenerateAccessToken(userId, "john_doe", roles))
            .Returns("fake_jwt_token");

        _identityServiceMock
            .Setup(s => s.GenerateAndSaveRefreshTokenAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("fake_refresh_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.AccessToken.Should().Be("fake_jwt_token");
        result.RefreshToken.Should().Be("fake_refresh_token");
    }

    [Fact]
    public async Task Handle_WhenCredentialsInvalid_ShouldThrowValidationException()
    {
        // Arrange
        var command = new LoginCommand("john_doe", "WrongPassword");

        _identityServiceMock
            .Setup(s => s.ValidateUserCredentialsAsync(command.UserName, command.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Guid.Empty, string.Empty, Array.Empty<string>(), new[] { "Invalid credentials." }));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }
}
