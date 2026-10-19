# GadgetsOnline → GMMCode Migration Guide

## What You Have

**GadgetsOnline** is a legacy ASP.NET MVC 4.7.2 e-commerce app:

- 4 Controllers: Home, Store, ShoppingCart, Checkout
- 5 Models: Product, Category, Cart, Order, OrderDetail
- 3 Services: Inventory, ShoppingCart, OrderProcessing
- Entity Framework 6 Code-First with SQL Server
- Razor Views + session-based shopping cart

**Goal:** Modernize to .NET 8 (ASP.NET Core MVC) via the GMMCode platform.

---

## Step 1 — Push the Project to GitHub

GMMCode worker clones the workspace from GitHub. The gadget project must be there first.

```bash
cd "dotnet-modernization-gadgetsonline"
git init
git add .
git commit -m "Initial commit: GadgetsOnline ASP.NET MVC 4.7.2"
git remote add origin <your-github-repo-url>
git push -u origin main
```

Register this repo in the GMMCode platform config so the worker knows where to clone from.

---

## Step 2 — Create the Jira Epic

In your Jira project (e.g. lane `GMMCOD`), create an epic:

| Field | Value |
|---|---|
| **Epic Name** | Modernize GadgetsOnline to .NET 8 |
| **Description** | Migrate ASP.NET MVC 4.7.2 GadgetsOnline app to ASP.NET Core 8 MVC. Retain all features: product browsing, shopping cart, checkout, EF Core data layer. |
| **Status** | `Send to GMM` ← this triggers the worker |

---

## Step 3 — L1 / L2 / L3 Service Tier Runs

Once the epic hits `Send to GMM`, the worker picks it up automatically via `jira_reconciliation_tick` (~every 5.5 min).

| Agent | Decision |
|---|---|
| **L1** | Classify: modernization/migration task |
| **L2** | Scope: full rewrite; risk = breaking changes in EF Core, no `System.Web`, Razor syntax differences |
| **L3** | Creates `SR-XXXXXX` + `CHG-XXXXXX` + `PROP-XXXXXX` |

### L3 must set `scope_included` (Files:) in the PROP

```
Files:
  GadgetsOnline/Controllers/HomeController.cs
  GadgetsOnline/Controllers/StoreController.cs
  GadgetsOnline/Controllers/ShoppingCartController.cs
  GadgetsOnline/Controllers/CheckoutController.cs
  GadgetsOnline/Models/Product.cs
  GadgetsOnline/Models/Category.cs
  GadgetsOnline/Models/Cart.cs
  GadgetsOnline/Models/Order.cs
  GadgetsOnline/Models/OrderDetail.cs
  GadgetsOnline/Models/GadgetsOnlineEntities.cs
  GadgetsOnline/Services/Inventory.cs
  GadgetsOnline/Services/ShoppingCart.cs
  GadgetsOnline/Services/OrderProcessing.cs
  GadgetsOnline/Views/**/*.cshtml
  GadgetsOnline/App_Start/RouteConfig.cs
  GadgetsOnline/App_Start/BundleConfig.cs
  GadgetsOnline.csproj
  Program.cs
  appsettings.json
```

> The `scope_adherence` gate will reject any file written outside this list.

---

## Step 4 — Delivery Pod Executes (9 Roles)

| Role | Work for GadgetsOnline |
|---|---|
| `product_owner` | Confirms: modernize all 4 controllers, keep same routes, no new features |
| `business_analyst` | Documents: cart uses Session → must move to ISession in ASP.NET Core |
| `architect` | New `.csproj` targeting `net8.0`; EF Core 8 replaces EF6; `DbContext` inherits from `Microsoft.EntityFrameworkCore.DbContext` |
| `developer` | Rewrites all `.cs` files; removes `System.Web.*`; adds `Program.cs` + `appsettings.json` |
| `tester` | Writes xUnit tests for Inventory, ShoppingCart, OrderProcessing services |
| `security_engineer` | Checks: no hard-coded connection strings, CSRF tokens on forms, HTTPS enforced |
| `code_reviewer` | Checks: no `using System.Web.Mvc` left, all EF6 APIs replaced with EF Core |
| `devops_engineer` | Verifies `dotnet build` passes; optionally adds Dockerfile |
| `delivery_manager` | Closes engagement, transitions Jira epic to Completed |

---

## Step 5 — 8 Governance Gates (All Mandatory)

| Gate | Pass Condition for This Project |
|---|---|
| `change_authorisation` | PROP approved, SR open |
| `design` | EF Core migration plan reviewed by architect |
| `scope_adherence` | No files written outside the `Files:` list |
| `implementation` | All 4 controllers compile against `net8.0` |
| `qa` | xUnit tests pass, no null reference exceptions in service layer |
| `security` | No plaintext connection strings, HTTPS middleware added |
| `code_review` | Zero remaining `System.Web` references |
| `release_readiness` | `dotnet build` green, runbook written |

> A rejected gate sends the pod back to fix the issue automatically — never manually bypassed.

---

## Step 6 — Git Flow

```
Developer commits → wip/GMMCOD-<n>
         ↓ gates pass
         → feature/gmmcod-modernize-gadgets
         ↓ PR reviewed by another lane
         → main
```

---

## Step 7 — Jira Auto-Closes

When `delivery_manager` closes the engagement:

- Stories: `Backlog → In Progress → Done`
- Epic: `Send to GMM → Completed from GMM`

---

## Key Risks to Flag Before You Start

Flag these 4 items explicitly in the L2 scope doc so the architect gate catches them before the developer starts:

| # | Risk | Detail |
|---|---|---|
| 1 | `System.Web.HttpContext` removed | `ShoppingCart.GetCart(HttpContext)` → must use `IHttpContextAccessor` via DI |
| 2 | `Database.SetInitializer` removed | `GadgetsOnlineDBInitializer` pattern gone; use EF Core migrations instead |
| 3 | `[ChildActionOnly]` removed | `StoreController.CategoryMenu()` needs refactoring to a **ViewComponent** |
| 4 | Session API changed | `GetCart` logic needs `services.AddSession()` + `ISession` in `Program.cs` |
