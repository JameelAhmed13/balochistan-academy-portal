# BalochiAcademy .NET 10 Web API

Clean Architecture backend for the Balochistan Academy Portal.

## Stack

| Layer          | Technology                                 |
|----------------|--------------------------------------------|
| API            | ASP.NET Core 10 Web API                    |
| ORM            | Entity Framework Core 9 (SQL Server)       |
| Auth           | JWT Bearer + Refresh Tokens (BCrypt)       |
| Real-time      | SignalR                                    |
| Validation     | FluentValidation                           |
| Logging        | Serilog → Console + SQL Server             |
| Mapping        | AutoMapper                                 |
| Documentation  | Swagger / OpenAPI at `/docs`               |

## Architecture

```
server-dotnet/
  src/
    BalochiAcademy.Domain/          # Entities, enums — no dependencies
    BalochiAcademy.Application/     # DTOs, interfaces, service contracts
    BalochiAcademy.Infrastructure/  # EF Core, JWT, BCrypt, Serilog, SignalR
    BalochiAcademy.API/             # Controllers, middleware, Program.cs
  tests/
    BalochiAcademy.API.Tests/
    BalochiAcademy.Application.Tests/
  database/
    BalochiAcademyDB.sql            # Full MS SQL Server schema + seed data
```

## Setup

### 1. Create the database

Open SQL Server Management Studio or Azure Data Studio and run:

```sql
-- Run from project root:
-- database/BalochiAcademyDB.sql
```

### 2. Configure connection string

Edit `src/BalochiAcademy.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost\\SQLEXPRESS;Database=BalochiAcademyDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "your-strong-32-character-secret-key"
  }
}
```

### 3. Run EF migrations (optional — if using Code First instead of SQL script)

```bash
cd src/BalochiAcademy.API
dotnet ef migrations add InitialCreate --project ../BalochiAcademy.Infrastructure
dotnet ef database update
```

### 4. Run the API

```bash
cd src/BalochiAcademy.API
dotnet run
```

API will be available at `http://localhost:5000`  
Swagger UI at `http://localhost:5000/docs`

### 5. Connect Vue frontend

The Vite dev server proxies `/api` to the .NET backend. Update `vite.config.js`:

```js
server: {
  proxy: {
    '/api': {
      target: 'http://localhost:5000',
      changeOrigin: true,
    }
  }
}
```

## API Endpoints Summary

### Auth
| Method | Path                   | Auth     | Description              |
|--------|------------------------|----------|--------------------------|
| POST   | /api/auth/login        | Public   | Login → JWT + refresh    |
| POST   | /api/auth/register     | Public   | Student self-registration |
| GET    | /api/auth/me           | Bearer   | Current user profile      |
| PUT    | /api/auth/me/grade     | Bearer   | Update student grade      |
| POST   | /api/auth/refresh      | Public   | Rotate refresh token      |
| POST   | /api/auth/logout       | Bearer   | Revoke tokens             |

### Catalog
| Method | Path                                          | Auth     |
|--------|-----------------------------------------------|----------|
| GET    | /api/grades                                   | Public   |
| GET    | /api/grades/{code}/subjects                   | Public   |
| GET    | /api/grades/{code}/subjects/{id}/syllabus     | Bearer   |
| GET    | /api/tutors                                   | Public   |
| POST   | /api/admin/grades                             | Admin    |
| PUT    | /api/admin/grades/{code}                      | Admin    |
| DELETE | /api/admin/grades/{code}                      | Admin    |
| PUT    | /api/admin/grades/{code}/subjects             | Admin    |
| GET/POST/PUT/DELETE | /api/admin/subjects/{id}          | Admin    |
| POST/PUT/DELETE | /api/admin/tutors/{id}              | Admin    |
| CRUD   | /api/admin/syllabus/units|topics|objectives   | Admin    |

### Questions
| Method | Path                    | Auth   | Description              |
|--------|-------------------------|--------|--------------------------|
| GET    | /api/questions          | Bearer | Paginated + filtered      |
| GET    | /api/questions/{id}     | Bearer | Single question           |
| POST   | /api/questions          | Admin  | Create question           |
| PUT    | /api/questions/{id}     | Admin  | Update question           |
| DELETE | /api/questions/{id}     | Admin  | Delete question           |
| GET    | /api/questions/random   | Bearer | Random questions for test |

### Tests & Attempts
| Method | Path                      | Auth   |
|--------|---------------------------|--------|
| GET    | /api/tests                | Bearer |
| GET    | /api/tests/{id}           | Bearer |
| POST   | /api/tests                | Admin  |
| PATCH  | /api/tests/{id}/publish   | Admin  |
| DELETE | /api/tests/{id}           | Admin  |
| POST   | /api/tests/attempts       | Bearer |
| GET    | /api/tests/attempts       | Bearer |
| GET    | /api/tests/leaderboard    | Public |

### Coins
| Method | Path                              | Auth   |
|--------|-----------------------------------|--------|
| GET    | /api/coins/balance                | Bearer |
| GET    | /api/coins/transactions           | Bearer |
| GET/PUT | /api/coins/payout-account        | Bearer |
| POST   | /api/coins/withdraw               | Bearer |
| GET    | /api/coins/withdrawals            | Bearer |
| GET    | /api/admin/coins/withdrawals      | Admin  |
| PUT    | /api/admin/coins/withdrawals/{id} | Admin  |

### Complaints
| Method | Path                           | Auth   |
|--------|--------------------------------|--------|
| POST   | /api/complaints                | Bearer |
| GET    | /api/complaints                | Bearer |
| GET    | /api/admin/complaints          | Admin  |
| PUT    | /api/admin/complaints/{id}     | Admin  |

### Notifications
| Method | Path                              | Auth   |
|--------|-----------------------------------|--------|
| GET    | /api/notifications                | Bearer |
| PATCH  | /api/notifications/{id}/read      | Bearer |
| PATCH  | /api/notifications/read-all       | Bearer |
| POST   | /api/admin/notifications          | Admin  |

### Content
| Method | Path                       | Auth   |
|--------|----------------------------|--------|
| GET    | /api/content               | Bearer |
| GET    | /api/content/courses       | Bearer |
| GET    | /api/content/courses/{id}  | Bearer |
| CRUD   | /api/content/notes         | Bearer |
| CRUD   | /api/admin/content         | Admin  |

### Admin
| Method | Path                       | Auth  |
|--------|----------------------------|-------|
| GET    | /api/admin/users           | Admin |
| PATCH  | /api/admin/users/{id}/toggle | Admin |
| GET    | /api/admin/analytics       | Admin |
| GET/PUT | /api/admin/settings/{key} | Admin |

## Default Admin Credentials

```
Username: admin
Password: Admin@123   ← CHANGE THIS IN PRODUCTION
```

## Real-time (SignalR)

WebSocket endpoint: `ws://localhost:5000/hubs/notifications`

Client (Vue):
```js
import { HubConnectionBuilder } from '@microsoft/signalr'
const conn = new HubConnectionBuilder()
  .withUrl('/hubs/notifications', { accessTokenFactory: () => token })
  .build()
await conn.start()
conn.on('notification', (payload) => { /* show toast */ })
```
