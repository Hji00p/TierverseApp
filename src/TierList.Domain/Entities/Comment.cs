using TierList.Domain.Common;

namespace TierList.Domain.Entities;

public class Comment : BaseEntity
{
    public Guid TierListId { get; private set; }
    public TierList TierList { get; private set; } = null!;
    public Guid AuthorId { get; private set; }
    public User Author { get; private set; } = null!;
    public string Text { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; }
    public Guid? ParentCommentId { get; private set; }
    public Comment? ParentComment { get; private set; }
    public ICollection<Comment> Replies { get; private set; } = new List<Comment>();

    public ICollection<CommentVote> Votes { get; private set; } = new List<CommentVote>();

    private Comment() { }

    public Comment(Guid tierListId, Guid authorId, string text, Guid? parentCommentId = null)
    {
        TierListId = tierListId;
        AuthorId = authorId;
        Text = text ?? throw new ArgumentNullException(nameof(text));
        ParentCommentId = parentCommentId;
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }

    public void UpdateText(string text)
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Cannot update a deleted comment.");
        }
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    public void Delete()
    {
        IsDeleted = true;
    }
}
