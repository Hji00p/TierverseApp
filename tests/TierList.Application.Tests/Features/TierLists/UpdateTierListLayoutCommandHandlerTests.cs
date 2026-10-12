using FluentAssertions;
using Moq;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.TierLists.Commands.UpdateTierListLayout;
using TierList.Application.Features.TierLists.DTOs;
using TierList.Application.Tests.Common;
using TierListEntity = TierList.Domain.Entities.TierList;
using Xunit;

namespace TierList.Application.Tests.Features.TierLists;

public class UpdateTierListLayoutCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly UpdateTierListLayoutCommandHandler _handler;

    public UpdateTierListLayoutCommandHandlerTests()
    {
        _handler = new UpdateTierListLayoutCommandHandler(_context, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidLayout_ShouldUpdateTiersAndPositions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var tierList = new TierListEntity(userId, "Favorite Games");
        var tierS = tierList.AddTier("S Tier", "#FF7F7F", 0);
        var tierA = tierList.AddTier("A Tier", "#FFBF7F", 1);
        var item1 = tierList.AddItem("https://example.com/zelda.png", "Zelda");
        var item2 = tierList.AddItem("https://example.com/mario.png", "Mario");
        var item3 = tierList.AddItem("https://example.com/metroid.png", "Metroid");

        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        var layout = new SaveLayoutDto(
            Tiers: new List<TierLayoutItemDto>
            {
                new(tierS.Id, new List<Guid> { item2.Id }),
                new(tierA.Id, new List<Guid> { item1.Id })
            },
            UnplacedItemIds: new List<Guid> { item3.Id });

        var command = new UpdateTierListLayoutCommand(tierList.Id, layout);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedItem1 = await _context.TierItems.FindAsync(item1.Id);
        var updatedItem2 = await _context.TierItems.FindAsync(item2.Id);
        var updatedItem3 = await _context.TierItems.FindAsync(item3.Id);

        updatedItem2!.TierId.Should().Be(tierS.Id);
        updatedItem2.Position.Should().Be(0);

        updatedItem1!.TierId.Should().Be(tierA.Id);
        updatedItem1.Position.Should().Be(0);

        updatedItem3!.TierId.Should().BeNull();
        updatedItem3.Position.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotOwner_ShouldThrowForbiddenException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(anotherUserId);

        var tierList = new TierListEntity(ownerId, "Movies");
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        var command = new UpdateTierListLayoutCommand(
            tierList.Id,
            new SaveLayoutDto(new List<TierLayoutItemDto>(), new List<Guid>()));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenTierListNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());

        var command = new UpdateTierListLayoutCommand(
            Guid.NewGuid(),
            new SaveLayoutDto(new List<TierLayoutItemDto>(), new List<Guid>()));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenTierDoesNotBelongToTierList_ShouldThrowValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var tierList = new TierListEntity(userId, "Anime");
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        var foreignTierId = Guid.NewGuid();
        var layout = new SaveLayoutDto(
            Tiers: new List<TierLayoutItemDto> { new(foreignTierId, new List<Guid>()) },
            UnplacedItemIds: new List<Guid>());

        var command = new UpdateTierListLayoutCommand(tierList.Id, layout);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().ContainKey("Tiers");
        ex.Which.Errors["Tiers"].First().Should().Contain("does not belong to this tier list");
    }

    [Fact]
    public async Task Handle_WhenItemDoesNotBelongToTierList_ShouldThrowValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var tierList = new TierListEntity(userId, "Anime");
        var tier = tierList.AddTier("S", "#FF0000", 0);
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        var foreignItemId = Guid.NewGuid();
        var layout = new SaveLayoutDto(
            Tiers: new List<TierLayoutItemDto> { new(tier.Id, new List<Guid> { foreignItemId }) },
            UnplacedItemIds: new List<Guid>());

        var command = new UpdateTierListLayoutCommand(tierList.Id, layout);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().ContainKey("ItemIds");
        ex.Which.Errors["ItemIds"].First().Should().Contain("does not belong to this tier list");
    }

    [Fact]
    public async Task Handle_WhenDuplicateItemsInLayout_ShouldThrowValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var tierList = new TierListEntity(userId, "Anime");
        var tierS = tierList.AddTier("S", "#FF0000", 0);
        var tierA = tierList.AddTier("A", "#00FF00", 1);
        var item = tierList.AddItem("https://example.com/hero.png", "Hero");
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        // Put the same item in both S and A tier
        var layout = new SaveLayoutDto(
            Tiers: new List<TierLayoutItemDto>
            {
                new(tierS.Id, new List<Guid> { item.Id }),
                new(tierA.Id, new List<Guid> { item.Id })
            },
            UnplacedItemIds: new List<Guid>());

        var command = new UpdateTierListLayoutCommand(tierList.Id, layout);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().ContainKey("ItemIds");
        ex.Which.Errors["ItemIds"].First().Should().Contain("Duplicate item ID");
    }
}
