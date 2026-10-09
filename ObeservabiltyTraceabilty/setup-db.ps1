# PowerShell Database Setup Script for Observability & Traceability Application
# Run as Administrator

param(
	[string]$PostgresHost = "localhost",
	[int]$PostgresPort = 5432,
	[string]$PostgresUser = "postgres",
	[string]$PostgresPassword = "postgres",
	[string]$DbName = "ObservabilityTraceability",
	[string]$DbUser = "obstraceability",
	[string]$DbPassword = "obstraceability123"
)

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Observability & Traceability Database Setup" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This script will:" -ForegroundColor Yellow
Write-Host "1. Create a new PostgreSQL database: $DbName"
Write-Host "2. Create a new database user: $DbUser"
Write-Host "3. Grant permissions to the user"
Write-Host ""

# Check if psql is available
$psqlPath = Get-Command psql -ErrorAction SilentlyContinue
if (-not $psqlPath) {
	Write-Host "ERROR: psql (PostgreSQL command-line tool) not found in PATH" -ForegroundColor Red
	Write-Host "Please install PostgreSQL or add it to your PATH" -ForegroundColor Red
	exit 1
}

Write-Host "PostgreSQL found at: $($psqlPath.Source)" -ForegroundColor Green
Write-Host ""

$sqlScript = @"
-- Create user if not exists
DO
`$`$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_user WHERE usename = '$DbUser') THEN
		CREATE USER $DbUser WITH PASSWORD '$DbPassword';
	END IF;
END
`$`$;

-- Create database if not exists
SELECT 'CREATE DATABASE "$DbName"' WHERE NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = '$DbName')\gexec

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE "$DbName" TO "$DbUser";
ALTER DATABASE "$DbName" OWNER TO "$DbUser";

\dc $DbName
"@

Write-Host "Executing SQL commands..." -ForegroundColor Yellow
$sqlScript | psql -h $PostgresHost -U $PostgresUser -p $PostgresPort

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Database setup completed successfully!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Connection String:" -ForegroundColor Yellow
Write-Host "Host=$PostgresHost;Database=$DbName;Username=$DbUser;Password=$DbPassword;Port=$PostgresPort" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Update appsettings.json with the connection string above" -ForegroundColor White
Write-Host "2. Run: dotnet ef migrations add InitialCreate" -ForegroundColor White
Write-Host "3. Run: dotnet ef database update" -ForegroundColor White
Write-Host "4. Run: dotnet run" -ForegroundColor White
Write-Host ""
