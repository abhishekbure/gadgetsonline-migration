using WebInfrastructure.Models;
using WebInfrastructure.Services;

namespace WebInfrastructure.Tests;

/// <summary>Tests of the purge of cart lines whose session is long gone.</summary>
public class StaleCartCleanupTests
{
    private static Cart Line(string cartId, int productId, DateTime created) => new()
    {
        CartId = cartId,
        ProductId = productId,
        Count = 1,
        DateCreated = created,
    };

    [Fact]
    public async Task OnlyLinesOlderThanTheCutoffAreRemoved()
    {
        using var database = new TestDatabase();
        var now = DateTime.UtcNow;
        await using (var db = database.NewContext())
        {
            db.Carts.AddRange(
                Line("old-1", 1, now.AddDays(-3)),
                Line("old-1", 2, now.AddDays(-3)),
                Line("old-2", 1, now.AddHours(-30)),
                Line("fresh", 1, now.AddMinutes(-5)));
            await db.SaveChangesAsync();
        }

        int removed;
        await using (var db = database.NewContext())
        {
            removed = await StaleCartCleanupService.PurgeAsync(db, now.AddHours(-24));
        }

        Assert.Equal(3, removed);
        await using var check = database.NewContext();
        var left = check.Carts.ToList();
        Assert.Single(left);
        Assert.Equal("fresh", left[0].CartId);
    }

    [Fact]
    public async Task NothingIsRemovedWhenNoLineIsStale()
    {
        using var database = new TestDatabase();
        await using (var db = database.NewContext())
        {
            db.Carts.Add(Line("fresh", 1, DateTime.UtcNow));
            await db.SaveChangesAsync();
        }

        await using var again = database.NewContext();
        Assert.Equal(0, await StaleCartCleanupService.PurgeAsync(again, DateTime.UtcNow.AddHours(-24)));
        Assert.Single(again.Carts.ToList());
    }
}
