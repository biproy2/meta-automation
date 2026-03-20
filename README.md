# 🛒 Ecommerce Automation — Web API
### WhatsApp + Facebook Messenger + Pathao Courier | .NET 8 Clean Architecture

---

## 📐 Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│  Ecommerce.API          ← LAYER 1: HTTP Controllers + Webhooks  │
│  OrderController        WhatsAppWebhookController               │
│  LeadController         MessengerWebhookController              │
├─────────────────────────────────────────────────────────────────┤
│  Ecommerce.Application  ← LAYER 2: Business Logic               │
│  OrderService           LeadService                             │
│  DTOs + Validators      Interfaces (contracts)                  │
├────────────────────────────┬────────────────────────────────────┤
│  Ecommerce.Infrastructure  │  Ecommerce.Persistence             │
│  ← LAYER 3a: External APIs │  ← LAYER 3b: Database              │
│  WhatsAppService           │  ApplicationDbContext              │
│  MessengerService          │  OrderRepository                   │
│  PathaoService             │  LeadRepository                    │
├────────────────────────────┴────────────────────────────────────┤
│  Ecommerce.Domain       ← LAYER 4: Core (no dependencies)       │
│  Order, Lead, User, Delivery entities                           │
│  Enums: OrderStatus, LeadStatus, MessageChannel                 │
│  Interfaces: IWhatsAppService, IMessengerService, IPathaoService│
└─────────────────────────────────────────────────────────────────┘
```

**Rule:** Dependencies only point INWARD. API → Application → Domain. Never reverse.

---

## 📁 Project Structure

```
Ecommerce.Automation/
├── Ecommerce.Automation.sln
├── Ecommerce.API/
│   ├── Controllers/
│   │   ├── OrderController.cs              POST/GET /api/orders
│   │   ├── LeadController.cs               POST/GET /api/leads
│   │   ├── WhatsAppWebhookController.cs    GET+POST /api/whatsappwebhook
│   │   └── MessengerWebhookController.cs   GET+POST /api/messengerwebhook
│   ├── Middlewares/
│   │   └── ExceptionMiddleware.cs          Global error → clean JSON
│   ├── Extensions/
│   │   └── ServiceExtensions.cs            DI registrations
│   ├── Program.cs
│   ├── appsettings.json                    ← Fill in your API keys here
│   └── appsettings.Development.json
│
├── Ecommerce.Application/
│   ├── Services/
│   │   ├── OrderService.cs                 Create/Update/Dispatch orders
│   │   └── LeadService.cs                  Capture/Convert leads
│   ├── DTOs/
│   │   ├── CreateOrderDto.cs + Validator
│   │   ├── OrderResponseDto.cs
│   │   ├── LeadDto.cs + Validator
│   │   ├── WhatsAppWebhookDto.cs
│   │   └── MessengerWebhookDto.cs
│   ├── Interfaces/
│   │   ├── IOrderService.cs
│   │   ├── ILeadService.cs
│   │   ├── IOrderRepository.cs
│   │   └── ILeadRepository.cs
│   └── Common/
│       ├── Exceptions/                     NotFoundException, ValidationException
│       └── Models/                         ApiResponse<T>, PagedResult<T>
│
├── Ecommerce.Domain/
│   ├── Entities/
│   │   ├── BaseEntity.cs                   Id, CreatedAt, UpdatedAt, IsDeleted
│   │   ├── User.cs
│   │   ├── Lead.cs
│   │   ├── Order.cs
│   │   └── Delivery.cs
│   ├── Enums/
│   │   ├── OrderStatus.cs                  Pending → Confirmed → Dispatched → Delivered
│   │   ├── LeadStatus.cs
│   │   ├── DeliveryStatus.cs
│   │   └── MessageChannel.cs               WhatsApp | Messenger | Direct
│   └── Interfaces/
│       ├── IWhatsAppService.cs
│       ├── IMessengerService.cs
│       └── IPathaoService.cs
│
├── Ecommerce.Infrastructure/
│   ├── Services/
│   │   ├── WhatsAppService.cs              Calls graph.facebook.com/v19.0
│   │   ├── MessengerService.cs             Calls graph.facebook.com/v19.0/me/messages
│   │   └── PathaoService.cs                Calls hermes.pathao.com with OAuth2
│   └── Settings/
│       ├── WhatsAppSettings.cs
│       ├── MessengerSettings.cs
│       └── PathaoSettings.cs
│
├── Ecommerce.Persistence/
│   ├── DbContext/
│   │   └── ApplicationDbContext.cs         EF Core entry point
│   ├── Repositories/
│   │   ├── OrderRepository.cs              EF queries + soft delete + order number gen
│   │   └── LeadRepository.cs
│   └── Configurations/
│       ├── OrderConfiguration.cs           Fluent API: columns, indexes, relations
│       ├── LeadConfiguration.cs
│       ├── UserConfiguration.cs
│       └── DeliveryConfiguration.cs
│
└── Ecommerce.Tests/
    ├── ApplicationTests/
    │   ├── OrderServiceTests.cs
    │   └── LeadServiceTests.cs
    └── InfrastructureTests/
        └── PathaoServiceTests.cs
```

---

## 🚀 STEP-BY-STEP SETUP

### ✅ STEP 1 — Install Prerequisites

1. **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
2. **[Visual Studio 2022](https://visualstudio.microsoft.com/)** — select "ASP.NET and web development"
3. **SQL Server Express** or **LocalDB** (included with Visual Studio)
4. **[ngrok](https://ngrok.com)** — needed to expose localhost for Meta webhooks

---

### ✅ STEP 2 — Open Solution

Double-click `Ecommerce.Automation.sln` in Visual Studio.
Wait for NuGet packages to restore automatically.

---

### ✅ STEP 3 — Get Your API Credentials

#### WhatsApp Business Cloud API:
1. Go to **[Meta Developer Portal](https://developers.facebook.com)**
2. Create App → Business → Add WhatsApp product
3. Copy **Phone Number ID** and **Temporary/Permanent Access Token**
4. Set up a **System User** for a permanent token

#### Facebook Messenger:
1. Same Meta Developer Portal → Create/select your App
2. Add **Messenger** product
3. Go to your Facebook Page → Link to app
4. Generate **Page Access Token**
5. Copy **App Secret** from App Settings → Basic

#### Pathao Courier:
1. Register at **[pathao.com/merchant](https://pathao.com/merchant)**
2. Go to **API Integration** section
3. Get: Client ID, Client Secret, your email/password, Store ID
4. Use `https://sandbox.pathao.com` for testing first

---

### ✅ STEP 4 — Fill in appsettings.json

Open `Ecommerce.API/appsettings.json` and replace all `YOUR_*` values:

```json
"WhatsApp": {
  "PhoneNumberId": "1234567890123456",
  "AccessToken": "EAAxxxxxxxxxxxxx",
  "WebhookVerifyToken": "choose_any_secret_string"
},
"Messenger": {
  "PageAccessToken": "EAAxxxxxxxxxxxxx",
  "AppSecret": "your_app_secret_here",
  "WebhookVerifyToken": "choose_any_secret_string"
},
"Pathao": {
  "ClientId": "your-client-id",
  "ClientSecret": "your-client-secret",
  "Username": "you@email.com",
  "Password": "your_pathao_password",
  "StoreId": "your-store-id"
}
```

---

### ✅ STEP 5 — Create the Database

Open **Package Manager Console** (Tools → NuGet → Package Manager Console):

```powershell
# Create migration (reads your entities → generates SQL)
Add-Migration InitialCreate -Project Ecommerce.Persistence -StartupProject Ecommerce.API

# Apply migration (creates all tables in SQL Server)
Update-Database -Project Ecommerce.Persistence -StartupProject Ecommerce.API
```

Tables created: **Users, Leads, Orders, Deliveries**

---

### ✅ STEP 6 — Run the API

1. Right-click `Ecommerce.API` → **Set as Startup Project**
2. Press **F5**
3. Swagger opens at: `https://localhost:{PORT}/swagger`

---

### ✅ STEP 7 — Set Up Webhooks (for WhatsApp + Messenger)

Webhooks require a public HTTPS URL. Use ngrok in development:

```bash
# Install ngrok, then run:
ngrok http https://localhost:7001
# Copy the https URL e.g. https://abc123.ngrok.io
```

**Register WhatsApp Webhook:**
1. Meta Developer Portal → Your App → WhatsApp → Configuration
2. Callback URL: `https://abc123.ngrok.io/api/whatsappwebhook`
3. Verify Token: `my_secret_verify_token_123` (same as in appsettings.json)
4. Subscribe to: **messages** event

**Register Messenger Webhook:**
1. Meta Developer Portal → Your App → Messenger → Webhooks
2. Callback URL: `https://abc123.ngrok.io/api/messengerwebhook`
3. Verify Token: `my_secret_verify_token_123`
4. Subscribe to: **messages, messaging_postbacks** events

---

### ✅ STEP 8 — Test the Flow

**Test order creation:**
```bash
POST https://localhost:{PORT}/api/orders
{
  "customerName": "Rahim Uddin",
  "customerPhone": "+8801712345678",
  "deliveryAddress": "House 5, Road 3, Dhanmondi",
  "city": "Dhaka",
  "productName": "Cotton T-Shirt (L)",
  "quantity": 2,
  "unitPrice": 650,
  "deliveryCharge": 80,
  "orderSource": "WhatsApp",
  "channelUserId": "+8801712345678"
}
```
→ Customer automatically receives WhatsApp confirmation!

**Test dispatch (books Pathao):**
```bash
POST /api/orders/{id}/dispatch
```
→ Pathao consignment created + customer notified with tracking code!

---

## 📡 API Endpoints

| Method | URL | Description |
|--------|-----|-------------|
| GET | `/api/orders` | List orders (paginated, filter by status) |
| GET | `/api/orders/{id}` | Get order with delivery info |
| GET | `/api/orders/number/{orderNumber}` | Get by order number |
| POST | `/api/orders` | Create order + notify customer |
| PATCH | `/api/orders/{id}/status` | Update status + notify customer |
| POST | `/api/orders/{id}/dispatch` | Book Pathao + notify tracking |
| DELETE | `/api/orders/{id}` | Soft delete |
| GET | `/api/leads` | List leads |
| POST | `/api/leads` | Create lead + auto-reply |
| PATCH | `/api/leads/{id}/status` | Update lead status |
| POST | `/api/leads/{id}/convert` | Convert lead → order |
| GET | `/api/whatsappwebhook` | Meta verification |
| POST | `/api/whatsappwebhook` | Receive WhatsApp messages |
| GET | `/api/messengerwebhook` | Meta verification |
| POST | `/api/messengerwebhook` | Receive Messenger messages |
| GET | `/health` | DB connection health check |

---

## 🔄 Full Automation Flow

```
Customer texts on WhatsApp/Messenger
        ↓
Webhook controller receives message
        ↓
LeadService.CreateLeadAsync()
   → Saves lead to DB
   → Auto-replies: "Thanks! We'll contact you shortly."
        ↓
Admin reviews lead in dashboard
        ↓
Admin calls POST /api/leads/{id}/convert with order details
        ↓
OrderService.CreateOrderAsync()
   → Saves order with auto-generated order number
   → Sends WhatsApp/Messenger order confirmation to customer
        ↓
Admin calls POST /api/orders/{id}/dispatch
        ↓
PathaoService.CreateConsignmentAsync()
   → Authenticates with Pathao API
   → Books courier consignment
   → Returns tracking code
        ↓
Customer receives: "Your order is on the way! Track: {code}"
```

