using FluentAssertions;
using Moq;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.Comments.Queries.GetComments;
using TierList.Application.Tests.Common;
using TierList.Domain.Entities;
using TierList.Domain.Enums;
using Xunit;
using TierListEntity = TierList.Domain.Entities.TierList;

namespace TierList.Application.Tests.Features.Comments;

public class CommentQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    [Fact]
    public async Task GetComments_ShouldReturnRootCommentsWithNestedRepliesAndVoteCounts()
    {
        // Arrange
        var author = new User(Guid.NewGuid(), "author");
        var replier = new User(Guid.NewGuid(), "replier");
        var voter = new User(Guid.NewGuid(), "voter");
        _context.Users.AddRange(author, replier, voter);

        var tierList = new TierListEntity(author.Id, "Tier List Title");
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        var rootComment1 = new Comment(tierList.Id, author.Id, "Root Comment 1");
        var rootComment2 = new Comment(tierList.Id, author.Id, "Root Comment 2");
        _context.Comments.AddRange(rootComment1, rootComment2);
        await _context.SaveChangesAsync();

        var reply1 = new Comment(tierList.Id, replier.Id, "Reply to Comment 1", rootComment1.Id);
        _context.Comments.Add(reply1);

        var vote = new CommentVote(voter.Id, rootComment1.Id, VoteValue.Like);
        _context.CommentVotes.Add(vote);

        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(voter.Id);
        var handler = new GetCommentsQueryHandler(_context, _currentUserServiceMock.Object);

        // Act
        var result = await handler.Handle(new GetCommentsQuery(tierList.Id, 1, 10), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);

        var c1 = result.Items.First(c => c.Id == rootComment1.Id);
        c1.Text.Should().Be("Root Comment 1");
        c1.AuthorUserName.Should().Be("author");
        c1.LikesCount.Should().Be(1);
        c1.DislikesCount.Should().Be(0);
        c1.UserVote.Should().Be(1);
        c1.Replies.Should().HaveCount(1);
        c1.Replies![0].Text.Should().Be("Reply to Comment 1");
        c1.Replies[0].AuthorUserName.Should().Be("replier");

        var c2 = result.Items.First(c => c.Id == rootComment2.Id);
        c2.Replies.Should().BeEmpty();
    }

    [Fact]
    public async Task GetComments_ShouldExcludeSoftDeletedComments()
    {
        // Arrange
        var author = new User(Guid.NewGuid(), "author");
        _context.Users.Add(author);

        var tierList = new TierListEntity(author.Id, "Tier List Title");
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        var activeComment = new Comment(tierList.Id, author.Id, "Active Comment");
        var deletedComment = new Comment(tierList.Id, author.Id, "Deleted Comment");
        deletedComment.Delete();

        _context.Comments.AddRange(activeComment, deletedComment);
        await _context.SaveChangesAsync();

        var handler = new GetCommentsQueryHandler(_context, _currentUserServiceMock.Object);

        // Act
        var result = await handler.Handle(new GetCommentsQuery(tierList.Id, 1, 10), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Id.Should().Be(activeComment.Id);
    }
}
