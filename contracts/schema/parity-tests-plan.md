# Parity Tests Plan for web_infrastructure Migration

## Overview
This document defines the parity testing strategy for migrating the web_infrastructure module from .NET Framework 4.8 to ASP.NET Core on .NET 10.

## Legacy Endpoints Identified

The legacy web_infrastructure module (`.legacy-source/GadgetsOnline/GadgetsOnline/Controllers/`) exposes the following endpoints:

### StoreController (4 endpoints)
- `GET /Store/Index` - Store homepage
- `GET /Store/CategoryMenu` - Category menu (child action)
- `GET /Store/Browse?category={category}` - Browse by category
- `GET /Store/Details?id={id}` - Product details

### HomeController (3 endpoints)
- `GET /Home/Index` - Homepage with best sellers
- `GET /Home/About` - About page
- `GET /Home/Contact` - Contact page

### ShoppingCartController (3 endpoints)
- `GET /ShoppingCart/Index` - Shopping cart view
- `POST /ShoppingCart/AddToCart?id={id}` - Add item to cart
- `POST /ShoppingCart/RemoveFromCart?id={id}` - Remove item from cart

### CheckoutController (4 endpoints)
- `GET /Checkout/Index` - Checkout entry
- `GET /Checkout/AddressAndPayment` - Address/payment form
- `POST /Checkout/AddressAndPayment` - Process order
- `GET /Checkout/Complete?id={id}` - Order completion

**Total: 14 endpoints**

## Sample Input Strategy (100 samples)

### Category 1: Browse Operations (30 samples)
- Valid category names: "Headphones", "Laptop", "Mobile", "Camera"
- Invalid category names: "", "NonExistent", special characters
- Product IDs: 1-50 range with gaps

### Category 2: Cart Operations (40 samples)
- Session scenarios: new session, existing session with items
- Add operations: valid product IDs, duplicate adds, invalid IDs
- Remove operations: existing items, non-existing items, quantity > 1
- Edge cases: empty cart, maximum items

### Category 3: Checkout Operations (30 samples)
- Valid addresses with various fields
- Invalid/missing required fields
- Complete flow scenarios

## Data Sampling Approach

1. **Baseline Extraction**: Run 100 requests against legacy endpoints
2. **Response Capture**: Record HTML output, ViewBag values, and Model data
3. **Comparison Logic**: Semantic HTML comparison (structure, not whitespace)
4. **Tolerance Rules**:
   - Session identifiers may differ (use regex matching)
   - Timestamps formatted within same pattern
   - Anti-forgery tokens excluded

## Test Project Structure

```
tests/GadgetsOnline.Parity.Tests/
├── Fixtures/
│   ├── LegacyApplicationFixture.cs     # Legacy app host
│   └── NewApplicationFixture.cs        # New ASP.NET Core app host
├── Data/
│   └── SampleInputs.json               # 100 sample requests
├── Comparers/
│   └── HtmlResponseComparer.cs         # Semantic HTML comparison
├── ParityTests.cs                      # Main test suite
└── ParityTests.csproj                  # .NET 10 test project
```

## Execution Model

```
Test Execution:
  foreach sample in SampleInputs:
    legacy_response = LegacyHost.Execute(sample)
    new_response = NewHost.Execute(sample)
    assert SemanticEquals(legacy_response, new_response)
```

## Pass Criteria

- GOM-12-AC1: All 100 samples produce semantically equivalent responses
- GOM-12-AC2: Both legacy and new implementations run in same test session
- GOM-12-AC3: CI gate blocks GOM-13 merge until parity tests pass
