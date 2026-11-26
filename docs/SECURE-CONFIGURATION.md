# Secure local configuration

The repository does not contain admin credentials or a JWT signing key.

## ASP.NET Core User Secrets

From the repository root, configure secrets for the API project:

```bash
dotnet user-secrets --project API set "Jwt:Key" "replace-with-a-random-secret-at-least-32-characters"
dotnet user-secrets --project API set "AdminSeed:Username" "admin"
dotnet user-secrets --project API set "AdminSeed:Email" "admin@example.local"
dotnet user-secrets --project API set "AdminSeed:PhoneNumber" "+9677XXXXXXXX"
dotnet user-secrets --project API set "AdminSeed:FullName" "System Administrator"
dotnet user-secrets --project API set "AdminSeed:Password" "replace-with-a-strong-password"
```

The admin seed is optional. If the four `AdminSeed` values are not configured, no admin account is created. If an existing configured username is found, the seeder does not reset its password.

## Environment variables

For containers, CI/CD, or hosted environments, use environment variables instead of committing secrets:

- `Jwt__Key`
- `AdminSeed__Username`
- `AdminSeed__Email`
- `AdminSeed__PhoneNumber`
- `AdminSeed__FullName`
- `AdminSeed__Password`

ASP.NET Core maps double underscores in environment variable names to configuration section separators.

## Production rules

- Never commit `appsettings.Development.json` containing secrets.
- Never commit Firebase service-account credentials.
- Never place JWT keys or admin passwords in source code.
- Use the platform's secret manager or GitHub Actions Secrets for deployment automation.
- Rotate credentials if they were ever exposed in repository history.
