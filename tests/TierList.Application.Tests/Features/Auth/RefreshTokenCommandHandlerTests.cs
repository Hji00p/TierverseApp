using FluentAssertions;
using Moq;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.Auth.Commands.RefreshToken;
using Xunit;

namespace TierList.Application.Tests.Features.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _handler = new RefreshTokenCommandHandler(_identityServiceMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenValid_ShouldReturnNewAuthResult()
    {
        // Arrange
        var command = new RefreshTokenCommand("valid_old_token");
        var userId = Guid.NewGuid();
        var roles = new List<string> { "User" };

        _identityServiceMock
            .Setup(s => s.RotateRefreshTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, userId, "john_doe", roles, "new_refresh_token", null));

        _tokenServiceMock
            .Setup(s => s.GenerateAccessToken(userId, "john_doe", roles))
            .Returns("new_access_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.AccessToken.Should().Be("new_access_token");
        result.RefreshToken.Should().Be("new_refresh_token");
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenInvalid_ShouldThrowForbiddenException()
    {
        // Arrange
        var command = new RefreshTokenCommand("expired_or_invalid_token");

        _identityServiceMock
            .Setup(s => s.RotateRefreshTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Guid.Empty, string.Empty, Array.Empty<string>(), string.Empty, "Token expired"));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
