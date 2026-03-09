# WBAPI

Web API built with .NET 8 following Hexagonal Architecture (Ports and Adapters). Implements CQRS via MediatR, JWT authentication, and a full CRUD for Products backed by SQL Server.


## Solution structure

```
WBAPI/
├── src/
│   ├── WBAPI.Domain/          # Entities, port interfaces, domain exceptions
│   ├── WBAPI.Application/     # Commands, Queries, Handlers (MediatR), DTOs, Validators
│   ├── WBAPI.Infrastructure/  # EF Core DbContext, Repositories, UnitOfWork, JwtService
│   └── WBAPI.API/             # Controllers, ExceptionHandlingMiddleware, Program.cs
├── tests/
│   ├── WBAPI.UnitTests/       # xUnit unit tests (Moq, FluentAssertions)
│   └── WBAPI.IntegrationTests/# xUnit integration tests (WebApplicationFactory, EF InMemory)
├── WBAPI_Products.postman_collection.json
└── WBAPI.sln
```


## Prerequisites

- .NET 8 SDK (https://dotnet.microsoft.com/download)
- SQL Server 2019 or later (local instance or Docker)
- Entity Framework Core CLI tool:

```
dotnet tool install --global dotnet-ef
```

Verify the installation:

```
dotnet ef --version
```


## Configuration

All runtime settings live in `src/WBAPI.API/appsettings.json`.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WBAPIDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "CHANGE_ME_TO_A_VERY_LONG_AND_SECURE_SECRET_KEY_AT_LEAST_32_CHARS",
    "Issuer": "WBAPI",
    "Audience": "WBAPIClients",
    "ExpirationHours": "1"
  }
}
```

Settings reference:

| Key | Description | Required |
|-----|-------------|----------|
| `ConnectionStrings:DefaultConnection` | ADO.NET connection string to your SQL Server instance | Yes |
| `Jwt:Secret` | Symmetric signing key. Must be at least 32 characters. Never commit a real value. | Yes |
| `Jwt:Issuer` | Token issuer claim. Must match the value configured in the Blazor client. | Yes |
| `Jwt:Audience` | Token audience claim. Must match the value configured in the Blazor client. | Yes |
| `Jwt:ExpirationHours` | Token lifetime in hours. Defaults to 1. | No |

For a SQL Server Docker container, replace the connection string with:

```
Server=localhost,1433;Database=WBAPIDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;
```

For a named instance:

```
Server=.\\SQLEXPRESS;Database=WBAPIDb;Trusted_Connection=True;TrustServerCertificate=True;
```


## Database setup

Apply the existing migration to create the schema before the first run:

```
dotnet ef database update --project src/WBAPI.Infrastructure --startup-project src/WBAPI.API
```

If you need to recreate the migration from scratch:

```
dotnet ef migrations add InitialCreate --project src/WBAPI.Infrastructure --startup-project src/WBAPI.API
dotnet ef database update --project src/WBAPI.Infrastructure --startup-project src/WBAPI.API
```


## Running the API

HTTPS profile (recommended):

```
dotnet run --project src/WBAPI.API --launch-profile https
```

HTTP only profile:

```
dotnet run --project src/WBAPI.API --launch-profile http
```

The API will be available at:

- HTTPS: https://localhost:7299
- HTTP:  http://localhost:5090
- Swagger UI: https://localhost:7299/swagger (Development environment only)

The application uses the `Development` environment by default when running with `dotnet run`. In `Development`, Swagger UI is enabled automatically.


## API endpoints

All product endpoints require a valid JWT Bearer token in the `Authorization` header.

Authentication (no token required):

| Method | Route | Description | Roles |
|--------|-------|-------------|-------|
| POST | /api/auth/register | Register a new user | Public |
| POST | /api/auth/login | Authenticate and receive a JWT | Public |

Products:

| Method | Route | Description | Roles |
|--------|-------|-------------|-------|
| GET | /api/products | List all products | User, Admin |
| GET | /api/products/{id} | Get a single product by ID | User, Admin |
| POST | /api/products | Create a product | Admin |
| PUT | /api/products/{id} | Update a product | Admin |
| DELETE | /api/products/{id} | Delete a product | Admin |

Request body for POST /api/auth/register:

```json
{
  "username": "admin1",
  "password": "Admin@1234",
  "role": "Admin"
}
```

Valid roles: `Admin`, `User`, `Invitado`. When `role` is omitted the default is `Invitado`.

Users:

| Method | Route | Description | Roles |
|--------|-------|-------------|-------|
| GET | /api/users | List all users | Admin |
| PUT | /api/users/{id}/role | Assign a new role to a user | Admin |
| DELETE | /api/users/{id} | Delete a user account | Admin |

Request body for PUT /api/users/{id}/role:

```json
{
  "role": "User"
}
```

Request body for POST /api/products:

```json
{
  "name": "Laptop Pro",
  "description": "High-end laptop",
  "price": 1299.99,
  "category": "Electronics",
  "stock": 50
}
```


## CORS

The API allows cross-origin requests from the APPWeb Blazor application:

- https://localhost:7287
- http://localhost:5152

If you run APPWeb on a different port, update the `WithOrigins` call in `src/WBAPI.API/Program.cs` accordingly.


## Running tests

Run all tests (unit + integration):

```
dotnet test
```

Run only unit tests:

```
dotnet test tests/WBAPI.UnitTests
```

Run only integration tests:

```
dotnet test tests/WBAPI.IntegrationTests
```

Run with verbose output:

```
dotnet test --logger "console;verbosity=detailed"
```

Test summary:

| Project | Type | Count |
|---------|------|-------|
| WBAPI.UnitTests | Domain entity tests, Command/Query handler tests (Moq) | 17 |
| WBAPI.IntegrationTests | Auth controller tests, Products controller tests (WebApplicationFactory + EF InMemory) | 16 |

Integration tests run against an in-memory database and do not require SQL Server or a running API process.


## Postman collection

The file `WBAPI_Products.postman_collection.json` contains 11 requests covering the full flow.

To use it:

1. Open Postman.
2. Click Import and select the JSON file.
3. The collection uses three variables automatically managed by test scripts:
   - `base_url`: defaults to `https://localhost:7299`
   - `jwt_token`: populated after a successful login
   - `product_id`: populated after a successful product creation
4. Run the requests in order inside the Auth folder first, then the Products folder.

The collection includes negative test cases for unauthorized access and not-found responses.


## Git workflow

### Branch model

| Branch | Purpose | Merge strategy |
|--------|---------|----------------|
| `main` | Production. Tagged with semver on every release. Protected — no direct pushes. | Merge commit from `qa` |
| `qa` | Staging / QA sign-off. Receives only pre-validated code from `dev`. | Merge commit from `dev` |
| `dev` | Integration branch. All feature branches merge here first. | Squash merge from feature branches |
| `feature/*` | One branch per feature or task, always branched from `dev`. | Squash merged into `dev` via PR |
| `fix/*` | Non-production bug fixes, branched from `dev`. | Squash merged into `dev` via PR |
| `hotfix/*` | Critical production fixes, branched from `main`. | Merge commit into `main` + `dev` |
| `chore/*` | Dependency upgrades, CI changes, non-functional work. | Squash merged into `dev` via PR |

### Standard feature flow

```
# 1 — start from dev
git checkout dev
git pull origin dev
git checkout -b feature/my-feature

# 2 — develop, commit with conventional commit messages
git add -A
git commit -m "feat: describe what this does"

# 3 — push and open PR into dev
git push -u origin feature/my-feature
gh pr create --base dev --head feature/my-feature --title "feat: my feature" --body "..."
gh pr merge --squash --delete-branch

# 4 — promote dev → qa
gh pr create --base qa --head dev --title "promote: dev → qa vX.Y.Z" --body "..."
gh pr merge --merge

# 5 — promote qa → main (release)
gh pr create --base main --head qa --title "release: vX.Y.Z" --body "..."
gh pr merge --merge
git tag vX.Y.Z && git push origin vX.Y.Z
```

### Hotfix flow

```
git checkout main
git pull origin main
git checkout -b hotfix/critical-bug-description

# fix, commit
git commit -m "fix: describe critical fix"
git push -u origin hotfix/critical-bug-description

# PR hotfix → main
gh pr create --base main --head hotfix/... --title "hotfix: ..."
gh pr merge --merge
git tag vX.Y.Z-patch && git push origin vX.Y.Z-patch

# Back-merge into dev so the fix is not lost
git checkout dev && git merge main && git push origin dev
```

### Commit message conventions

Follows [Conventional Commits](https://www.conventionalcommits.org/):

| Prefix | When to use |
|--------|-------------|
| `feat:` | New feature |
| `fix:` | Bug fix |
| `test:` | Adding or updating tests |
| `docs:` | Documentation only |
| `refactor:` | Code restructure without behaviour change |
| `chore:` | Tooling, dependencies, CI/CD |

### Version tags

Tags follow [Semantic Versioning](https://semver.org/) (`vMAJOR.MINOR.PATCH`) and are created on `main` only after a release merge.
