#!/usr/bin/env bash
set -euo pipefail

ROOT="$(git rev-parse --show-toplevel)"
STATE_DIR="$ROOT/.codespaces"
ENV_FILE="$STATE_DIR/sql.env"
CONTAINER_NAME="smart-diabetes-sqlserver"
VOLUME_NAME="smart-diabetes-sqlserver-data"
IMAGE="mcr.microsoft.com/mssql/server:2022-latest"

mkdir -p "$STATE_DIR"
chmod 700 "$STATE_DIR"

if ! command -v docker >/dev/null 2>&1; then
  echo "Docker CLI is not installed in this Codespace."
  exit 1
fi

if [[ -f "$ENV_FILE" ]]; then
  source "$ENV_FILE"
fi

if [[ -z "${MSSQL_SA_PASSWORD:-}" ]]; then
  MSSQL_SA_PASSWORD="$(openssl rand -hex 16)A1!"
fi

if [[ -z "${JWT_KEY:-}" ]]; then
  JWT_KEY="$(openssl rand -hex 32)"
fi

umask 077
cat > "$ENV_FILE" <<EOF
MSSQL_SA_PASSWORD='$MSSQL_SA_PASSWORD'
ConnectionStrings__DefaultConnection='Server=localhost,1433;Database=DiabetesPlatformDb;User Id=sa;Password=$MSSQL_SA_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true'
Jwt__Key='$JWT_KEY'
EOF

echo "Waiting for Docker daemon..."
for _ in {1..60}; do
  if docker info >/dev/null 2>&1; then
    break
  fi
  sleep 2
done

if ! docker info >/dev/null 2>&1; then
  echo "Docker daemon is not available from the Codespace after 120 seconds."
  echo "Rebuild the Codespace after pulling the latest main branch."
  exit 1
fi

if docker ps -a --format '{{.Names}}' | grep -qx "$CONTAINER_NAME"; then
  docker start "$CONTAINER_NAME" >/dev/null 2>&1 || true
else
  docker volume create "$VOLUME_NAME" >/dev/null
  docker run -d     --name "$CONTAINER_NAME"     --restart unless-stopped     -e ACCEPT_EULA=Y     -e MSSQL_PID=Developer     -e MSSQL_SA_PASSWORD="$MSSQL_SA_PASSWORD"     -p 1433:1433     -v "$VOLUME_NAME:/var/opt/mssql"     "$IMAGE" >/dev/null
fi

echo "Waiting for SQL Server to become ready..."
for _ in {1..60}; do
  if docker exec "$CONTAINER_NAME" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" >/dev/null 2>&1; then
    echo "SQL Server is ready."
    break
  fi
  sleep 2
done

if ! docker exec "$CONTAINER_NAME" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" >/dev/null 2>&1; then
  echo "SQL Server did not become ready within 120 seconds."
  docker logs --tail 80 "$CONTAINER_NAME" || true
  exit 1
fi

cd "$ROOT/Infrastructure"
dotnet ef database update --project Infrastructure.csproj --startup-project ../API/API.csproj

echo
echo "SQL Server is running on localhost:1433."
echo "Development connection settings are stored in $ENV_FILE and are ignored by Git."
echo "For the API, run: source $ENV_FILE && dotnet run --project API"
