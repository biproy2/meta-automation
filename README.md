# 🛒 Ecommerce Automation — Multi-Tenant Web API
### WhatsApp + Messenger + Pathao + Shopify | .NET 8 | Multi-Tenant Architecture

---

## 📐 Multi-Tenant Architecture

```
One API → Many Clients
┌──────────────────────────────────────────────┐
│  meta-automation.onrender.com                │
│                                              │
│  Tenant A: gadgetry                          │
│  → WhatsApp: +8801704240177                  │
│  → Webhook: /api/webhook/gadgetry/whatsapp   │
│                                              │
│  Tenant B: usa-store                         │
│  → WhatsApp: +1234567890                     │
│  → Shopify: usastore.myshopify.com           │
│  → Webhook: /api/webhook/usa-store/whatsapp  │
└──────────────────────────────────────────────┘
```

---

## 🚀 Quick Start (VS Code)

### Step 1 — Install prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [VS Code](https://code.visualstudio.com/)
- Install recommended extensions (VS Code will prompt you)

### Step 2 — Open project
```bash
code .
```

### Step 3 — Update database connection
Edit `Ecommerce.API/appsettings.json`:
```json
"DefaultConnection": "Data Source=YOUR_SERVER;Initial Catalog=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASS;TrustServerCertificate=True;"
```

### Step 4 — Install EF tools & run migrations
```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add MultiTenantInit --project Ecommerce.Persistence --startup-project Ecommerce.API
dotnet ef database update --project Ecommerce.Persistence --startup-project Ecommerce.API
```

### Step 5 — Run the API
Press **F5** or run:
```bash
dotnet run --project Ecommerce.API
```
Open: `http://localhost:5000/swagger`

---

## 📡 API Endpoints

### Tenant (Client) Management
| Method | URL | Description |
|--------|-----|-------------|
| POST | `/api/tenant/register` | Register new client account |
| POST | `/api/tenant/login` | Login and get JWT token |
| GET | `/api/tenant/settings` | Get webhook URLs and settings |
| PUT | `/api/tenant/settings` | Update WhatsApp/Shopify/Pathao credentials |

### Webhooks (per tenant slug)
| Method | URL | Description |
|--------|-----|-------------|
| GET | `/api/webhook/{slug}/whatsapp` | Meta verification |
| POST | `/api/webhook/{slug}/whatsapp` | Receive WhatsApp messages |
| GET | `/api/webhook/{slug}/messenger` | Meta verification |
| POST | `/api/webhook/{slug}/messenger` | Receive Messenger messages |
| POST | `/api/webhook/{slug}/shopify/order` | Receive Shopify orders |

### Orders (requires JWT)
| Method | URL | Description |
|--------|-----|-------------|
| GET | `/api/order` | List orders |
| POST | `/api/order` | Create order |
| PATCH | `/api/order/{id}/status` | Update status |
| POST | `/api/order/{id}/dispatch` | Dispatch via Pathao/Shopify |

### Leads (requires JWT)
| Method | URL | Description |
|--------|-----|-------------|
| GET | `/api/lead` | List leads |
| POST | `/api/lead/{id}/convert` | Convert lead to order |

---

## 🔑 How Client Uses the API

### 1. Register
```bash
POST /api/tenant/register
{
  "businessName": "My Store",
  "ownerName": "John",
  "ownerEmail": "john@store.com",
  "password": "Password123",
  "confirmPassword": "Password123"
}
```
→ Returns JWT token + slug e.g. `my-store`

### 2. Save credentials
```bash
PUT /api/tenant/settings
Authorization: Bearer {token}
{
  "whatsAppPhoneNumberId": "...",
  "whatsAppAccessToken": "...",
  "shopifyStoreUrl": "mystore.myshopify.com",
  "shopifyAccessToken": "shpat_xxx",
  "deliveryProvider": "ShopifyShipping"
}
```

### 3. Get webhook URLs
```bash
GET /api/tenant/settings
```
Returns:
```json
{
  "whatsAppWebhookUrl": "https://meta-automation.onrender.com/api/webhook/my-store/whatsapp",
  "shopifyWebhookUrl": "https://meta-automation.onrender.com/api/webhook/my-store/shopify/order"
}
```

### 4. Paste URLs into Meta/Shopify → Done! ✅

---

## 🗄️ Database Tables

| Table | Description |
|-------|-------------|
| Tenants | One row per client |
| TenantSettings | WhatsApp/Shopify/Pathao credentials per client |
| Orders | All orders with TenantId |
| Leads | All leads with TenantId |
| Deliveries | Pathao consignments |

