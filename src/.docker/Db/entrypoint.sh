#!/bin/bash

# Start the SQL Server process in the background
/opt/mssql/bin/sqlservr &

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to start..."
until /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" &>/dev/null; do
  >&2 echo "SQL Server is unavailable - sleeping"
  sleep 1
done
echo "SQL Server is up - executing scripts"


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

# --- Script Execution Loop ---
for DB_NAME in "${DATABASES[@]}"; do
  echo "--- Processing database: $DB_NAME ---"
  SCRIPT_DIR="./$DB_NAME"

  if [ ! -d "$SCRIPT_DIR" ]; then
    echo "Warning: Db directory not found for database '$DB_NAME'. Skipping."
    continue
  fi

    # Check if the database already exists before trying to create it
  if /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -h -1 -Q "SET NOCOUNT ON; SELECT 1 FROM sys.databases WHERE name = N'$DB_NAME'" | grep -q 1; then
    echo "Database '$DB_NAME' already exists. Skipping creation."
  else
    echo "Database '$DB_NAME' does not exist. Creating..."
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -Q "CREATE DATABASE [$DB_NAME]"
    echo "Database '$DB_NAME' created."

    # Execute all other .sql files
    find "$SCRIPT_DIR" -type f -name "*.sql" | sort | while read -r sql_file; do
        echo "Executing script: $sql_file on database: $DB_NAME"
        /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -d "$DB_NAME" -C -i "$sql_file"
        if [ $? -ne 0 ]; then
            echo "Error executing script: $sql_file"
            exit 1
        fi
    done
    echo "--- Finished processing database: $DB_NAME ---"
    echo "All database scripts executed successfully."
  fi
done

# Wait for the SQL Server process to exit.
# This is important to keep the container running.
wait $!
