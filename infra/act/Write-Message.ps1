function Write-Message {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Message,

        [Parameter(Mandatory)]
        [ValidateSet('Debug', 'Info', 'Step', 'Success', 'Warning', 'Error')]
        [string]$Level,

        [string]$Color,

        # New Parameter for "Mode"
        # This overrides the Global variable if provided
        [Parameter(Mandatory=$false)]
        [ValidateSet('Normal', 'Verbose')]
        [string]$LogLevel
    )

    # --- 1. CONFIGURATION (Weights & Colors) ---
    $levelWeights = @{
        'Debug'   = 1
        'Info'    = 2
        'Step'    = 3
        'Success' = 4
        'Warning' = 5
        'Error'   = 6
    }

    $levelColors = @{
        'Debug'   = 'DarkGray'
        'Info'    = 'Gray'
        'Step'    = 'Cyan'
        'Success' = 'Green'
        'Warning' = 'Yellow'
        'Error'   = 'Red'
    }

    # --- 2. DETERMINE THRESHOLD ---

    # Logic:
    # 1. If -LogLevel is passed, use that to determine the threshold level.
    # 2. If no -LogLevel, check $Global:CurrentLogLevel.
    # 3. Default to 'Info'.

    $thresholdLevel = 'Info' # Absolute default

    if ($PSBoundParameters.ContainsKey('LogLevel')) {
        # Map the convenient "LogLevel" names to your specific "Log Levels"
        $thresholdLevel = switch ($LogLevel) {
            'Verbose' { 'Debug' }  # Show Everything (1+)
            'Normal'  { 'Info' }   # Show Info and above (2+)
        }
    }
    elseif ($Global:CurrentLogLevel -and $levelWeights.ContainsKey($Global:CurrentLogLevel)) {
        $thresholdLevel = $Global:CurrentLogLevel
    }

    # --- 3. FILTERING ---
    if ($levelWeights[$Level] -lt $levelWeights[$thresholdLevel]) {
        return
    }

    # --- 4. OUTPUT (Color Logic) ---
    $effectiveColor = if ($PSBoundParameters.ContainsKey('Color')) { 
        $Color 
    } else { 
        $levelColors[$Level] 
    }

    Write-Host $Message -ForegroundColor $effectiveColor
}