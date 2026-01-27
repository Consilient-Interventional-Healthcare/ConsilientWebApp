#!/bin/bash

# Startup timing helper
STARTUP_START=$(date +%s%N)
log_timing() {
    local now=$(date +%s%N)
    local elapsed_ms=$(( (now - STARTUP_START) / 1000000 ))
    echo "[+${elapsed_ms}ms] $1"
}

log_timing "Starting SQL Server..."

# Start the SQL Server process in the background
/opt/mssql/bin/sqlservr &

# Wait for SQL Server to be ready
log_timing "Waiting for SQL Server to accept connections..."
until /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" &>/dev/null; do
  sleep 1
done
log_timing "SQL Server is ready"


# --- Argument Parsing ---
if [ "$1" != "-databases" ]; then
  echo "Error: Usage: $0 -databases <db1> <db2> ..."
  exit 1
fi
shift # Remove '-databases'
DATABASES=("$@") # Get the list of database names

if [ ${#DATABASES[@]} -eq 0 ]; then
  echo "Error: No database names provided."
  exit 1
fi

# --- Check for reset markers and drop databases if requested ---
for DB_NAME in "${DATABASES[@]}"; do
    if [ -f "/var/opt/mssql/.reset-${DB_NAME}" ]; then
        log_timing "Reset marker found for $DB_NAME. Dropping database..."
        # Wait a bit more for SQL Server to be fully ready for DDL operations
        sleep 5
        /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -Q "
            IF EXISTS (SELECT 1 FROM sys.databases WHERE name = '$DB_NAME')
            BEGIN
                ALTER DATABASE [$DB_NAME] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [$DB_NAME];
            END
        "
        # Clean up data files in case DROP didn't fully succeed
        rm -f /var/opt/mssql/data/${DB_NAME}.mdf /var/opt/mssql/data/${DB_NAME}_log.ldf
        rm "/var/opt/mssql/.reset-${DB_NAME}"
        echo "Database $DB_NAME dropped. Will be recreated."
    fi
done

# --- Script Execution Loop ---
for DB_NAME in "${DATABASES[@]}"; do
  log_timing "Processing database: $DB_NAME"
  SCRIPT_DIR="./$DB_NAME"

  if [ ! -d "$SCRIPT_DIR" ]; then
    log_timing "Warning: Db directory not found for database '$DB_NAME'. Skipping."
    continue
  fi

    # Check if the database already exists before trying to create it
  if /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -h -1 -Q "SET NOCOUNT ON; SELECT 1 FROM sys.databases WHERE name = N'$DB_NAME'" | grep -q 1; then
    log_timing "Database '$DB_NAME' already exists. Skipping creation."
  else
    log_timing "Database '$DB_NAME' does not exist. Creating..."
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -Q "CREATE DATABASE [$DB_NAME]"
    log_timing "Database '$DB_NAME' created."

    # Execute all other .sql files
    find "$SCRIPT_DIR" -type f -name "*.sql" | sort | while read -r sql_file; do
        log_timing "Executing: $(basename $sql_file)"
        /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -d "$DB_NAME" -C -I -i "$sql_file"
        if [ $? -ne 0 ]; then
            log_timing "Error executing script: $sql_file"
            exit 1
        fi
    done
    log_timing "Finished database: $DB_NAME"
  fi
done

log_timing "Database initialization complete. SQL Server running."

# Wait for the SQL Server process to exit.
# This is important to keep the container running.
wait $!
