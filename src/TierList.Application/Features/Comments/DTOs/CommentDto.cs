namespace TierList.Application.Features.Comments.DTOs;

public record CommentDto(
    Guid Id,
    Guid TierListId,
    Guid AuthorId,
    string AuthorUserName,
    string Text,
    DateTime CreatedAt,
    bool IsDeleted,
    int LikesCount,
    int DislikesCount,
    int? UserVote,
    Guid? ParentCommentId = null,
    IReadOnlyList<CommentDto>? Replies = null);
