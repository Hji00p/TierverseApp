using TierList.Domain.Enums;

namespace TierList.Domain.Entities;

public class CommentVote
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public Guid CommentId { get; private set; }
    public Comment Comment { get; private set; } = null!;
    public VoteValue Value { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private CommentVote() { }

    public CommentVote(Guid userId, Guid commentId, VoteValue value)
    {
        UserId = userId;
        CommentId = commentId;
        Value = value;
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangeVote(VoteValue value)
    {
        Value = value;
    }
}
