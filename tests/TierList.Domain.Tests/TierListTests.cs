using TierListEntity = TierList.Domain.Entities.TierList;
using Xunit;

namespace TierList.Domain.Tests;

public class TierListTests
{
    [Fact]
    public void NewTierList_ShouldInitialize_WithDefaultValues()
    {
        var authorId = Guid.NewGuid();
        var tierList = new TierListEntity(authorId, "Favorite Movies", "Ranked from best to worst");

        Assert.NotEqual(Guid.Empty, tierList.Id);
        Assert.Equal(authorId, tierList.AuthorId);
        Assert.Equal("Favorite Movies", tierList.Title);
        Assert.Equal("Ranked from best to worst", tierList.Description);
        Assert.Empty(tierList.Tiers);
        Assert.Empty(tierList.Items);
    }

    [Fact]
    public void AddTier_ShouldAddTier_ToCollection()
    {
        var tierList = new TierListEntity(Guid.NewGuid(), "My List");

        var tier = tierList.AddTier("S Tier", "#FF7F7F", 0);

        Assert.Single(tierList.Tiers);
        Assert.Equal("S Tier", tier.Name);
        Assert.Equal("#FF7F7F", tier.Color);
        Assert.Equal(0, tier.Order);
        Assert.Equal(tierList.Id, tier.TierListId);
    }

    [Fact]
    public void AddItem_AndPlaceInTier_ShouldUpdatePositionAndTierId()
    {
        var tierList = new TierListEntity(Guid.NewGuid(), "My List");
        var tier = tierList.AddTier("S Tier", "#FF7F7F", 0);

        var item = tierList.AddItem("https://example.com/img.png", "Item 1");
        Assert.Null(item.TierId);
        Assert.Equal(0, item.Position);

        item.PlaceInTier(tier.Id, 1);
        Assert.Equal(tier.Id, item.TierId);
        Assert.Equal(1, item.Position);
    }
}
