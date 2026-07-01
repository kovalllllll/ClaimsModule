# Claims Module — FNOL & Reserve Management

DICEUS Fullstack Engineer assessment: greenfield Claims Management vertical slice (.NET 9 + Angular 18).

This README satisfies **Assessment §4.2 Setup Instructions**. Deployed endpoints for the live review are listed under **§4.3 Deployed application** below.

---

## 1. Prerequisites

Install the following before running locally or deploying:

| Tool | Version | Notes |
|------|---------|--------|
| [.NET SDK](https://dotnet.microsoft.com/download) | **9.0.x** | Backend API, EF migrations, tests |
| [Node.js](https://nodejs.org/) | **20 LTS** (or newer) | Angular UI; matches CI `NODE_VERSION` |
| npm | Bundled with Node | `npm ci` in `claims-module-ui/` |
| SQL Server | **2022** or **LocalDB** | Default connection uses `(localdb)\mssqllocaldb` |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional) | Current | `docker compose` for SQL Server and/or full stack |
| Git | Any recent | Clone repository and CI |

**EF Core CLI (local migrations):** restore the pinned tool from the repo root:

```bash
dotnet tool restore
```

Tool manifest: [`.config/dotnet-tools.json`](.config/dotnet-tools.json) (`dotnet-ef` **9.0.5**).

**Environment:** Windows, macOS, or Linux. For local Angular dev, use **http** (not https) on port **4200** so the dev proxy to the API works.

---

## 2. Local development setup

Run from the repository root unless noted.

### Step 1 — Restore and build (optional check)

```bash
dotnet restore ClaimsModule.sln
dotnet build ClaimsModule.sln
```

### Step 2 — Database (schema + reference seed)

See [§4 Database migration and seed](#4-database-migration-and-seed) for details. Minimal commands:

```bash
dotnet tool restore
dotnet ef database update \
  --project src/ClaimsModule.Persistence/ClaimsModule.Persistence.csproj \
  --startup-project src/ClaimsModule.API/ClaimsModule.API.csproj
```

**Docker SQL only:** start SQL Server, then point the API at it:

```bash
docker compose up -d sqlserver
```

After the container is healthy, set `ConnectionStrings:DefaultConnection` in [`appsettings.Development.json`](src/ClaimsModule.API/appsettings.Development.json) to:

```text
Server=localhost,1433;Database=ClaimsModule;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true
```

Then run `dotnet ef database update` again (same commands as above).

### Step 3 — Backend API

```bash
dotnet run --project src/ClaimsModule.API/ClaimsModule.API.csproj --launch-profile http
```

| Endpoint | URL |
|----------|-----|
| Swagger | http://localhost:5260/swagger |
| Hangfire dashboard | http://localhost:5260/hangfire (open access in Development) |

### Step 4 — Frontend

```bash
cd claims-module-ui
npm ci
npm start
```

Open **http://localhost:4200**. The dev server proxies `/api` to `http://localhost:5260` (see `claims-module-ui/proxy.conf.json`).

### Step 5 — Sign in

Use the **role switcher** in the UI header, or obtain a mock JWT:

```http
POST http://localhost:5260/api/auth/token
Content-Type: application/json

{ "role": "handler" }
```

Roles: `handler`, `supervisor`, `manager` (see [Test credentials](#test-credentials-mock-jwt)).

### All-in-one Docker (API + SQL)

```bash
docker compose up --build
```

The API container sets `Database__ApplyMigrationsOnStartup=true`, applies EF migrations (including seed data), then listens on **http://localhost:5260**.

---

## 3. Environment variables and appsettings

Configuration uses ASP.NET Core conventions: [`appsettings.json`](src/ClaimsModule.API/appsettings.json), environment-specific overrides ([`appsettings.Development.json`](src/ClaimsModule.API/appsettings.Development.json)), and environment variables with `__` nesting (e.g. `ConnectionStrings__DefaultConnection` in Azure App Service).

### Backend (`ClaimsModule.API`)

| Key | Description | Example value (local) |
|-----|-------------|------------------------|
| `ConnectionStrings:DefaultConnection` | SQL Server for EF + Hangfire | `Server=(localdb)\mssqllocaldb;Database=ClaimsModule;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True` |
| `Jwt:SecretKey` | Mock JWT signing (min 32 chars) | `ClaimsModuleAssessmentSecretKey2026MustBe32CharsMin!` |
| `Jwt:Issuer` | JWT issuer | `ClaimsModule` |
| `Jwt:Audience` | JWT audience | `ClaimsModule` |
| `Tenant:DefaultOrganisationId` | Tenant for mock auth / seed data | `00000000-0000-0000-0000-000000000001` |
| `Database:ApplyMigrationsOnStartup` | Run `dotnet ef database update` on startup | `false` (local); `true` in Docker compose API service |
| `StorageProvider` | `LocalFileSystem` or `AzureBlob` | `LocalFileSystem` |
| `AzureBlob:ConnectionString` | Azure Storage (required if `AzureBlob`) | _(empty locally)_ |
| `AzureBlob:ContainerName` | Blob container name | `claim-documents` |
| `LocalStorage:BasePath` | Upload folder (local provider) | `./uploads` |
| `LocalStorage:BaseUrl` | URL path prefix for signed download URLs | `/uploads` |
| `LocalStorage:SigningKey` | HMAC key for local SAS-style URLs | `ClaimsModuleLocalStorageSigningKeyDevOnly2026!` |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Production` | `Development` when using `launchSettings` |

### Frontend (`claims-module-ui`)

| File | Key | Example (local) |
|------|-----|-----------------|
| [`environment.ts`](claims-module-ui/src/environments/environment.ts) | `apiUrl` | `http://localhost:5260` |
| [`environment.prod.ts`](claims-module-ui/src/environments/environment.prod.ts) | `apiUrl` | `https://claims-module-api-koval.azurewebsites.net` (set to your App Service URL before production build) |

Build production UI: `cd claims-module-ui && npm run build` → output in `dist/claims-module-ui/browser/`.

### Azure App Service (additional settings)

When `StorageProvider` is `AzureBlob`, also set:

| App setting | Example |
|-------------|---------|
| `StorageProvider` | `AzureBlob` |
| `AzureBlob__ConnectionString` | _(from Storage account → Access keys)_ |
| `AzureBlob__ContainerName` | `claim-documents` |
| `ConnectionStrings__DefaultConnection` | _(Azure SQL ADO.NET connection string)_ |
| `Jwt__SecretKey` | _(strong secret, 32+ characters)_ |

Enable **Always On** on the App Service plan so in-process Hangfire jobs keep running.

---

## 4. Database migration and seed

### Apply migrations

Migrations live in [`src/ClaimsModule.Persistence/Migrations/`](src/ClaimsModule.Persistence/Migrations/). Apply to an empty database:

```bash
dotnet tool restore
dotnet ef database update \
  --project src/ClaimsModule.Persistence/ClaimsModule.Persistence.csproj \
  --startup-project src/ClaimsModule.API/ClaimsModule.API.csproj
```

List migrations (optional):

```bash
dotnet ef migrations list \
  --project src/ClaimsModule.Persistence/ClaimsModule.Persistence.csproj \
  --startup-project src/ClaimsModule.API/ClaimsModule.API.csproj
```

CI runs the same against a fresh SQL Server container (see [`.github/workflows/ci.yml`](.github/workflows/ci.yml)).

### Reference data seed (no separate SQL scripts)

Reference data is embedded with EF Core **`HasData()`** in entity configurations and applied automatically when migrations run. There are **no** standalone `.sql` seed files.

| Data | Source file | Contents |
|------|-------------|----------|
| Cause of loss codes | [`CauseOfLossCodeConfiguration.cs`](src/ClaimsModule.Persistence/Configurations/CauseOfLossCodeConfiguration.cs) | 10 codes (e.g. `COL-FIRE`, `COL-FLOOD`, …) |
| Simulated policies | [`PolicyConfiguration.cs`](src/ClaimsModule.Persistence/Configurations/PolicyConfiguration.cs) | 5 policies (including expired `POL-2023-000099` for BR-C-02 tests) |
| Organisation | [`SeedConstants.cs`](src/ClaimsModule.Persistence/Seeding/SeedConstants.cs) | `OrganisationId = 00000000-0000-0000-0000-000000000001` |

**Not seeded in the database:** mock users (`handler` / `supervisor` / `manager`) — issued via `POST /api/auth/token` only.

**Runtime data:** claim numbers use table `ClaimSequences` (allocated when claims are created). Hangfire creates its own `HangFire` schema tables on first API start.

To change seed data, edit the configuration `HasData` blocks, add a new EF migration, and run `database update` again.

---

## 5. Azure deployment

### Azure resources

This project deploys to **two Linux App Service** web apps (not Azure Static Web Apps):

| Resource | Stack | Purpose |
|----------|--------|---------|
| **Azure SQL Database** | SQL | Application schema, Hangfire storage, EF migrations target |
| **App Service — API** | Linux, **.NET 9** | Host `ClaimsModule.API` (Swagger, Hangfire, REST) |
| **App Service — UI** | Linux, **Node 22** | Host Angular static build (`dist/claims-module-ui/browser`) |
| **Storage account** (Blob) | — | Document uploads; container **`claim-documents`** |

Optional: Azure Key Vault for secrets; Application Insights for monitoring.

**UI App Service (portal):** create a separate Web App (e.g. `claims-module-ui-koval`), runtime stack **Node 22 LTS** on Linux. The CI pipeline deploys the pre-built static files from `claims-module-ui/dist/claims-module-ui/browser` (no server-side `npm start` in Azure). Ensure `environment.prod.ts` points `apiUrl` at the API App Service **before** the workflow builds the frontend artifact.

### Reproduce deployment (GitHub Actions)

1. Create the Azure resources above (two App Services + SQL + Storage).
2. In GitHub: **Settings → Secrets and variables → Actions**, add these **six** repository secrets (matches [`.github/workflows/ci.yml`](.github/workflows/ci.yml) deploy job — no Static Web Apps token):

   | Secret | Value |
   |--------|--------|
   | `AZURE_API_APP_NAME` | Short name of the **API** App Service (Linux, .NET 9), e.g. `claims-module-api-koval` |
   | `AZURE_API_PUBLISH_PROFILE` | Full XML from API App Service → **Download publish profile** |
   | `AZURE_API_URL` | Public API base URL, no trailing slash; used for post-deploy Swagger smoke test, e.g. `https://claims-module-api-koval.azurewebsites.net` |
   | `AZURE_SQL_CONNECTION_STRING` | ADO.NET connection string to Azure SQL (`ClaimsModuleDb`) |
   | `AZURE_UI_APP_NAME` | Short name of the **UI** App Service (Linux, Node 22), e.g. `claims-module-ui-koval` |
   | `AZURE_UI_PUBLISH_PROFILE` | Full XML from UI App Service → **Download publish profile** |

3. **Actions → CI → Run workflow** on branch `main` or `master`, enable **Deploy to Azure**.
4. The workflow builds and tests, then the `deploy` job: applies EF migrations to Azure SQL, deploys the API package to the API App Service, deploys the UI static files to the UI App Service, and smoke-checks Swagger.
5. Configure the **API** App Service → **Configuration** (see [Azure App Service settings](#azure-app-service-additional-settings)): SQL connection string, JWT, `StorageProvider=AzureBlob`, blob connection string, **Always On = On**.
6. On the **API** App Service, enable **CORS** for your UI origin (e.g. `https://<ui-app-name>.azurewebsites.net`) because the browser calls the API directly using `environment.prod.ts` `apiUrl`.
7. Set `apiUrl` in [`environment.prod.ts`](claims-module-ui/src/environments/environment.prod.ts) to the API URL **before** running deploy (the `frontend` job runs `npm run build` in CI).

Manual alternative: `dotnet publish` the API, `dotnet ef database update` with the Azure SQL connection string, `npm run build` in `claims-module-ui`, then publish `dist/claims-module-ui/browser` to the UI App Service (FTP, VS, or `azure/webapps-deploy` with the UI publish profile).

### Hangfire on Azure

- Jobs run **in-process** on the App Service (not separate WebJobs).
- Open `/hangfire` with JWT role **supervisor** or **manager** (use a browser extension such as ModHeader to send `Authorization: Bearer <token>` from `POST /api/auth/token`).
- Failed GL jobs: use **Retry GL** in the claim UI or **Requeue** in the Hangfire dashboard.

---

## 6. Application walkthrough

Brief tour of implemented features (Assessment §4.2.6):

1. **Login** — header role switcher (`handler` / `supervisor` / `manager`) or mock JWT from `/api/auth/token`.
2. **FNOL** — **Log New Claim**: policy search, loss details, at least one **Claimant** party, optional initial reserve with authority hints.
3. **Claims list** — filters (status, dates, cause of loss, handler), search by claim number / policy / client.
4. **Claim detail — Overview** — loss event fields, editable claim notes.
5. **Reserves** — summary per component; add reserve; as **handler** try ≤ $10k (auto-approved); as **supervisor** approve larger pending amounts.
6. **GL posting** — transaction rows show GL status (Pending / Posted / Failed); **Retry GL** for failed postings.
7. **Close claim** — **Change status → Closed** shows live CC-01–CC-04 checklist before confirming.
8. **Documents** — upload PDF/images/office files; download via short-lived SAS URL (not proxied through API).
9. **Audit log** — append-only events for status, reserves, documents, SLA, etc.
10. **Background jobs** — Hangfire: GL posting on reserve approval; SLA monitoring every 15 minutes (`SLA_BREACH_DETECTED` audit only).

Document download uses SAS URLs from `GET /api/claims/{id}/documents` or claim detail (FRS §13).

---

## §4.3 Deployed application (live review)

| Service | URL |
|---------|-----|
| Backend (Swagger) | https://claims-module-api-koval.azurewebsites.net/swagger |
| Frontend (App Service, Node 22) | https://claims-module-ui-koval.azurewebsites.net |

### Test credentials (mock JWT)

Assessment requires at least **handler** and **supervisor** for reserve approval flows.

| Role | Name | Obtain token |
|------|------|--------------|
| handler | John Handler | `POST /api/auth/token` with `{ "role": "handler" }` |
| supervisor | Sarah Supervisor | `{ "role": "supervisor" }` |
| manager | Mike Manager | `{ "role": "manager" }` |

Example (replace host with your API URL):

```bash
curl -s -X POST https://claims-module-api-koval.azurewebsites.net/api/auth/token \
  -H "Content-Type: application/json" \
  -d "{\"role\":\"supervisor\"}"
```

Use the returned `token` as `Authorization: Bearer <token>` in Swagger **Authorize** or in the UI role switcher flow.

**OrganisationId (tenant):** `00000000-0000-0000-0000-000000000001`

---

## Solution structure

```
src/
  ClaimsModule.Domain/
  ClaimsModule.Application/    # MediatR CQRS, validators, DTOs
  ClaimsModule.Persistence/    # EF Core, migrations, HasData seed
  ClaimsModule.Infrastructure/ # Hangfire, storage, audit
  ClaimsModule.API/              # REST controllers, mock JWT
claims-module-ui/              # Angular 18 SPA
tests/                         # Unit and API integration tests
```

---