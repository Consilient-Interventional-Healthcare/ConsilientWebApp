<#
.SYNOPSIS
    Retrieves log entries from Loki using LogCLI.

.DESCRIPTION
    Queries the Consilient Loki instance using LogCLI with credentials
    loaded from the dev environment secrets file.

.PARAMETER Query
    LogQL query string. Default: '{app=~".+"}'

.PARAMETER Since
    Time range to query (e.g., "1h", "30m", "24h"). Default: "1h"

.PARAMETER Limit
    Maximum number of log entries to return. Default: 100

.PARAMETER App
    Filter by app name (consilient-api, consilient-react). Optional.

.PARAMETER Level
    Filter by log level (INFO, WARN, ERROR, DEBUG). Optional.

.PARAMETER Contains
    Filter logs containing this text. Optional.

.EXAMPLE
    .\Get-LokiLogs.ps1
    # Gets last hour of all logs

.EXAMPLE
    .\Get-LokiLogs.ps1 -App "consilient-api" -Level "ERROR" -Since "24h"
    # Gets API errors from last 24 hours

.EXAMPLE
    .\Get-LokiLogs.ps1 -Query '{app="consilient-api"} |= "NullReferenceException"'
    # Custom LogQL query
#>

param(
    [string]$Query,
    [string]$Since = "1h",
    [int]$Limit = 100,
    [ValidateSet("consilient-api", "consilient-react")]
    [string]$App,
    [ValidateSet("INFO", "WARN", "ERROR", "DEBUG")]
    [string]$Level,
    [string]$Contains
)

$ErrorActionPreference = "Stop"

# Load environment variables
$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $ScriptRoot "..\common\Load-Environment.ps1")
Import-ConsilientEnvironment -Environment dev -Categories @('loki')

# Validate logcli is installed
if (-not (Get-Command logcli -ErrorAction SilentlyContinue)) {
    Write-Host "LogCLI not found. Install with:" -ForegroundColor Red
    Write-Host "  Windows: scoop install logcli" -ForegroundColor Yellow
    Write-Host "  macOS:   brew install logcli" -ForegroundColor Yellow
    exit 1
}

# Build query if not provided
if (-not $Query) {
    $labelFilters = @()
    if ($App) { $labelFilters += "app=\`"$App\`"" }
    if ($Level) { $labelFilters += "level=\`"$Level\`"" }

    if ($labelFilters.Count -gt 0) {
        $Query = "{" + ($labelFilters -join ", ") + "}"
    } else {
        $Query = '{app=~\".+\"}'
    }

    if ($Contains) {
        $Query += " |= \`"$Contains\`""
    }
}

Write-Host "Querying Loki..." -ForegroundColor Cyan
Write-Host "  Address: $env:LOKI_ADDR" -ForegroundColor DarkGray
Write-Host "  Query:   $Query" -ForegroundColor DarkGray
Write-Host "  Since:   $Since" -ForegroundColor DarkGray
Write-Host "  Limit:   $Limit" -ForegroundColor DarkGray
Write-Host ""

# Execute logcli query (uses LOKI_ADDR, LOKI_USERNAME, LOKI_PASSWORD env vars)
logcli query `
    --since="$Since" `
    --limit=$Limit `
    --output=jsonl `
    "$Query"
