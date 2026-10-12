using FluentAssertions;
using Moq;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Common.Models;
using TierList.Application.Features.Admin.DTOs;
using TierList.Application.Features.Admin.Queries.GetUsers;
using Xunit;

namespace TierList.Application.Tests.Features.Admin;

public class GetUsersQueryHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly GetUsersQueryHandler _handler;

    public GetUsersQueryHandlerTests()
    {
        _handler = new GetUsersQueryHandler(_identityServiceMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAdmin_ShouldReturnUsersList()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.IsInRole("Admin")).Returns(true);

        var usersList = new List<UserRoleDto>
        {
            new(Guid.NewGuid(), "alice", "alice@example.com", new List<string> { "User" }, DateTime.UtcNow),
            new(Guid.NewGuid(), "bob", "bob@example.com", new List<string> { "Moderator" }, DateTime.UtcNow)
        };
        var paginated = new PaginatedList<UserRoleDto>(usersList, 2, 1, 20);

        _identityServiceMock
            .Setup(s => s.GetUsersAsync(null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginated);

        var query = new GetUsersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNotAdmin_ShouldThrowForbiddenException()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.IsInRole("Admin")).Returns(false);

        var query = new GetUsersQuery();

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
