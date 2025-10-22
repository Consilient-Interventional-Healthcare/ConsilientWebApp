#!/bin/bash

# Start the SQL Server process in the background
/opt/mssql/bin/sqlservr &

# Run the initialization script
./run_initialization.sh

# Wait for the SQL Server process to exit.
# This is important to keep the container running.
wait $!