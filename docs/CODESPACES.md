# GitHub Codespaces

This repository can be developed and tested in GitHub Codespaces.

## What the Codespace provides

The Dev Container installs:

- .NET 10 SDK
- Entity Framework Core CLI 10.0.14
- Flutter stable with web support
- Dart SDK supplied by Flutter
- Git and native tools required by Flutter web builds

The repository remains the source of truth. The Codespace is only a development environment.

## Create the Codespace

1. Open the repository on GitHub.
2. Select Code -> Codespaces -> Create codespace on main (or on your feature branch).
3. Wait for the Dev Container to build.
4. The postCreateCommand restores the .NET solution and Flutter packages.

## Configure secrets

Do not put real credentials in appsettings.json or commit secret files.

For a Codespace, configure environment variables through GitHub Codespaces secrets or the Codespace environment. ASP.NET Core uses the double-underscore convention.

Required values for a real database-backed run:

- ConnectionStrings__DefaultConnection
- Jwt__Key
- AdminSeed__Username
- AdminSeed__Email
- AdminSeed__Password
- AdminSeed__FullName

Firebase is optional while Firebase:Enabled is false. If Firebase is enabled, provide its credentials through a secure mechanism; never commit a Firebase service-account JSON file.

## Start the API

From the repository root:

~~~bash
dotnet run --project API/API.csproj
~~~

The Codespace exposes the API on port 5000.

Verify it with:

~~~bash
curl http://localhost:5000/api/health
curl http://localhost:5000/api/health/live
curl http://localhost:5000/api/health/ready
~~~

The ready endpoint requires a reachable SQL Server database.

## Apply EF Core migrations

After ConnectionStrings__DefaultConnection points to the intended SQL Server database:

~~~bash
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj
~~~

Do not edit applied migrations manually.

## Start Flutter Web

First obtain the forwarded HTTPS URL for the API from the Codespaces Ports panel. Then run:

~~~bash
cd mobile
flutter run -d web-server --web-hostname 0.0.0.0 --web-port 8080 --dart-define=API_BASE_URL=https://<your-forwarded-api-host>
~~~

Use the forwarded HTTPS API URL, not localhost, when Flutter Web is opened through the forwarded browser URL. This avoids browser mixed-content restrictions.

## Verification commands

Backend:

~~~bash
dotnet restore DiabetesPlatform.sln
dotnet build DiabetesPlatform.sln --configuration Release
dotnet test DiabetesPlatform.sln --configuration Release
~~~

Flutter:

~~~bash
cd mobile
flutter pub get
flutter analyze
flutter test
flutter build web --release --dart-define=API_BASE_URL=https://<your-forwarded-api-host>
~~~

## Local SQL Server in Codespaces

The Codespace now includes the Docker CLI and automatically starts a local SQL Server 2022 Developer container during creation.

The bootstrap script:

- Creates a persistent Docker volume named `smart-diabetes-sqlserver-data`.
- Starts SQL Server on `localhost:1433`.
- Generates a development-only SA password if one does not already exist.
- Stores the generated password and connection string in `.codespaces/sql.env`, which is ignored by Git.
- Waits for SQL Server to become ready.
- Applies the existing EF Core migrations. It does not create, delete, or modify migrations.

After the Codespace is created, load the connection settings before starting the API:

~~~bash
source .codespaces/sql.env
dotnet run --project API/API.csproj
~~~

Then verify:

~~~bash
curl http://localhost:5000/api/health
curl http://localhost:5000/api/health/ready
~~~

To inspect the SQL Server container:

~~~bash
docker ps
docker logs smart-diabetes-sqlserver
~~~

The local SQL Server is intended for development and testing inside the Codespace. For a persistent shared/cloud environment, use a managed SQL Server-compatible service such as Azure SQL and store the connection string as a secret.
### Rebuilding an existing Codespace

The Codespace configuration uses the official Docker-in-Docker Dev Container Feature so Docker is available to the SQL Server startup script. After pulling a change to `.devcontainer/devcontainer.json`, an existing Codespace must be rebuilt from the Command Palette (`Codespaces: Rebuild Container`) before Docker changes take effect. The development environment also generates a local JWT signing key alongside the SQL Server development credentials in `.codespaces/sql.env`; this file is ignored by Git and must never be committed.

