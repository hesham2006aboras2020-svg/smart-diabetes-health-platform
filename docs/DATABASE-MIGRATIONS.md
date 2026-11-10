# Database migrations

The project uses Entity Framework Core with SQL Server.

## Create the initial migration

From the repository root:

```bash
dotnet restore
dotnet ef migrations add InitialIdentityAndDomain --project Infrastructure --startup-project API --output-dir Data/Migrations
```

## Apply the migration

```bash
dotnet ef database update --project Infrastructure --startup-project API
```

## Verify migrations

```bash
dotnet ef migrations list --project Infrastructure --startup-project API
```

Do not commit real passwords, JWT keys, or production connection strings. Use User Secrets or environment variables for sensitive values.
