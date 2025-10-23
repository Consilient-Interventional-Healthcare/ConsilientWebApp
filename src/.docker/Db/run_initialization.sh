#!/bin/bash

echo "Waiting for SQL Server to start..."
until /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" &>/dev/null; do
  >&2 echo "SQL Server is unavailable - sleeping"
  sleep 1
done

# Find all .sql files, sort them, and execute them
for sql_file in $(find . -type f -name "*.sql" | sort); do
  # Extract the parent directory name to use as the database name
  db_name=$(basename "$(dirname "$sql_file")")
  
  echo "Executing script: $sql_file on database: $db_name"
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -d "$db_name" -i "$sql_file" -C

  # Exit if any script fails
  if [ $? -ne 0 ]; then
    echo "Error executing script: $sql_file"
    exit 1
  fi
done

echo "All scripts executed successfully."
