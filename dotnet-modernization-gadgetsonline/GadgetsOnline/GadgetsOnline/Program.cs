using GadgetsOnline.Models;
using GadgetsOnline.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var useLocalDb = builder.Configuration.GetValue<bool>("DatabaseConfiguration:UseLocalDb");
var connectionString = useLocalDb 
    ? builder.Configuration.GetConnectionString("GadgetsOnlineEntities")
    : "Server=sqlserver;Database=GadgetsOnlineDB;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;";

builder.Services.AddDbContext<GadgetsOnlineEntities>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<Inventory>();
builder.Services.AddScoped<ShoppingCart>();
builder.Services.AddScoped<OrderProcessing>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GadgetsOnlineEntities>();
    db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
