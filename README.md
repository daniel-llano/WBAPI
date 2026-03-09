# WBAPI

Web API con **.NET 8**, Arquitectura Hexagonal, MediatR y JWT.

## Estructura

```
WBAPI/
├── src/
│   ├── WBAPI.Domain/          # Entidades y puertos (interfaces)
│   ├── WBAPI.Application/     # Casos de uso MediatR (Commands/Queries/Handlers)
│   ├── WBAPI.Infrastructure/  # EF Core, SQL Server, JWT, Repositorios
│   └── WBAPI.API/             # Controllers, Middleware, Program.cs
└── WBAPI_Products.postman_collection.json
```

## Requisitos

- .NET 8 SDK
- SQL Server (local o Docker)
- dotnet-ef tool: `dotnet tool install --global dotnet-ef`

## Configuración

Edita `src/WBAPI.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WBAPIDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "<min 32 caracteres aleatorios seguros>",
    "Issuer": "WBAPI",
    "Audience": "WBAPIClients"
  }
}
```

## Ejecutar

```bash
# 1. Aplicar migraciones
dotnet ef database update --project src/WBAPI.Infrastructure --startup-project src/WBAPI.API

# 2. Correr la API
dotnet run --project src/WBAPI.API

# 3. Swagger UI disponible en https://localhost:7100/swagger
```

## Colección Postman

Importa `WBAPI_Products.postman_collection.json` y ejecuta los 11 requests en orden para correr el flujo completo de pruebas.

## Flujo de ramas Git

| Rama  | Propósito              |
|-------|------------------------|
| `dev` | Desarrollo diario      |
| `qa`  | Test / QA              |
| `main`| Producción (protegida) |
