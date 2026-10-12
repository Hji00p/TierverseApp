using FluentAssertions;
using Moq;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.TierLists.Commands.CreateTierList;
using TierList.Application.Features.TierLists.Commands.DeleteTierList;
using TierList.Application.Features.TierLists.Commands.UpdateTierList;
using TierList.Application.Features.TierLists.DTOs;
using TierList.Application.Tests.Common;
using TierListEntity = TierList.Domain.Entities.TierList;
using Xunit;

namespace TierList.Application.Tests.Features.TierLists;

public class TierListCrudCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    [Fact]
    public async Task Create_WhenAuthenticated_ShouldCreateTierListWithTiers()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var handler = new CreateTierListCommandHandler(_context, _currentUserServiceMock.Object);
        var command = new CreateTierListCommand(
            "Top Frameworks",
            "Ranking .NET, React, etc.",
            new List<CreateTierDto> { new("S", "#FF0000", 0), new("A", "#00FF00", 1) },
            new List<CreateTierItemDto> { new("https://example.com/net.png", ".NET") });

        // Act
        var resultId = await handler.Handle(command, CancellationToken.None);

        // Assert
        var created = await _context.TierLists.FindAsync(resultId);
        created.Should().NotBeNull();
        created!.Title.Should().Be("Top Frameworks");
        created.AuthorId.Should().Be(userId);
    }

    [Fact]
    public async Task Update_WhenOwner_ShouldUpdateTitleAndDescription()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var tierList = new TierListEntity(userId, "Original Title", "Original Desc");
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        var handler = new UpdateTierListCommandHandler(_context, _currentUserServiceMock.Object);
        var command = new UpdateTierListCommand(tierList.Id, "Updated Title", "Updated Desc");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _context.TierLists.FindAsync(tierList.Id);
        updated!.Title.Should().Be("Updated Title");
        updated.Description.Should().Be("Updated Desc");
    }

    [Fact]
    public async Task Update_WhenNotOwner_ShouldThrowForbiddenException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(anotherUserId);

        var tierList = new TierListEntity(ownerId, "Original Title");
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        var handler = new UpdateTierListCommandHandler(_context, _currentUserServiceMock.Object);
        var command = new UpdateTierListCommand(tierList.Id, "Hacked Title", null);

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Delete_WhenModerator_ShouldDeleteEvenIfNotOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(moderatorId);
        _currentUserServiceMock.Setup(s => s.IsInRole("Moderator")).Returns(true);

        var tierList = new TierListEntity(ownerId, "Bad Content");
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        var handler = new DeleteTierListCommandHandler(_context, _currentUserServiceMock.Object);
        var command = new DeleteTierListCommand(tierList.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deleted = await _context.TierLists.FindAsync(tierList.Id);
        deleted.Should().BeNull();
    }
}
