<#
.SYNOPSIS
    Shared configuration for act local testing scripts.

.DESCRIPTION
    This file contains centralized configuration used by multiple act-related scripts:
    - Build-RunnerImage.ps1
    - Initialize-ActCache.ps1
    - Invoke-ActWorkflow.ps1

    By centralizing configuration here, we maintain a single source of truth and avoid
    duplication. Scripts dot-source this file to access the configuration hashtables.

.NOTES
    All paths are relative to the repository root.
#>

# Docker image configuration for the custom GitHub Actions runner
# Used by: Build-RunnerImage.ps1, Invoke-ActWorkflow.ps1
$Script:ActDockerConfig = @{
    LocalImageName = "consilientwebapp-runner"
    LocalImageTag  = "latest"
}

# Workflow file configuration
# Used by: Invoke-ActWorkflow.ps1
$Script:ActWorkflowConfig = @{
    WorkflowFile = ".github\workflows\main.yml"
}
