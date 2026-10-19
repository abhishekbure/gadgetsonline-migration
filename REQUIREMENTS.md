# GadgetsOnline — .NET 4.7.2 → .NET 10 Migration Requirements

## Project Overview

Migrate the **GadgetsOnline** e-commerce web application from **ASP.NET MVC 4.7.2 (.NET Framework)** to **ASP.NET Core MVC (.NET 10)**. The repository contains a partially migrated codebase. This document describes all issues that must be fixed, tested, and closed as Jira tickets to complete the migration.

---

## Repository

```bash
git clone https://github.com/abhishekbure/gadgetsonline-legacy.git
```

> Public repo — no PAT required.

Working project path after clone:
```
dotnet-modernization-gadgetsonline/GadgetsOnline/GadgetsOnline
```

Run with:
```bash
dotnet run --project dotnet-modernization-gadgetsonline/GadgetsOnline/GadgetsOnline --environment Development
```

App runs at: `http://localhost:5000`

---

## Tech Stack

| Layer | Before (.NET Framework) | After (.NET 10) |
|-------|------------------------|-----------------|
| Framework | ASP.NET MVC 4.7.2 (.NET Framework) | ASP.NET Core MVC (.NET 10) |
| Language | C# 7.3 | C# 14 |
| ORM | Entity Framework 6 | EF Core 10 |
| Auth | Forms Authentication | ASP.NET Core Identity (anonymous user for now) |
| Session | `System.Web.SessionState` | ASP.NET Core Distributed Memory Cache |
| Config | `Web.config` | `appsettings.json` |
| DI | None (manual instantiation) | Built-in ASP.NET Core DI |
| Bundling | `BundleConfig.cs` (System.Web.Optimization) | Static files served from `wwwroot/` |
| JSON | `Newtonsoft.Json` (PascalCase default) | `System.Text.Json` (camelCase default) |
| Routing | `RouteConfig.cs` + attribute routing | Conventional + attribute routing in `Program.cs` |
| Error handling | `CustomErrors` in Web.config | `UseExceptionHandler` / `UseStatusCodePages` middleware |
| OpenAPI | N/A | Built-in `Microsoft.AspNetCore.OpenApi` (no Swashbuckle needed) |

---

## .csproj Target Framework

```xml
<TargetFramework>net10.0</TargetFramework>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

> **Critical:** `<Nullable>enable</Nullable>` activates C# nullable reference types. Non-nullable `string` properties not in the HTML form will fail `ModelState.IsValid` unless annotated with `[BindNever]` + `[ValidateNever]`. See ISSUE-1.

---

## .NET 10 vs .NET 4.7.2 — Key Breaking Changes

| Area | .NET Framework 4.7.2 | .NET 10 Core |
|------|----------------------|--------------|
| `HttpContext` | `System.Web.HttpContext` | `Microsoft.AspNetCore.Http.HttpContext` |
| `Session` | `HttpContext.Current.Session` | Injected `IHttpContextAccessor` or `HttpContext.Session` in controllers |
| `TempData` | Auto-persistent across redirects | Requires `TempDataProvider` registered (cookie or session) |
| JSON serialization | Newtonsoft.Json, PascalCase | `System.Text.Json`, camelCase by default |
| Model binding | Non-nullable string = not required | Non-nullable string = implicitly required (ISSUE-1) |
| DI | Not built-in | All services registered in `Program.cs` |
| `Global.asax` | Application lifecycle hooks | `Program.cs` middleware pipeline |
| `Web.config` | All config including connection strings | `appsettings.json` + environment variables |
| `BundleConfig` | Script/CSS bundling | Static files in `wwwroot/` referenced directly |
| EF migrations | `Enable-Migrations` / `Add-Migration` | `EnsureCreated()` or `dotnet ef migrations add` |
| `[Authorize]` | Forms auth ticket | Cookie auth scheme — must configure in `Program.cs` |

---

## Security Constraints (Non-Negotiable)

- **Never** delete databases, drop tables, truncate data, or run destructive migrations
- **Never** expose passwords, connection strings with credentials, or API keys in logs or reports
- Do not disable security checks or remove tests to make things pass
- Keep original and migrated versions separate — never modify the original baseline
- Connection string must use `Trusted_Connection=True` — no plaintext credentials
- Do not add `--no-verify` to git commands or suppress compiler warnings with `#pragma warning disable`

---

## Issue List — All Must Be Implemented

Each issue must become a Jira ticket, be implemented in code, and verified via UI test.

---

### HIGHEST Priority (App-Breaking)

#### ISSUE-1: CheckoutController POST ignores ModelState.IsValid
**File:** `Controllers/CheckoutController.cs`
**Problem:** The `AddressAndPayment` POST action processes the order without checking `ModelState.IsValid`, allowing malformed orders to be created. In .NET 10 with nullable enabled, this is always `false` unless scaffold-hidden properties are annotated.
**Fix:**
- Add `if (!ModelState.IsValid) return View(order);` as the **first** statement.
- Add `[ValidateAntiForgeryToken]` attribute to the POST action.
- Inject `ShoppingCart` into the constructor (needed for ISSUE-6).

**Critical .NET 10 addendum — `Models/Order.cs`:**
Properties `OrderId`, `OrderDate`, `Username`, `Total` are `[ScaffoldColumn(false)]` (hidden from form) but non-nullable. .NET 10 nullable reference types make these implicitly required by the model binder, so `ModelState` always fails because they are never in the form.
**Fix:** Add `[BindNever]` + `[ValidateNever]` to all four properties:
```csharp
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

[ScaffoldColumn(false)]
[BindNever]
[ValidateNever]
public int OrderId { get; set; }

[ScaffoldColumn(false)]
[BindNever]
[ValidateNever]
public System.DateTime OrderDate { get; set; }

[ScaffoldColumn(false)]
[BindNever]
[ValidateNever]
public string Username { get; set; } = null!;

[ScaffoldColumn(false)]
[BindNever]
[ValidateNever]
public decimal Total { get; set; }
```

---

#### ISSUE-2: Missing jQuery script references (404 on load)
**Files:** `Views/Shared/_Layout.cshtml`, `Views/ShoppingCart/Index.cshtml`
**Problem:** Files reference `jquery-1.4.4.min.js` or `jquery-3.5.1.min.js` — neither exists in `wwwroot/Scripts/`. Breaks all jQuery-dependent functionality site-wide.
**Fix:** Verify actual file in `wwwroot/Scripts/` (expected: `jquery-3.6.0.min.js`) and update all references.

---

#### ISSUE-3: ShoppingCart.RemoveFromCart uses Single() — crashes on missing item
**File:** `Services/ShoppingCart.cs`
**Method:** `RemoveFromCart(int id)`
**Problem:** `db.Carts.Single(...)` throws `InvalidOperationException` if the cart item doesn't exist.
**Fix:**
```csharp
var cartItem = db.Carts.SingleOrDefault(cart => cart.CartId == ShoppingCartId && cart.ProductId == id);
if (cartItem == null) return 0;
```

---

#### ISSUE-4: GetProductNameById crashes with null-forgiving operator
**File:** `Services/Inventory.cs`
**Method:** `GetProductNameById(int id)`
**Problem:** `FirstOrDefault()!.Name` causes `NullReferenceException` when the product doesn't exist.
**Fix:**
```csharp
var product = db.Products.FirstOrDefault(p => p.ProductId == id);
return product?.Name ?? "Unknown product";
```

---

#### ISSUE-5: AJAX cart removal — wrong return type and camelCase mismatch
**Files:** `Controllers/ShoppingCartController.cs`, `Views/ShoppingCart/Index.cshtml`
**Problem (Controller):** `RemoveFromCart` returns `RedirectToAction` instead of JSON — AJAX call gets a redirect, not data.
**Problem (View):** JavaScript uses PascalCase (`data.ItemCount`) but `System.Text.Json` in .NET 10 serializes in **camelCase** by default.

**Fix — Controller:**
```csharp
[HttpPost]
public ActionResult RemoveFromCart(int id)
{
    int itemCount = _cart.RemoveFromCart(id);
    var results = new ShoppingCartRemoveViewModel
    {
        Message = System.Net.WebUtility.HtmlEncode(_inventory.GetProductNameById(id)) + " has been removed from your shopping cart.",
        CartTotal = _cart.GetTotal(),
        CartCount = _cart.GetCount(),
        ItemCount = itemCount,
        DeleteId = id
    };
    return Json(results);
}
```

**Fix — View (camelCase property names):**
```javascript
if (data.itemCount == 0) {
    $('#row-' + data.deleteId).fadeOut('slow');
} else {
    $('#item-count-' + data.deleteId).text(data.itemCount);
}
$('#cart-total').text(data.cartTotal);
$('#update-message').text(data.message);
$('#cart-status').text('Cart (' + data.cartCount + ')');
```

---

#### ISSUE-6: Checkout processes order when cart is empty
**File:** `Controllers/CheckoutController.cs`
**Problem:** POST action doesn't check for empty cart — zero-total ghost orders get inserted.
**Fix:** After `ModelState.IsValid` check:
```csharp
if (_cart.GetCartItems().Count == 0)
{
    TempData["Message"] = "Your cart is empty. Please add items before checking out.";
    return RedirectToAction("Index", "ShoppingCart");
}
```

---

### HIGH Priority (Data / Runtime Correctness)

#### ISSUE-7: CS8618 nullable warnings on all model classes
**Files:** `Models/Product.cs`, `Models/Category.cs`, `Models/Order.cs`, `Models/Cart.cs`, `Models/OrderDetail.cs`, `ViewModel/ShoppingCartViewModel.cs`, `ViewModel/ShoppingCartRemoveViewModel.cs`
**Problem:** Non-nullable reference type properties lack initializers — CS8618 warnings throughout, potential runtime nulls.
**Fix:** Add `= null!` to non-nullable string/object properties. Add `= new List<T>()` to collection navigation properties. Build must show **0 warnings**.

---

#### ISSUE-8: Missing HasPrecision for decimal columns
**File:** `Models/GadgetsOnlineEntities.cs`
**Method:** `OnModelCreating`
**Problem:** `decimal` columns have no precision configured — EF Core 10 will warn and default to `decimal(18,2)`.
**Fix:**
```csharp
modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(10, 2);
modelBuilder.Entity<Order>().Property(o => o.Total).HasPrecision(10, 2);
modelBuilder.Entity<OrderDetail>().Property(od => od.UnitPrice).HasPrecision(10, 2);
```

---

#### ISSUE-9: GetBestSellers missing OrderBy before Take()
**File:** `Services/Inventory.cs`
**Method:** `GetBestSellers(int count)`
**Problem:** `.Take(count)` without `.OrderBy(...)` → non-deterministic results + EF Core 10 warning.
**Fix:** Add `.OrderBy(p => p.ProductId)` before `.Take(count)`.

---

#### ISSUE-10: No empty cart UI state
**File:** `Views/ShoppingCart/Index.cshtml`
**Problem:** Empty cart renders blank table — no feedback for user.
**Fix:**
```razor
@if (Model.CartItems.Count > 0)
{
    // existing table
}
else
{
    <p>Your cart is empty. <a href="/Store">Browse our store</a></p>
}
```

---

#### ISSUE-11: Browse view missing category name heading
**Files:** `Controllers/StoreController.cs`, `Views/Store/Browse.cshtml`
**Problem:** Category heading commented out; `ViewBag.Category` never set.
**Fix — Controller:** `ViewBag.Category = category;` in `Browse` action.
**Fix — View:**
```razor
<h3><em>@ViewBag.Category</em> Products</h3>
```

---

### MEDIUM Priority (Polish / Standards)

#### ISSUE-12: Add Bootstrap CSS/JS and standard meta tags to layout
**File:** `Views/Shared/_Layout.cshtml`
**Problem:** Missing `charset`, `viewport`, Bootstrap CSS, Bootstrap JS bundle.
**Fix — `<head>`:**
```html
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<link rel="stylesheet" href="~/Content/bootstrap.min.css" />
```
**Fix — before `</body>`:**
```html
<script src="~/Scripts/bootstrap.bundle.min.js"></script>
```
Only add what is not already present. Verify files exist in `wwwroot/` before referencing.

---

#### ISSUE-13: Remove legacy .NET Framework files
**Problem:** Leftover stubs from the .NET Framework project — not used by .NET 10, cause confusion.
Files to delete **only if stub/unused**:
- `App_Start/BundleConfig.cs`, `FilterConfig.cs`, `RouteConfig.cs`
- `Global.asax`, `Global.asax.cs`
- `Web.config`, `Web.Debug.config`, `Web.Release.config`
- `packages.config`
- `Models/GadgetsOnlineDBInitializer.cs` (if stub only)
- `Migrations/Configuration.cs` (if stub only)
- Old EF6 migration files (`Migrations/202111...`)

**Do NOT delete** `Program.cs`, `appsettings.json`, or any file actively referenced by the .NET 10 project.

---

#### ISSUE-14: Checkout/Complete.cshtml — demo disclaimer and external link
**File:** `Views/Checkout/Complete.cshtml`
**Problem:** Contains "demo" disclaimer and Amazon link — inappropriate for production.
**Fix:**
```
Thank you for your order! Your order number is: [OrderId]
We have received your order and will process it shortly.
You will receive a confirmation email once your order has been dispatched.
[Return to home page] | [Continue shopping]
```

---

#### ISSUE-15: Price on Store/Details.cshtml not currency-formatted
**File:** `Views/Store/Details.cshtml`
**Problem:** `$@String.Format("{0:F}", Model.Price)` — raw format, no currency symbol.
**Fix:** `@Model.Price.ToString("C")`

---

#### ISSUE-16: Order.cs email validation uses narrow regex
**File:** `Models/Order.cs`
**Problem:** `[RegularExpression]` rejects valid TLDs longer than 4 characters (`.online`, `.store`, `.technology`).
**Fix:** Replace with `[EmailAddress(ErrorMessage = "Email is not valid.")]`. Keep `[DataType(DataType.EmailAddress)]`.

---

### NEW — .NET 10 Specific Issues

#### ISSUE-17: Update target framework to net10.0
**File:** `GadgetsOnline.csproj`
**Problem:** `.csproj` may target `net8.0` or `net6.0` — must target `net10.0`.
**Fix:**
```xml
<TargetFramework>net10.0</TargetFramework>
```
Then run `dotnet restore` to pull updated packages. Update all `Microsoft.*` NuGet packages to their `10.x` versions.

---

#### ISSUE-18: Upgrade EF Core packages to version 10
**File:** `GadgetsOnline.csproj`
**Problem:** EF Core packages must match the runtime version.
**Fix — update in `.csproj`:**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.*" />
```
Run `dotnet restore` after updating.

---

#### ISSUE-19: Replace Newtonsoft.Json with System.Text.Json (if present)
**Files:** Any file using `JsonConvert.SerializeObject` / `JsonConvert.DeserializeObject`
**Problem:** .NET 10 apps should use `System.Text.Json`. If `Newtonsoft.Json` is still referenced for serialization, it must be replaced.
**Fix:** Use `JsonSerializer.Serialize` / `JsonSerializer.Deserialize` from `System.Text.Json`. Remove the `Newtonsoft.Json` NuGet reference.
> **Note:** If removing Newtonsoft.Json, ensure all AJAX-consuming JavaScript uses camelCase property names (see ISSUE-5).

---

#### ISSUE-20: Program.cs — verify middleware pipeline order for .NET 10
**File:** `Program.cs`
**Problem:** Middleware order matters. `UseRouting()` → `UseAuthentication()` → `UseAuthorization()` → `MapControllerRoute()` must be in the correct sequence. .NET 10 will warn at startup for incorrect order.
**Fix — minimum correct pipeline:**
```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // only if auth is configured
app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
```

---

#### ISSUE-21: Session configuration — verify IDistributedCache registration
**File:** `Program.cs`
**Problem:** `AddSession()` requires `AddDistributedMemoryCache()` to be called first. Missing registration causes runtime error on session access.
**Fix — `Program.cs` services section:**
```csharp
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

---

#### ISSUE-22: Antiforgery token validation on all POST actions
**Files:** All controllers with `[HttpPost]` actions
**Problem:** .NET 10 ASP.NET Core enforces antiforgery by default in some configurations. All POST form submissions must include the token and controllers must have `[ValidateAntiForgeryToken]`.
**Fix:** Ensure every `[HttpPost]` action that processes a form has `[ValidateAntiForgeryToken]`. Ensure `@Html.AntiForgeryToken()` (or `@Html.BeginForm()` which auto-includes it) is present in every form view.

---

## Previously Fixed (Do Not Re-Implement)

Verify these are still present in the codebase before declaring the sprint done:

| Fix | File | What to verify |
|-----|------|----------------|
| Shopping cart "Pizza" → "Product" column header | `Views/ShoppingCart/Index.cshtml` | `<th>Product</th>` |
| Cart item price `.ToString("C")` | `Views/ShoppingCart/Index.cshtml` | `@item.Product.Price.ToString("C")` |
| Cart total `.ToString("C")` | `Views/ShoppingCart/Index.cshtml` | `@Model.CartTotal.ToString("C")` |
| Checkout promo copy updated | `Views/Checkout/AddressAndPayment.cshtml` | `Use promo code GADGET10` |
| 404 error page wired up | `Views/Home/Error.cshtml`, `Controllers/HomeController.cs`, `Program.cs` | Navigate to `/nonexistent` → see custom 404 page |

---

## Build Requirement

After all fixes are applied:
```bash
dotnet build
```
Must exit with **0 errors and 0 warnings**.

Then verify the target framework:
```bash
dotnet --version
```
Must be `10.x.x` or higher.

---

## UI Test Suite — 8 Required Flows

After build succeeds, start the server and verify each flow:

| # | Flow | Steps | Expected Result |
|---|------|-------|-----------------|
| 1 | Home page | Navigate to `/` | Best sellers grid shown, nav links visible |
| 2 | Store index | Navigate to `/Store` | Category list shown |
| 3 | Browse category | Click a category (e.g. Laptops) | `<Category> Products` heading shown, product list populated |
| 4 | Product details | Click a product | Name, description, price (currency format), "Add to cart" link |
| 5 | Add to cart + Cart view | Click "Add to cart", go to `/ShoppingCart` | Product shown with currency price, total shown, "Remove from cart" links present |
| 6 | Checkout form | Navigate to `/Checkout/AddressAndPayment` with items in cart | Form renders with all fields, "GADGET10" promo text shown |
| 7 | Place order | Fill all form fields, submit | Redirects to `/Checkout/Complete/{id}` with order number, professional message |
| 8 | 404 error page | Navigate to `/nonexistent-page` | Custom error page with "404" heading and navigation links |

**Bonus tests:**
- **Empty cart guard:** With empty cart, POST checkout → redirected to `/ShoppingCart`
- **AJAX remove:** Cart page, click AJAX remove → row fades out, message shown, total updates (no page reload)
- **ModelState validation:** Submit checkout with a required field blank → stays on form, no order created

---

## Jira Workflow

1. **GMMPlan** reads this document → creates one Jira ticket per issue (ISSUE-1 through ISSUE-22) in project **GDE** under the migration Epic
2. **GMMCode** reads each ticket → implements the fix → runs `dotnet build` → marks ticket Done
3. After all tickets Done → restart server → run all 8 UI flows → confirm pass
4. Any new UI blocker → create a new ticket → fix → retest

---

## Notes for GMMCode

- Read every file before editing it
- Do not introduce new NuGet packages beyond what is needed for .NET 10 — work with what is in the `.csproj`
- Do not delete any file actively referenced by the .NET 10 project
- Preserve all existing working functionality
- **camelCase JSON (ISSUE-5 + ISSUE-19):** `System.Text.Json` in .NET 10 serializes camelCase by default. All JavaScript reading JSON from the server must use camelCase property names (`data.itemCount` not `data.ItemCount`).
- **`[BindNever]` + `[ValidateNever]` (ISSUE-1):** Critical for nullable enabled projects — without it, `ModelState.IsValid` always returns `false` for scaffold-hidden non-nullable properties.
- **EF Core 10 `EnsureCreated()`:** Do not switch to migrations unless explicitly asked. `EnsureCreated()` is the current strategy.
- **Connection strings:** Must use `Trusted_Connection=True` — never commit plaintext credentials.
- **`Program.cs` middleware order (ISSUE-20):** Wrong order causes silent auth failures or session not working — follow the exact sequence listed above.
