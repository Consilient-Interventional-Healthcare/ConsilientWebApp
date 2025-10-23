# Choose ubuntu version
FROM mcr.microsoft.com/mssql/server:2022-latest

# Set the working directory
WORKDIR /app

# Copy all contents from the 'Db' subdirectory into the container's working directory
COPY --chmod=755 Db/run_initialization.sh .
COPY --chmod=755 Db/entrypoint.sh .
COPY Db/consilient_hangfire/ ./consilient_hangfire/
COPY Db/consilient_dev/ ./consilient_dev/

# Set the entrypoint to be the script
ENTRYPOINT ["/bin/bash", "./entrypoint.sh"]