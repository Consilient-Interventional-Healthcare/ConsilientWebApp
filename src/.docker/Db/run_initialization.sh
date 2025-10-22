#!/bin/bash

echo "Waiting for SQL Server to start..."
until /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" &>/dev/null; do
  >&2 echo "SQL Server is unavailable - sleeping"
  sleep 1
done

echo "SQL Server is up - executing initialization script"
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -d master -i init.sql -C