using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebInfrastructure.Data;

namespace WebInfrastructure.Tests;

/// <summary>A seeded in-memory database of its own; every context it hands out sees the same data.</summary>
internal sealed class TestDatabase : IDisposable
{
    private readonly DbContextOptions<GadgetsOnlineDbContext> _options;

    public TestDatabase(bool seed = true)
    {
        _options = new DbContextOptionsBuilder<GadgetsOnlineDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        using var context = NewContext();
        context.Database.EnsureCreated();
        if (seed)
        {
            GadgetsOnlineDbInitializer.Seed(context);
        }
    }

    public GadgetsOnlineDbContext NewContext() => new(_options);

    public void Dispose()
    {
        using var context = NewContext();
        context.Database.EnsureDeleted();
    }
}

/// <summary>A session that lives in a dictionary, for tests of code that keeps state in the session.</summary>
internal sealed class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _store = new();

    public string Id { get; } = Guid.NewGuid().ToString();

    public bool IsAvailable => true;

    public IEnumerable<string> Keys => _store.Keys;

    public void Clear() => _store.Clear();

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Remove(string key) => _store.Remove(key);

    public void Set(string key, byte[] value) => _store[key] = value;

    public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value) => _store.TryGetValue(key, out value);
}

internal static class TestHttp
{
    /// <summary>A request context with its own session, as the controllers see it during a request.</summary>
    public static DefaultHttpContext NewContext() => new() { Session = new TestSession() };

    public static T WithContext<T>(this T controller, HttpContext context) where T : Controller
    {
        controller.ControllerContext = new ControllerContext { HttpContext = context };
        return controller;
    }
}
