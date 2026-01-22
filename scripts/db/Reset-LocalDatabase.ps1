param(
    [string]$Database = "consilient_main",
    [switch]$Force
)

if (-not $Force) {
    $confirm = Read-Host "This will DELETE all data in $Database. Continue? (y/N)"
    if ($confirm -ne 'y') {
        Write-Host "Cancelled."
        exit 0
    }
}

$composeFile = Join-Path $PSScriptRoot "../../src/.docker/docker-compose.yml"

# Rebuild the database image to ensure latest SQL scripts are included
Write-Host "Rebuilding database image with latest scripts..."
docker compose -f $composeFile build --no-cache db

# Remove existing container to avoid naming conflicts
Write-Host "Removing existing database container..."
docker rm -f consilient.dbs.container 2>$null

# Ensure the container is running first
Write-Host "Ensuring database container is running..."
docker compose -f $composeFile up -d db
Start-Sleep -Seconds 2

# Create marker file on the persistent volume (survives container recreation)
Write-Host "Creating reset marker for $Database..."
docker compose -f $composeFile exec -T db bash -c "touch /var/opt/mssql/.reset-$Database"

# Restart the db service to trigger entrypoint with the marker
Write-Host "Restarting database container..."
docker compose -f $composeFile up -d --force-recreate db

# Stream logs in real-time while waiting for scripts to complete
Write-Host "`nStreaming database initialization logs..." -ForegroundColor Cyan
Write-Host "(Press Ctrl+C to stop streaming, database will continue in background)`n" -ForegroundColor DarkGray

# Start log streaming in background job
$logJob = Start-Job -ScriptBlock {
    param($composeFile)
    docker compose -f $composeFile logs -f db 2>&1
} -ArgumentList $composeFile

# Wait for "All database scripts executed successfully" or timeout
$timeout = 120
$startTime = Get-Date
$completed = $false

while (-not $completed -and ((Get-Date) - $startTime).TotalSeconds -lt $timeout) {
    # Check for new log output
    $output = Receive-Job -Job $logJob -ErrorAction SilentlyContinue
    if ($output) {
        $output | ForEach-Object {
            $line = $_
            # Color-code the output
            if ($line -match "Error|Msg \d+") {
                Write-Host $line -ForegroundColor Red
            } elseif ($line -match "Executing script") {
                Write-Host $line -ForegroundColor Yellow
            } elseif ($line -match "created|dropped|SUCCESS") {
                Write-Host $line -ForegroundColor Green
            } elseif ($line -match "Processing database|Finished processing") {
                Write-Host $line -ForegroundColor Cyan
            } else {
                Write-Host $line
            }

            # Check if scripts completed
            if ($line -match "All database scripts executed successfully") {
                $completed = $true
            }
        }
    }
    Start-Sleep -Milliseconds 200
}

Stop-Job -Job $logJob -ErrorAction SilentlyContinue
Remove-Job -Job $logJob -Force -ErrorAction SilentlyContinue

if (-not $completed) {
    Write-Host "`nTimeout waiting for scripts. Checking final status..." -ForegroundColor Yellow
}

# Show any remaining relevant logs
Write-Host "`nFinal initialization summary:" -ForegroundColor Cyan
docker compose -f $composeFile logs --tail=50 db 2>&1 | Select-String -Pattern "(Executing script|Error|Msg \d+|dropped|Created|does not exist|already exists|Processing database|Finished processing|rows affected)"

# Check if tables were created successfully
Write-Host "`nVerifying table creation..." -ForegroundColor Cyan
$tableCheck = docker exec consilient.dbs.container bash -c "echo 'SELECT COUNT(*) FROM $Database.INFORMATION_SCHEMA.TABLES' | /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P `$SA_PASSWORD -C -h -1" 2>$null
if ($tableCheck -match '^\s*(\d+)') {
    $tableCount = [int]$Matches[1]
    if ($tableCount -lt 10) {
        Write-Host "WARNING: Only $tableCount tables created. Check logs above for SQL errors." -ForegroundColor Yellow
    } else {
        Write-Host "SUCCESS: $tableCount tables created in $Database." -ForegroundColor Green
    }
} else {
    Write-Host "WARNING: Could not verify table count." -ForegroundColor Yellow
}

Write-Host "`nDatabase $Database has been reset." -ForegroundColor Green
