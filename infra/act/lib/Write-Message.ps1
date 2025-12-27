<#
.SYNOPSIS
    Unified output system with proper level-based filtering.

.DESCRIPTION
    Provides consistent, level-based message output across all act scripts.
    Respects $Quiet, $Info, and $VerbosePreference flags for output filtering.

.PARAMETER Message
    The message text to display.

.PARAMETER Level
    The message level: Debug, Info, Warning, Error, Success, Step

.PARAMETER Color
    Optional color override. If not specified, defaults per level are used.

.EXAMPLE
    Write-Message -Level Info -Message "Processing deployment..."
    Write-Message -Level Error -Message "Failed to connect" -Color Red
    Write-Message -Level Success -Message "Deployment complete"
#>

function Write-Message {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Message,

        [Parameter(Mandatory)]
        [ValidateSet('Debug', 'Info', 'Warning', 'Error', 'Success', 'Step')]
        [string]$Level,

        [string]$Color
    )

    # Level-based visibility logic
    # - Debug: Only in -Verbose mode (for diagnostic details)
    # - Info: Hidden in -Info and -Quiet modes, unless -Verbose is explicit
    # - Warning: Always shown
    # - Error: Always shown
    # - Success: Only in normal mode, hidden in -Info and -Quiet modes
    # - Step: Only in normal mode, hidden in -Info mode
    $shouldShow = switch ($Level) {
        'Debug'   { $VerbosePreference -eq 'Continue' }
        'Info'    { -not $Quiet -and (-not $Info -or $VerbosePreference -eq 'Continue') }
        'Warning' { $true }
        'Error'   { $true }
        'Success' { -not $Info -and -not $Quiet }
        'Step'    { -not $Info }
    }

    if (-not $shouldShow) { return }

    # Default colors per level
    $effectiveColor = if ($Color) {
        $Color
    }
    else {
        @{
            'Debug'   = 'DarkGray'
            'Info'    = 'Gray'
            'Warning' = 'Yellow'
            'Error'   = 'Red'
            'Success' = 'Green'
            'Step'    = 'Cyan'
        }[$Level]
    }

    Write-Host $Message -ForegroundColor $effectiveColor
}
