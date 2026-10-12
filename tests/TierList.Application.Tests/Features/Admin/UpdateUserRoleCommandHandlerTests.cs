using FluentAssertions;
using Moq;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.Admin.Commands.UpdateUserRole;
using Xunit;

namespace TierList.Application.Tests.Features.Admin;

public class UpdateUserRoleCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly UpdateUserRoleCommandHandler _handler;

    public UpdateUserRoleCommandHandlerTests()
    {
        _handler = new UpdateUserRoleCommandHandler(_identityServiceMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAdmin_ShouldUpdateRoleSuccessfully()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.IsInRole("Admin")).Returns(true);

        _identityServiceMock
            .Setup(s => s.UpdateUserRoleAsync(targetUserId, "Moderator", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var command = new UpdateUserRoleCommand(targetUserId, "Moderator");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _identityServiceMock.Verify(s => s.UpdateUserRoleAsync(targetUserId, "Moderator", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotAdmin_ShouldThrowForbiddenException()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.IsInRole("Admin")).Returns(false);

        var command = new UpdateUserRoleCommand(Guid.NewGuid(), "Admin");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.IsInRole("Admin")).Returns(true);

        _identityServiceMock
            .Setup(s => s.UpdateUserRoleAsync(targetUserId, "Moderator", It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, new[] { "User not found." }));

        var command = new UpdateUserRoleCommand(targetUserId, "Moderator");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
