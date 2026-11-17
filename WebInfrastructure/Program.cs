using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebInfrastructure.Data;
using WebInfrastructure.Infrastructure;
using WebInfrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Database: SQL Server when "Database:Provider" is SqlServer (production, with "Database:ConnectionString");
// an in-memory sample database that starts with the sample catalogue when it is InMemory or not set
// (development and tests). Any other value stops the application at start instead of choosing silently.
var provider = builder.Configuration["Database:Provider"] ?? "InMemory";
var useSqlServer = provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase);
if (!useSqlServer && !provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException($"Database:Provider '{provider}' is not supported; use InMemory or SqlServer.");
}

var connectionString = builder.Configuration["Database:ConnectionString"];
if (useSqlServer && string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Database:ConnectionString is required when Database:Provider is SqlServer.");
}

// "Security:RequireHttps": production setting; sends cookies only over HTTPS and redirects HTTP to HTTPS.
var requireHttps = builder.Configuration.GetValue<bool>("Security:RequireHttps");

builder.Services.AddControllersWithViews(options =>
{
    // Required fields are declared explicitly on the models; non-nullable types are not required by themselves.
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    options.Filters.Add<RequestLoggingFilter>();
});

// The shopping cart is identified through the user's session.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = requireHttps ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
    options.IdleTimeout = TimeSpan.FromMinutes(20);
});

builder.Services.AddDbContext<GadgetsOnlineDbContext>(options =>
{
    if (useSqlServer)
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseInMemoryDatabase("GadgetsOnline")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning));
    }
});

builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<IOrderProcessingService, OrderProcessingService>();
builder.Services.AddSingleton<ICartIdProvider, SessionCartProvider>();
builder.Services.AddHostedService<StaleCartCleanupService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GadgetsOnlineDbContext>();
    db.Database.EnsureCreated();
    GadgetsOnlineDbInitializer.Seed(db);
}

if (requireHttps)
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseExceptionHandler("/Home/Error");

// Browser hardening for every response: no MIME sniffing, no framing (the checkout form collects personal data),
// no referrer, and a policy that allows only the site's own scripts, styles and images.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "no-referrer";
    headers["Content-Security-Policy"] =
        "default-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; frame-ancestors 'none'; form-action 'self'";
    await next();
});

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

/// <summary>Entry point type, so tests can start the application.</summary>
public partial class Program
{
}
