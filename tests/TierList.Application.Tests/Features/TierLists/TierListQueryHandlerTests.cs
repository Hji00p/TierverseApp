using FluentAssertions;
using Moq;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.TierLists.Queries.GetTierListById;
using TierList.Application.Features.TierLists.Queries.GetTierLists;
using TierList.Application.Tests.Common;
using TierList.Domain.Entities;
using TierList.Domain.Enums;
using Xunit;
using TierListEntity = TierList.Domain.Entities.TierList;

namespace TierList.Application.Tests.Features.TierLists;

public class TierListQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    [Fact]
    public async Task GetTierListsQuery_ShouldReturnPaginatedList_AndFilterBySearch()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), "creator");
        _context.Users.Add(user);

        var tl1 = new TierListEntity(user.Id, "Top Action Movies", "Action");
        var tl2 = new TierListEntity(user.Id, "Best Comedies", "Funny");
        var tl3 = new TierListEntity(user.Id, "Top Horror Movies", "Scary");

        _context.TierLists.AddRange(tl1, tl2, tl3);
        await _context.SaveChangesAsync();

        var handler = new GetTierListsQueryHandler(_context);

        // Act
        var resultAll = await handler.Handle(new GetTierListsQuery(null, 1, 10), CancellationToken.None);
        var resultSearch = await handler.Handle(new GetTierListsQuery("Movies", 1, 10), CancellationToken.None);

        // Assert
        resultAll.Items.Should().HaveCount(3);
        resultSearch.Items.Should().HaveCount(2);
        resultSearch.Items.Should().Contain(t => t.Title == "Top Action Movies");
        resultSearch.Items.Should().Contain(t => t.Title == "Top Horror Movies");
        resultSearch.Items.Should().NotContain(t => t.Title == "Best Comedies");
    }

    [Fact]
    public async Task GetTierListByIdQuery_ShouldReturnDetailsWithOrderedTiersAndItemsAndVoteScores()
    {
        // Arrange
        var author = new User(Guid.NewGuid(), "authorUser");
        var voter1 = new User(Guid.NewGuid(), "voter1");
        var voter2 = new User(Guid.NewGuid(), "voter2");
        _context.Users.AddRange(author, voter1, voter2);

        var tierList = new TierListEntity(author.Id, "Anime Tier List", "Ranking anime");
        var tierB = tierList.AddTier("B", "#0000ff", 1);
        var tierS = tierList.AddTier("S", "#ff0000", 0);
        var item2 = tierList.AddItem("https://img/2.png", "Item 2", tierS.Id, 1);
        var item1 = tierList.AddItem("https://img/1.png", "Item 1", tierS.Id, 0);

        _context.TierLists.Add(tierList);

        var vote1 = new TierListVote(voter1.Id, tierList.Id, VoteValue.Like);
        var vote2 = new TierListVote(voter2.Id, tierList.Id, VoteValue.Like);
        _context.TierListVotes.AddRange(vote1, vote2);

        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(voter1.Id);
        var handler = new GetTierListByIdQueryHandler(_context, _currentUserServiceMock.Object);

        // Act
        var result = await handler.Handle(new GetTierListByIdQuery(tierList.Id), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(tierList.Id);
        result.Title.Should().Be("Anime Tier List");
        result.AuthorUserName.Should().Be("authorUser");
        result.Likes.Should().Be(2);
        result.Dislikes.Should().Be(0);
        result.Score.Should().Be(2);
        result.UserVote.Should().Be(1);

        // Tiers should be ordered by Order (S = 0, B = 1)
        result.Tiers[0].Name.Should().Be("S");
        result.Tiers[1].Name.Should().Be("B");

        // Items should be ordered by Position (Item 1 = 0, Item 2 = 1)
        result.Items[0].Caption.Should().Be("Item 1");
        result.Items[1].Caption.Should().Be("Item 2");
    }

    [Fact]
    public async Task GetTierListByIdQuery_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var handler = new GetTierListByIdQueryHandler(_context, _currentUserServiceMock.Object);

        // Act
        var act = () => handler.Handle(new GetTierListByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
