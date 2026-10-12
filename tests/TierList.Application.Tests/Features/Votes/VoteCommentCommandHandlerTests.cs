using FluentAssertions;
using Moq;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.Votes.Commands.VoteComment;
using TierList.Application.Tests.Common;
using TierList.Domain.Entities;
using Xunit;

namespace TierList.Application.Tests.Features.Votes;

public class VoteCommentCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly VoteCommentCommandHandler _handler;

    public VoteCommentCommandHandlerTests()
    {
        _handler = new VoteCommentCommandHandler(_context, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenLike_ShouldRecordCommentVote()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var comment = new Comment(Guid.NewGuid(), Guid.NewGuid(), "Great list!");
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var command = new VoteCommentCommand(comment.Id, 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.LikesCount.Should().Be(1);
        result.DislikesCount.Should().Be(0);
        result.UserVote.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenCommentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());

        var command = new VoteCommentCommand(Guid.NewGuid(), 1);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
