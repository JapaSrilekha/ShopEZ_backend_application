# ShopEZ — E-Commerce REST API

A backend REST API for an e-commerce platform built with **ASP.NET Core 8**, **Entity Framework Core**, **SQL Server**, and **JWT authentication**.

---

## Tech Stack

| Package | Purpose |
|---|---|
| EF Core 8 + SQL Server | ORM & database |
| `JwtBearer` 8.0 | JWT middleware |
| `BCrypt.Net-Next` 4.1.0 | Password hashing |
| `Swashbuckle.AspNetCore` 6.5.0 | Swagger UI |

**Requirements:** .NET SDK 8.0+, SQL Server 2019+ or LocalDB, Visual Studio 2022

---

## Getting Started

1. Open `ShopEZ.sln` in Visual Studio 2022.
2. Update the connection string in `ECommerce.API/appsettings.json`:
   ```json
   "DefaultConnection": "Server=YOUR_SERVER\\SQLEXPRESS;Database=ShopEZ;Trusted_Connection=True;TrustServerCertificate=True"
   ```
   For LocalDB: `"Server=(localdb)\\mssqllocaldb;Database=ShopEZ;Trusted_Connection=True;"`
3. Press **F5**. EF Core auto-migrates, seeds the database, and opens Swagger UI at `https://localhost:60466`.

---

## Configuration

```json
"JwtSettings": {
  "SecretKey": "ShopEZ_SuperSecret_JWT_Key_2024_Min32Chars!",
  "Issuer": "ShopEZ.API",
  "Audience": "ShopEZ.Client",
  "ExpiresInHours": "24"
},
"Cors": {
  "AllowedOrigins": ["http://localhost:3000", "http://localhost:4200"]
}
```

> Replace `SecretKey` with a strong random value before deploying.

---

## Seed Data

| Name | Email | Password | Role |
|---|---|---|---|
| Super Admin | admin@shopez.com | `Admin@123` | Admin |
| Japa Srilekha | japasrilekha@example.com | `Sri@123` | Customer |
| Japa Likhithanjali | japalikhithanjali@example.com | `Likhithanjali@123` | Customer |

5 products are seeded (Wireless Headphones ₹2999, Mechanical Keyboard ₹4499, USB-C Hub ₹1299, Gaming Mouse ₹1899, Monitor Stand ₹899) plus 1 sample order for Srilekha.

---

## API Reference

**Base URL:** `https://localhost:60466`  
**Response format:** `{ "success": bool, "message": string, "data": object | null }`

### Auth — Public
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register` | Register — returns JWT. Password needs uppercase, lowercase, digit & special char. |
| POST | `/api/auth/login` | Login — returns JWT. |

### Products
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/products` | Public | List all products (sorted by name) |
| GET | `/api/products/{id}` | Public | Get single product |
| POST | `/api/products` | Admin | Create product |
| PUT | `/api/products/{id}` | Admin | Update product (all fields required) |
| DELETE | `/api/products/{id}` | Admin | Delete product — blocked if it has orders |

### Orders
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/orders` | Any user | Place order; stock validated & deducted server-side. `UserId` from JWT. |
| GET | `/api/orders` | Admin | All orders |
| GET | `/api/orders/{id}` | Admin or own | Single order (Customer can only see own) |
| GET | `/api/orders/user/{userId}` | Admin | All orders for a user |

---

## Authentication

Tokens are **HMAC-SHA256 JWT**, valid 24 hours. Claims: `sub` (UserId), `email`, `name`, `role`, `jti`.

In Postman: call `/api/auth/login` → copy `data.token` → set as **Bearer Token** on protected requests.

---

## Error Handling

`ExceptionHandlingMiddleware` handles all exceptions globally — no try/catch in controllers.

| Status | Cause |
|---|---|
| 400 | Validation failure (missing field, bad format, quantity = 0) |
| 401 | Missing/expired token or wrong credentials |
| 403 | Insufficient role |
| 404 | Product or order not found |
| 409 | Duplicate email on register, or insufficient stock |
| 500 | Unexpected server error |

---

## Architecture

Clean 3-layer: **Controllers → Services → Repositories → SQL Server**

- Controllers return `ApiResponse<T>` wrappers with no try/catch — exceptions bubble to middleware
- `UserId` is always read from JWT claims, never from the request body
- Passwords never appear in any response DTO
- Product delete is **restricted** by FK if any `OrderItem` references it (preserves order history)

---

*ShopEZ — ASP.NET Core 8 · EF Core · SQL Server · JWT*
