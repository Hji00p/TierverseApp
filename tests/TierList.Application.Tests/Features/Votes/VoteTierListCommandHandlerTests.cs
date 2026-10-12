using FluentAssertions;
using Moq;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.Votes.Commands.VoteTierList;
using TierList.Application.Tests.Common;
using TierListEntity = TierList.Domain.Entities.TierList;
using Xunit;

namespace TierList.Application.Tests.Features.Votes;

public class VoteTierListCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly VoteTierListCommandHandler _handler;

    public VoteTierListCommandHandlerTests()
    {
        _handler = new VoteTierListCommandHandler(_context, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenLike_ShouldRecordLikeVote()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var tierList = new TierListEntity(Guid.NewGuid(), "Awesome Tier List");
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        var command = new VoteTierListCommand(tierList.Id, 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.LikesCount.Should().Be(1);
        result.DislikesCount.Should().Be(0);
        result.UserVote.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenChangingFromLikeToDislike_ShouldUpdateTally()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var tierList = new TierListEntity(Guid.NewGuid(), "Awesome Tier List");
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        // Vote Like first
        await _handler.Handle(new VoteTierListCommand(tierList.Id, 1), CancellationToken.None);

        // Act: change to Dislike
        var result = await _handler.Handle(new VoteTierListCommand(tierList.Id, -1), CancellationToken.None);

        // Assert
        result.LikesCount.Should().Be(0);
        result.DislikesCount.Should().Be(1);
        result.UserVote.Should().Be(-1);
    }

    [Fact]
    public async Task Handle_WhenVoteZero_ShouldRemoveVote()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var tierList = new TierListEntity(Guid.NewGuid(), "Awesome Tier List");
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        // Vote Like
        await _handler.Handle(new VoteTierListCommand(tierList.Id, 1), CancellationToken.None);

        // Act: remove vote
        var result = await _handler.Handle(new VoteTierListCommand(tierList.Id, 0), CancellationToken.None);

        // Assert
        result.LikesCount.Should().Be(0);
        result.DislikesCount.Should().Be(0);
        result.UserVote.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenTierListNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());

        var command = new VoteTierListCommand(Guid.NewGuid(), 1);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
