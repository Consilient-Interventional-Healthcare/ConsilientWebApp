#Requires -Version 7.0

<#
.SYNOPSIS
    Installs Claude Desktop MCP configuration for Consilient Web App.

.DESCRIPTION
    Automatically detects repository path and installs Claude Desktop MCP configuration
    to allow Claude to access project documentation and source code.
    
    The template uses ${REPO_ROOT} placeholder which gets replaced with the actual
    repository path on each developer's machine.

.PARAMETER Force
    Overwrite existing Claude Desktop configuration without prompting.

.EXAMPLE
    .\Install-ClaudeConfig.ps1
    Install configuration with prompts

.EXAMPLE
    .\Install-ClaudeConfig.ps1 -Force
    Install configuration, overwriting existing config
#>

[CmdletBinding()]
param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Claude Desktop MCP Configuration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Detect repository root (this script is in .claude/)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir

Write-Host "Repository: " -NoNewline -ForegroundColor Gray
Write-Host $repoRoot -ForegroundColor White
Write-Host ""

# Determine Claude Desktop config location based on OS
if ($IsWindows -or $env:OS -match "Windows") {
    $claudeConfigPath = Join-Path $env:APPDATA "Claude\claude_desktop_config.json"
    $pathSeparator = "\\"
} elseif ($IsMacOS) {
    $claudeConfigPath = "$HOME/Library/Application Support/Claude/claude_desktop_config.json"
    $pathSeparator = "/"
} elseif ($IsLinux) {
    $claudeConfigPath = "$HOME/.config/Claude/claude_desktop_config.json"
    $pathSeparator = "/"
} else {
    throw "Unsupported operating system"
}

Write-Host "Claude config location:" -ForegroundColor Gray
Write-Host $claudeConfigPath -ForegroundColor White
Write-Host ""

# Check if Claude Desktop is installed
$claudeConfigDir = Split-Path -Parent $claudeConfigPath
if (-not (Test-Path $claudeConfigDir)) {
    Write-Host "Claude Desktop not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install Claude Desktop from:" -ForegroundColor Yellow
    Write-Host "  https://claude.ai/download" -ForegroundColor White
    Write-Host ""
    exit 1
}

# Read template from repository
$templatePath = Join-Path $scriptDir "claude_desktop_config.json"
if (-not (Test-Path $templatePath)) {
    throw "Template file not found: $templatePath"
}

Write-Host "Reading configuration template..." -ForegroundColor Yellow
$templateContent = Get-Content $templatePath -Raw

# Prepare repository path for JSON
if ($IsWindows -or $env:OS -match "Windows") {
    # Windows: escape backslashes for JSON
    $repoRootEscaped = $repoRoot.Replace('\', '\\')
} else {
    # Unix: use as-is
    $repoRootEscaped = $repoRoot
}

# Replace placeholder with actual repository path
Write-Host "Replacing ${REPO_ROOT} with repository path..." -ForegroundColor Yellow
$configContent = $templateContent.Replace('${REPO_ROOT}', $repoRootEscaped)

# On Windows, also replace forward slashes with escaped backslashes in paths
if ($IsWindows -or $env:OS -match "Windows") {
    # Handle mixed separators in template (if any)
    $configContent = $configContent.Replace('/', '\\')
}

# Validate JSON
try {
    $config = $configContent | ConvertFrom-Json
    Write-Host "Configuration validated successfully" -ForegroundColor Green
} catch {
    Write-Host "Invalid JSON generated!" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Yellow
    throw "Failed to generate valid configuration"
}

Write-Host ""

# Check if config already exists
$existingConfig = $null
if (Test-Path $claudeConfigPath) {
    Write-Host "Existing Claude configuration found" -ForegroundColor Yellow
    
    try {
        $existingConfig = Get-Content $claudeConfigPath -Raw | ConvertFrom-Json
        Write-Host "  Existing servers: " -NoNewline -ForegroundColor Gray
        Write-Host ($existingConfig.mcpServers.PSObject.Properties.Name -join ", ") -ForegroundColor White
    } catch {
        Write-Host "  Warning: Could not parse existing config" -ForegroundColor Yellow
    }
    
    Write-Host ""
    
    if (-not $Force) {
        $response = Read-Host "Overwrite existing configuration? (y/n)"
        if ($response -ne 'y') {
            Write-Host ""
            Write-Host "Installation cancelled" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "To merge manually:" -ForegroundColor Gray
            Write-Host "  1. Edit: $claudeConfigPath" -ForegroundColor White
            Write-Host "  2. Add server entries from this generated config" -ForegroundColor White
            Write-Host ""
            exit 0
        }
    }
}

# Ensure directory exists
if (-not (Test-Path $claudeConfigDir)) {
    New-Item -ItemType Directory -Path $claudeConfigDir -Force | Out-Null
}

# Write configuration
Write-Host "Installing MCP configuration..." -ForegroundColor Yellow
Set-Content -Path $claudeConfigPath -Value $configContent -Encoding UTF8
Write-Host "Configuration installed successfully!" -ForegroundColor Green
Write-Host ""

# Show what was configured
Write-Host "Configured MCP Servers:" -ForegroundColor Cyan
$config.mcpServers.PSObject.Properties | ForEach-Object {
    $serverName = $_.Name
    $serverPath = $_.Value.args[-1]  # Last argument is the path
    Write-Host "  • $serverName" -NoNewline -ForegroundColor Gray
    Write-Host " ? " -NoNewline -ForegroundColor DarkGray
    Write-Host $serverPath -ForegroundColor White
}
Write-Host ""

# Check Node.js installation
Write-Host "Checking Node.js installation..." -ForegroundColor Yellow
$nodeInstalled = Get-Command node -ErrorAction SilentlyContinue
if ($nodeInstalled) {
    $nodeVersion = node --version
    Write-Host "Node.js $nodeVersion found" -ForegroundColor Green
} else {
    Write-Host "Node.js not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "MCP requires Node.js. Install from:" -ForegroundColor Yellow
    Write-Host "  https://nodejs.org/" -ForegroundColor White
    Write-Host ""
}
Write-Host ""

# Next steps
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Installation Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Restart Claude Desktop (completely quit and reopen)" -ForegroundColor Gray
Write-Host "  2. Generate OpenAPI doc if not already done:" -ForegroundColor Gray
Write-Host "     pwsh Scripts\openapi-generation\Generate-OpenApiDoc.ps1" -ForegroundColor White
Write-Host "  3. Test by asking Claude:" -ForegroundColor Gray
Write-Host "     'Can you read docs/openapi.json?'" -ForegroundColor White
Write-Host ""
Write-Host "Configuration location: $claudeConfigPath" -ForegroundColor DarkGray
Write-Host "For troubleshooting, see: .claude/README.md" -ForegroundColor DarkGray
Write-Host ""
