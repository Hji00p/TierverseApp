using FluentAssertions;
using TierList.Application.Features.Templates.Queries.GetTemplateById;
using TierList.Application.Features.Templates.Queries.GetTemplates;
using TierList.Application.Tests.Common;
using TierList.Domain.Entities;
using TierList.Domain.Enums;
using Xunit;

namespace TierList.Application.Tests.Features.Templates;

public class TemplateQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();

    [Fact]
    public async Task GetTemplatesQuery_ShouldOnlyReturnPublicTemplates_ExcludingUnlisted()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), "creator");
        _context.Users.Add(user);

        var publicTemplate1 = new Template(user.Id, "Public Pokemon Gen 1", "desc", TemplateVisibility.Public);
        var publicTemplate2 = new Template(user.Id, "Public CS2 Weapons", "desc", TemplateVisibility.Public);
        var unlistedTemplate = new Template(user.Id, "Secret Unlisted Weapons", "hidden", TemplateVisibility.Unlisted);

        _context.Templates.AddRange(publicTemplate1, publicTemplate2, unlistedTemplate);
        await _context.SaveChangesAsync();

        var handler = new GetTemplatesQueryHandler(_context);
        var query = new GetTemplatesQuery(null, 1, 20);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(t => t.Id == publicTemplate1.Id);
        result.Items.Should().Contain(t => t.Id == publicTemplate2.Id);
        result.Items.Should().NotContain(t => t.Id == unlistedTemplate.Id);
    }

    [Fact]
    public async Task GetTemplatesQuery_WithSearchTerm_ShouldFilterMatchingTitles()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), "creator");
        _context.Users.Add(user);

        var template1 = new Template(user.Id, "Best Anime 2026", null, TemplateVisibility.Public);
        var template2 = new Template(user.Id, "Fast Food Chains", null, TemplateVisibility.Public);

        _context.Templates.AddRange(template1, template2);
        await _context.SaveChangesAsync();

        var handler = new GetTemplatesQueryHandler(_context);
        var query = new GetTemplatesQuery("Anime", 1, 20);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Best Anime 2026");
    }

    [Fact]
    public async Task GetTemplateByIdQuery_WhenTemplateIsUnlisted_ShouldStillBeAccessibleById()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), "streamer");
        _context.Users.Add(user);

        var unlistedTemplate = new Template(user.Id, "Streamer Rank This", "Private link only", TemplateVisibility.Unlisted);
        unlistedTemplate.AddTier("S", "#ff0000", 0);
        unlistedTemplate.AddItem("https://example.com/item.png", "Item Caption", 0);

        _context.Templates.Add(unlistedTemplate);
        await _context.SaveChangesAsync();

        var handler = new GetTemplateByIdQueryHandler(_context);
        var query = new GetTemplateByIdQuery(unlistedTemplate.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(unlistedTemplate.Id);
        result.Title.Should().Be("Streamer Rank This");
        result.Visibility.Should().Be(TemplateVisibility.Unlisted);
        result.Tiers.Should().HaveCount(1);
        result.Items.Should().HaveCount(1);
    }
}
