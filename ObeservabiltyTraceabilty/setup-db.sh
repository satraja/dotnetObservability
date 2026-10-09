#!/bin/bash
# Database Setup Script for Observability & Traceability Application
# This script creates the PostgreSQL database and user

# Configuration
POSTGRES_HOST="localhost"
POSTGRES_PORT="5432"
POSTGRES_USER="postgres"  # Default postgres superuser
POSTGRES_PASSWORD="postgres"  # Change this to your password
DB_NAME="ObservabilityTraceability"
DB_USER="obstraceability"
DB_PASSWORD="obstraceability123"

echo "=========================================="
echo "Observability & Traceability Database Setup"
echo "=========================================="
echo ""
echo "This script will:"
echo "1. Create a new PostgreSQL database: $DB_NAME"
echo "2. Create a new database user: $DB_USER"
echo "3. Grant permissions to the user"
echo ""
read -p "Press Enter to continue or Ctrl+C to cancel..."

# Create database and user
PGPASSWORD=$POSTGRES_PASSWORD psql -h $POSTGRES_HOST -U $POSTGRES_USER -p $POSTGRES_PORT <<EOF
-- Create user if not exists
DO
\$\$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_user WHERE usename = '$DB_USER') THEN
		CREATE USER $DB_USER WITH PASSWORD '$DB_PASSWORD';
	END IF;
END
\$\$;

-- Create database if not exists
SELECT 'CREATE DATABASE $DB_NAME' WHERE NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = '$DB_NAME')\gexec

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE $DB_NAME TO $DB_USER;
ALTER DATABASE $DB_NAME OWNER TO $DB_USER;

EOF

echo ""
echo "=========================================="
echo "Database setup completed successfully!"
echo "=========================================="
echo ""
echo "Connection String:"
echo "Host=localhost;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASSWORD;Port=$POSTGRES_PORT"
echo ""
echo "Next steps:"
echo "1. Update appsettings.json with the connection string above"
echo "2. Run: dotnet ef migrations add InitialCreate"
echo "3. Run: dotnet ef database update"
echo "4. Run: dotnet run"
