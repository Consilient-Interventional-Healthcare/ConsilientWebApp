    # Recursively convert file-scoped namespaces (namespace X.Y.Z;) to block-scoped (namespace X.Y.Z { ... })
# Creates .bak backups for each modified file.
param(
    [string]$Root = ".\Consilient.Infrastructure.ExcelImporter"
)

$csFiles = Get-ChildItem -Path $Root -Recurse -Filter *.cs -File

foreach ($file in $csFiles) {
    $text = Get-Content -Raw -Encoding UTF8 $file.FullName

    # Skip if already block-scoped namespace anywhere (check with multiline mode)
    if ($text -match '(?m)^\s*namespace\s+[^{;]+\s*\{') {
        continue
    }

    # Find file-scoped namespace: namespace X.Y.Z;
    $nsRegex = '^\s*namespace\s+([^\s;{]+(?:\.[^\s;{]+)*)\s*;'
    $m = [regex]::Match($text, $nsRegex, [System.Text.RegularExpressions.RegexOptions]::Multiline)
    if (-not $m.Success) {
        continue
    }

    $namespace = $m.Groups[1].Value

    # Split lines (remove line endings, we'll add them back when joining)
    $lines = $text -split "\r\n|\r|\n"

    # Find index of the namespace line
    $nsLineIndex = -1
    for ($i = 0; $i -lt $lines.Length; $i++) {
        if ($lines[$i] -match $nsRegex) { $nsLineIndex = $i; break }
    }
    if ($nsLineIndex -lt 0) { continue }

    # Collect immediately-following using/extern/global using lines (they appear after file-scoped namespace)
    $usingLines = New-Object System.Collections.Generic.List[string]
    $usingEndIndex = $nsLineIndex + 1
    for ($i = $nsLineIndex + 1; $i -lt $lines.Length; $i++) {
        $line = $lines[$i]
        if ($line -match '^\s*(using\s|global\s+using\s|extern\s+)') {
            $usingLines.Add($line)
            $usingEndIndex = $i + 1
            continue
        }
        # stop at first non-using/blank/comment line
        break
    }

    # Build new lines:
    # - Keep any leading lines before namespace (file header comments / blank lines)
    # - Place moved using lines after header (if present)
    # - Insert block-scoped namespace and opening brace
    # - Append the remaining content (lines after using block)
    $before = if ($nsLineIndex -gt 0) { @($lines[0..($nsLineIndex-1)]) } else { @() }
    $after = if ($usingEndIndex -lt $lines.Length) { @($lines[$usingEndIndex..($lines.Length-1)]) } else { @() }

    $newLines = New-Object System.Collections.Generic.List[string]
    if ($before.Length -gt 0) {
        $newLines.AddRange([string[]]$before)
    }

    if ($usingLines.Count -gt 0) {
        # ensure a blank line between header and using block
        if ($newLines.Count -eq 0 -or -not [string]::IsNullOrWhiteSpace($newLines[$newLines.Count - 1])) {
            $newLines.Add('')
        }
        $newLines.AddRange([string[]]$usingLines)
        $newLines.Add('')
    }

    $newLines.Add("namespace $namespace")
    $newLines.Add("{")
    if ($after.Length -gt 0) {
        $newLines.AddRange([string[]]$after)
    }
    # Ensure final newline before closing brace
    if ($newLines.Count -gt 0 -and -not [string]::IsNullOrWhiteSpace($newLines[$newLines.Count - 1])) {
        $newLines.Add('')
    }
    $newLines.Add("}")

    $newText = ($newLines -join "`r`n")

    # Backup and write
    $bakPath = $file.FullName + ".bak"
    if (-not (Test-Path $bakPath)) {
        Copy-Item -Path $file.FullName -Destination $bakPath -Force
    }
    # Use Out-File to preserve exact encoding behavior
    [System.IO.File]::WriteAllText($file.FullName, $newText, [System.Text.UTF8Encoding]::new($true))
    Write-Host "Converted: $($file.FullName)"
}