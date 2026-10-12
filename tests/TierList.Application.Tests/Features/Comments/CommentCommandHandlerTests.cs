using FluentAssertions;
using Moq;
using TierList.Application.Common.Exceptions;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.Comments.Commands.CreateComment;
using TierList.Application.Features.Comments.Commands.DeleteComment;
using TierList.Application.Tests.Common;
using TierList.Domain.Entities;
using TierListEntity = TierList.Domain.Entities.TierList;
using Xunit;

namespace TierList.Application.Tests.Features.Comments;

public class CommentCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    [Fact]
    public async Task CreateComment_WhenAuthenticated_ShouldAddComment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var tierList = new TierListEntity(Guid.NewGuid(), "Tier List");
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        var handler = new CreateCommentCommandHandler(_context, _currentUserServiceMock.Object);
        var command = new CreateCommentCommand(tierList.Id, "This ranking is spot on!");

        // Act
        var commentId = await handler.Handle(command, CancellationToken.None);

        // Assert
        var comment = await _context.Comments.FindAsync(commentId);
        comment.Should().NotBeNull();
        comment!.Text.Should().Be("This ranking is spot on!");
        comment.AuthorId.Should().Be(userId);
        comment.IsDeleted.Should().BeFalse();
        comment.ParentCommentId.Should().BeNull();
    }

    [Fact]
    public async Task CreateComment_WhenReply_ShouldSetParentCommentId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var tierList = new TierListEntity(Guid.NewGuid(), "Tier List");
        _context.TierLists.Add(tierList);
        await _context.SaveChangesAsync();

        var parentComment = new Comment(tierList.Id, userId, "Original comment");
        _context.Comments.Add(parentComment);
        await _context.SaveChangesAsync();

        var handler = new CreateCommentCommandHandler(_context, _currentUserServiceMock.Object);
        var replyCommand = new CreateCommentCommand(tierList.Id, "I agree with this!", parentComment.Id);

        // Act
        var replyId = await handler.Handle(replyCommand, CancellationToken.None);

        // Assert
        var reply = await _context.Comments.FindAsync(replyId);
        reply.Should().NotBeNull();
        reply!.Text.Should().Be("I agree with this!");
        reply.ParentCommentId.Should().Be(parentComment.Id);
    }

    [Fact]
    public async Task DeleteComment_WhenOwner_ShouldSoftDelete()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var comment = new Comment(Guid.NewGuid(), userId, "I will delete this");
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var handler = new DeleteCommentCommandHandler(_context, _currentUserServiceMock.Object);
        var command = new DeleteCommentCommand(comment.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _context.Comments.FindAsync(comment.Id);
        updated!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteComment_WhenModerator_ShouldSoftDeleteEvenIfNotOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(moderatorId);
        _currentUserServiceMock.Setup(s => s.IsInRole("Moderator")).Returns(true);

        var comment = new Comment(Guid.NewGuid(), ownerId, "Spam content");
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var handler = new DeleteCommentCommandHandler(_context, _currentUserServiceMock.Object);
        var command = new DeleteCommentCommand(comment.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _context.Comments.FindAsync(comment.Id);
        updated!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteComment_WhenNotOwnerNorModerator_ShouldThrowForbiddenException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var intruderId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(intruderId);

        var comment = new Comment(Guid.NewGuid(), ownerId, "Nice comment");
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var handler = new DeleteCommentCommandHandler(_context, _currentUserServiceMock.Object);
        var command = new DeleteCommentCommand(comment.Id);

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task DeleteComment_ShouldAlsoSoftDeleteAllReplies()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var tierListId = Guid.NewGuid();
        var parentComment = new Comment(tierListId, userId, "Parent comment");
        _context.Comments.Add(parentComment);
        await _context.SaveChangesAsync();

        var reply1 = new Comment(tierListId, Guid.NewGuid(), "Reply 1", parentComment.Id);
        var reply2 = new Comment(tierListId, Guid.NewGuid(), "Reply 2", parentComment.Id);
        _context.Comments.AddRange(reply1, reply2);
        await _context.SaveChangesAsync();

        var handler = new DeleteCommentCommandHandler(_context, _currentUserServiceMock.Object);
        var command = new DeleteCommentCommand(parentComment.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedParent = await _context.Comments.FindAsync(parentComment.Id);
        var updatedReply1 = await _context.Comments.FindAsync(reply1.Id);
        var updatedReply2 = await _context.Comments.FindAsync(reply2.Id);

        updatedParent!.IsDeleted.Should().BeTrue();
        updatedReply1!.IsDeleted.Should().BeTrue();
        updatedReply2!.IsDeleted.Should().BeTrue();
    }
}
