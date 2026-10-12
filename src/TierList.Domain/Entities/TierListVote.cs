using TierList.Domain.Enums;

namespace TierList.Domain.Entities;

public class TierListVote
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public Guid TierListId { get; private set; }
    public TierList TierList { get; private set; } = null!;
    public VoteValue Value { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private TierListVote() { }

    public TierListVote(Guid userId, Guid tierListId, VoteValue value)
    {
        UserId = userId;
        TierListId = tierListId;
        Value = value;
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangeVote(VoteValue value)
    {
        Value = value;
    }
}
