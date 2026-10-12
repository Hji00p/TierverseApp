using TierList.Domain.Common;

namespace TierList.Domain.Entities;

public class User : BaseEntity
{
    public string UserName { get; private set; } = string.Empty;

    public ICollection<TierList> TierLists { get; private set; } = new List<TierList>();
    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
    public ICollection<TierListVote> TierListVotes { get; private set; } = new List<TierListVote>();
    public ICollection<CommentVote> CommentVotes { get; private set; } = new List<CommentVote>();

    private User() { }

    public User(Guid id, string userName)
    {
        Id = id;
        UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateUserName(string userName)
    {
        UserName = userName ?? throw new ArgumentNullException(nameof(userName));
    }
}
