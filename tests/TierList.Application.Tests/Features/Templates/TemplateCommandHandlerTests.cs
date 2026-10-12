using FluentAssertions;
using Moq;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.Templates.Commands.CreateTemplate;
using TierList.Application.Features.Templates.Commands.DeleteTemplate;
using TierList.Application.Features.Templates.Commands.UpdateTemplate;
using TierList.Application.Tests.Common;
using TierList.Domain.Entities;
using TierList.Domain.Enums;
using Xunit;

namespace TierList.Application.Tests.Features.Templates;

public class TemplateCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    [Fact]
    public async Task CreateTemplate_WhenAuthenticated_ShouldCreateTemplateWithTiersAndItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var tiers = new List<CreateTemplateTierModel>
        {
            new("S", "#ff7f7f", 0),
            new("A", "#ffbf7f", 1)
        };
        var items = new List<CreateTemplateItemModel>
        {
            new("https://example.com/ak47.png", "AK-47", 0),
            new("https://example.com/m4a1.png", "M4A1-S", 1)
        };

        var command = new CreateTemplateCommand("CS2 Weapons", "Rank CS2 rifles", TemplateVisibility.Public, tiers, items);
        var handler = new CreateTemplateCommandHandler(_context, _currentUserServiceMock.Object);

        // Act
        var templateId = await handler.Handle(command, CancellationToken.None);

        // Assert
        var created = await _context.Templates.FindAsync(templateId);
        created.Should().NotBeNull();
        created!.Title.Should().Be("CS2 Weapons");
        created.AuthorId.Should().Be(userId);
        created.Visibility.Should().Be(TemplateVisibility.Public);
        created.Tiers.Should().HaveCount(2);
        created.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateTemplate_WhenOwner_ShouldUpdateDetailsAndReplaceTiersAndItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var template = new Template(userId, "Old Title", "Old Desc", TemplateVisibility.Public);
        template.AddTier("S", "#ff0000", 0);
        template.AddItem("https://img.com/1.png", "Item 1", 0);
        _context.Templates.Add(template);
        await _context.SaveChangesAsync();

        var updatedTiers = new List<CreateTemplateTierModel>
        {
            new("God Tier", "#gold", 0),
            new("Mid Tier", "#silver", 1)
        };
        var updatedItems = new List<CreateTemplateItemModel>
        {
            new("https://img.com/2.png", "New Item", 0)
        };

        var command = new UpdateTemplateCommand(template.Id, "New Title", "New Desc", TemplateVisibility.Unlisted, updatedTiers, updatedItems);
        var handler = new UpdateTemplateCommandHandler(_context, _currentUserServiceMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _context.Templates.FindAsync(template.Id);
        updated!.Title.Should().Be("New Title");
        updated.Description.Should().Be("New Desc");
        updated.Visibility.Should().Be(TemplateVisibility.Unlisted);
        updated.Tiers.Should().HaveCount(2);
        updated.Tiers.First().Name.Should().Be("God Tier");
        updated.Items.Should().HaveCount(1);
        updated.Items.First().Caption.Should().Be("New Item");
    }

    [Fact]
    public async Task UpdateTemplate_WhenNotOwner_ShouldThrowForbiddenException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var intruderId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(intruderId);

        var template = new Template(ownerId, "Title", null, TemplateVisibility.Public);
        _context.Templates.Add(template);
        await _context.SaveChangesAsync();

        var command = new UpdateTemplateCommand(template.Id, "Hacked", null, TemplateVisibility.Public, new(), new());
        var handler = new UpdateTemplateCommandHandler(_context, _currentUserServiceMock.Object);

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task DeleteTemplate_WhenOwner_ShouldDeleteTemplate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var template = new Template(userId, "To Delete", null, TemplateVisibility.Public);
        _context.Templates.Add(template);
        await _context.SaveChangesAsync();

        var command = new DeleteTemplateCommand(template.Id);
        var handler = new DeleteTemplateCommandHandler(_context, _currentUserServiceMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deleted = await _context.Templates.FindAsync(template.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTemplate_WhenModerator_ShouldDeleteEvenIfNotOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(moderatorId);
        _currentUserServiceMock.Setup(s => s.IsInRole("Moderator")).Returns(true);

        var template = new Template(ownerId, "Spam Template", null, TemplateVisibility.Public);
        _context.Templates.Add(template);
        await _context.SaveChangesAsync();

        var command = new DeleteTemplateCommand(template.Id);
        var handler = new DeleteTemplateCommandHandler(_context, _currentUserServiceMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deleted = await _context.Templates.FindAsync(template.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTemplate_WhenNotOwnerNorMod_ShouldThrowForbiddenException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var strangerId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(strangerId);

        var template = new Template(ownerId, "Template", null, TemplateVisibility.Public);
        _context.Templates.Add(template);
        await _context.SaveChangesAsync();

        var command = new DeleteTemplateCommand(template.Id);
        var handler = new DeleteTemplateCommandHandler(_context, _currentUserServiceMock.Object);

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
