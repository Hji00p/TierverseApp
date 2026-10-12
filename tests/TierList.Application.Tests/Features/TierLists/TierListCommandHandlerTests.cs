using FluentAssertions;
using Moq;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.TierLists.Commands.CreateTierList;
using TierList.Application.Features.TierLists.Commands.DeleteTierList;
using TierList.Application.Features.TierLists.Commands.UpdateTierList;
using TierList.Application.Features.TierLists.DTOs;
using TierList.Application.Tests.Common;
using TierList.Domain.Entities;
using TierList.Domain.Enums;
using Xunit;
using TierListEntity = TierList.Domain.Entities.TierList;

namespace TierList.Application.Tests.Features.TierLists;

public class TierListCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    [Fact]
    public async Task CreateTierList_WhenFromScratch_ShouldCreateDefaultTiersAndSave()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var command = new CreateTierListCommand("My Custom Tier List", "Description", null);
        var handler = new CreateTierListCommandHandler(_context, _currentUserServiceMock.Object);

        // Act
        var tierListId = await handler.Handle(command, CancellationToken.None);

        // Assert
        var created = await _context.TierLists.FindAsync(tierListId);
        created.Should().NotBeNull();
        created!.Title.Should().Be("My Custom Tier List");
        created.AuthorId.Should().Be(userId);
        created.TemplateId.Should().BeNull();
        created.Tiers.Should().HaveCount(5); // S, A, B, C, D
        created.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateTierList_WhenFromTemplate_ShouldCopyTiersAndItemsIndependentOfTemplate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var template = new Template(Guid.NewGuid(), "Template Title", "Template Desc", TemplateVisibility.Public);
        var tTier1 = template.AddTier("S", "#ff0000", 0);
        var tTier2 = template.AddTier("A", "#00ff00", 1);
        var tItem1 = template.AddItem("https://img.com/1.png", "Item 1", 0);
        var tItem2 = template.AddItem("https://img.com/2.png", "Item 2", 1);
        _context.Templates.Add(template);
        await _context.SaveChangesAsync();

        var layout = new SaveLayoutDto(
            new List<TierLayoutItemDto>
            {
                new(tTier1.Id, new List<Guid> { tItem1.Id })
            },
            new List<Guid> { tItem2.Id }
        );

        var command = new CreateTierListCommand(
            "My Ranked Tier List",
            "From Template",
            template.Id,
            null,
            null,
            layout
        );

        var handler = new CreateTierListCommandHandler(_context, _currentUserServiceMock.Object);

        // Act
        var tierListId = await handler.Handle(command, CancellationToken.None);

        // Assert
        var created = await _context.TierLists.FindAsync(tierListId);
        created.Should().NotBeNull();
        created!.TemplateId.Should().Be(template.Id);
        created.Tiers.Should().HaveCount(2);
        created.Items.Should().HaveCount(2);

        // Verify items and tiers are copied with new IDs (not template's IDs)
        var copiedTierIds = created.Tiers.Select(t => t.Id).ToList();
        copiedTierIds.Should().NotContain(tTier1.Id);
        copiedTierIds.Should().NotContain(tTier2.Id);

        var placedItem = created.Items.First(i => i.Caption == "Item 1");
        var copiedTier1 = created.Tiers.First(t => t.Name == "S");
        placedItem.TierId.Should().Be(copiedTier1.Id);
        placedItem.Position.Should().Be(0);

        var unplacedItem = created.Items.First(i => i.Caption == "Item 2");
        unplacedItem.TierId.Should().BeNull();
        unplacedItem.Position.Should().Be(0);

        // Modifying the template should not affect the created tier list
        var newTemplateTier = template.AddTier("B", "#0000ff", 2);
        _context.TemplateTiers.Add(newTemplateTier);
        await _context.SaveChangesAsync();

        var reloaded = await _context.TierLists.FindAsync(tierListId);
        reloaded!.Tiers.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateTierList_WhenTemplateNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());
        var command = new CreateTierListCommand("Title", null, Guid.NewGuid());
        var handler = new CreateTierListCommandHandler(_context, _currentUserServiceMock.Object);

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateTierList_WhenUnauthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);
        var command = new CreateTierListCommand("Title", null);
        var handler = new CreateTierListCommandHandler(_context, _currentUserServiceMock.Object);

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateTierList_WhenOwner_ShouldUpdateTitleAndDescription()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var tierList = new TierListEntity(userId, "Old Title", "Old Desc");
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        var command = new UpdateTierListCommand(tierList.Id, "New Title", "New Desc");
        var handler = new UpdateTierListCommandHandler(_context, _currentUserServiceMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _context.TierLists.FindAsync(tierList.Id);
        updated!.Title.Should().Be("New Title");
        updated.Description.Should().Be("New Desc");
    }

    [Fact]
    public async Task UpdateTierList_WhenNotOwner_ShouldThrowForbiddenException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(otherUserId);

        var tierList = new TierListEntity(ownerId, "Title", "Desc");
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        var command = new UpdateTierListCommand(tierList.Id, "Hacked Title", null);
        var handler = new UpdateTierListCommandHandler(_context, _currentUserServiceMock.Object);

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task DeleteTierList_WhenOwner_ShouldDeleteTierList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var tierList = new TierListEntity(userId, "To Delete", null);
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        var command = new DeleteTierListCommand(tierList.Id);
        var handler = new DeleteTierListCommandHandler(_context, _currentUserServiceMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deleted = await _context.TierLists.FindAsync(tierList.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTierList_WhenModerator_ShouldDeleteEvenIfNotOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(moderatorId);
        _currentUserServiceMock.Setup(s => s.IsInRole("Moderator")).Returns(true);

        var tierList = new TierListEntity(ownerId, "Spam TierList", null);
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        var command = new DeleteTierListCommand(tierList.Id);
        var handler = new DeleteTierListCommandHandler(_context, _currentUserServiceMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deleted = await _context.TierLists.FindAsync(tierList.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTierList_WhenNotOwnerNorMod_ShouldThrowForbiddenException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var strangerId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(strangerId);

        var tierList = new TierListEntity(ownerId, "Title", null);
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        var command = new DeleteTierListCommand(tierList.Id);
        var handler = new DeleteTierListCommandHandler(_context, _currentUserServiceMock.Object);

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
