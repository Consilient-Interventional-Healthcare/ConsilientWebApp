# Choose ubuntu version
FROM mcr.microsoft.com/mssql/server:2022-latest

# Set the working directory
WORKDIR /app

# Copy the entrypoint script first
COPY --chmod=755 Db/entrypoint.sh .

# Explicitly copy the database script directories that match the command arguments
COPY Db/consilient_hangfire/ ./consilient_hangfire/
COPY Db/consilient_main/ ./consilient_main/

# Set the entrypoint to be the script
ENTRYPOINT ["/bin/bash", "./entrypoint.sh"]