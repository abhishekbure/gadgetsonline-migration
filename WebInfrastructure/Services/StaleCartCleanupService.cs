using Microsoft.EntityFrameworkCore;
using WebInfrastructure.Data;

namespace WebInfrastructure.Services;

/// <summary>
/// Removes the cart lines of sessions that are long gone. A visitor without a session cookie gets a new cart id
/// on every request, so without this the Carts table would only ever grow.
/// "Cart:RetentionHours" (default 24) is how long a line is kept; "Cart:CleanupMinutes" (default 30) is how often the
/// purge runs.
/// </summary>
public class StaleCartCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<StaleCartCleanupService> _logger;
    private readonly TimeSpan _retention;
    private readonly TimeSpan _interval;

    public StaleCartCleanupService(IServiceScopeFactory scopes, IConfiguration configuration,
        ILogger<StaleCartCleanupService> logger)
    {
        _scopes = scopes;
        _logger = logger;
        _retention = TimeSpan.FromHours(Math.Max(1, configuration.GetValue("Cart:RetentionHours", 24)));
        _interval = TimeSpan.FromMinutes(Math.Max(1, configuration.GetValue("Cart:CleanupMinutes", 30)));
    }

    /// <summary>Deletes every cart line created before <paramref name="cutoff"/>; returns how many were removed.</summary>
    public static async Task<int> PurgeAsync(GadgetsOnlineDbContext db, DateTime cutoff,
        CancellationToken cancellationToken = default)
    {
        var stale = await db.Carts.Where(c => c.DateCreated < cutoff).ToListAsync(cancellationToken);
        if (stale.Count == 0)
        {
            return 0;
        }

        db.Carts.RemoveRange(stale);
        await db.SaveChangesAsync(cancellationToken);
        return stale.Count;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);
        do
        {
            try
            {
                using var scope = _scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<GadgetsOnlineDbContext>();
                var removed = await PurgeAsync(db, DateTime.UtcNow - _retention, stoppingToken);
                if (removed > 0)
                {
                    _logger.LogInformation("web_infrastructure.cart.purged removed {Removed} stale cart lines", removed);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // A failed purge must never stop the site; the next round tries again.
                _logger.LogWarning(ex, "web_infrastructure.cart.purge_failed");
            }
        }
        while (await WaitAsync(timer, stoppingToken));
    }

    private static async Task<bool> WaitAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
