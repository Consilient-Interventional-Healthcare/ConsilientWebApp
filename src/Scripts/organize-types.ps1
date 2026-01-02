#Requires -Version 7.0

<#
.SYNOPSIS
    Organizes generated TypeScript interfaces into module namespaces based on OpenAPI tags.

.DESCRIPTION
    Post-processes NSwag-generated TypeScript to organize interfaces into namespaces
    based on controller information from the OpenAPI spec (via tags/operations).
    
    This is a generic, reusable script that works with any .NET API project.
    It eliminates hardcoded module mappings by extracting controller context
    directly from the OpenAPI specification.

.PARAMETER InputFile
    Path to the generated TypeScript file. Can be absolute or relative.

.PARAMETER SwaggerFile
    Path to the OpenAPI/Swagger JSON file. Default: swagger.json (in current directory)

.PARAMETER DefaultNamespace
    Default namespace for unmatched types. Default: Common

.PARAMETER FallbackPatterns
    Hashtable of regex patterns for fallback namespace detection.
    Format: @{ 'NamespaceName' = @('Pattern1', 'Pattern2') }
    If not specified, uses generic patterns based on CRUD prefixes.

.EXAMPLE
    .\organize-types.ps1 -InputFile "output/api.generated.ts"

.EXAMPLE
    .\organize-types.ps1 -InputFile "api.ts" -SwaggerFile "openapi.json" -Verbose

.EXAMPLE
    # Custom fallback patterns
    $patterns = @{
        'Products' = @('Product', 'Catalog')
        'Orders' = @('Order', 'Purchase')
    }
    .\organize-types.ps1 -InputFile "api.ts" -FallbackPatterns $patterns
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$InputFile = "api.generated.ts",
    
    [string]$SwaggerFile = "swagger.json",
    [string]$DefaultNamespace = "Common",
    [hashtable]$FallbackPatterns = $null
)

function Get-ControllerTagsFromSwagger {
    param([string]$SwaggerPath)
    
    if (-not (Test-Path $SwaggerPath)) {
        Write-Warning "OpenAPI spec file not found: $SwaggerPath"
        Write-Warning "Using fallback: interface name pattern matching"
        return $null
    }
    
    try {
        $swagger = Get-Content $SwaggerPath -Raw | ConvertFrom-Json
        
        # Extract tags (usually controller names)
        $tags = @{}
        if ($swagger.tags) {
            foreach ($tag in $swagger.tags) {
                $tags[$tag.name] = @{
                    Name = $tag.name
                    Description = $tag.description
                    Types = @()
                }
            }
        }
        
        # Extract schemas and their associations with tags from paths
        if ($swagger.paths) {
            foreach ($path in $swagger.paths.PSObject.Properties) {
                foreach ($method in $path.Value.PSObject.Properties) {
                    $operation = $method.Value
                    
                    if ($operation.tags -and $operation.tags.Count -gt 0) {
                        $tag = $operation.tags[0]
                        
                        # Initialize tag if not already present
                        if (-not $tags.ContainsKey($tag)) {
                            $tags[$tag] = @{
                                Name = $tag
                                Description = $null
                                Types = @()
                            }
                        }
                        
                        # Extract request body schemas
                        if ($operation.requestBody -and $operation.requestBody.content) {
                            foreach ($content in $operation.requestBody.content.PSObject.Properties) {
                                if ($content.Value.schema.'$ref') {
                                    $schemaName = $content.Value.schema.'$ref' -replace '#/components/schemas/', ''
                                    $tags[$tag].Types += $schemaName
                                }
                            }
                        }
                        
                        # Extract response schemas
                        if ($operation.responses) {
                            foreach ($response in $operation.responses.PSObject.Properties) {
                                if ($response.Value.content) {
                                    foreach ($content in $response.Value.content.PSObject.Properties) {
                                        if ($content.Value.schema.'$ref') {
                                            $schemaName = $content.Value.schema.'$ref' -replace '#/components/schemas/', ''
                                            $tags[$tag].Types += $schemaName
                                        }
                                        elseif ($content.Value.schema.type -eq 'array' -and $content.Value.schema.items.'$ref') {
                                            $schemaName = $content.Value.schema.items.'$ref' -replace '#/components/schemas/', ''
                                            $tags[$tag].Types += $schemaName
                                        }
                                    }
                                }
                            }
                        }
                        
                        # Extract parameter schemas
                        if ($operation.parameters) {
                            foreach ($param in $operation.parameters) {
                                if ($param.schema.'$ref') {
                                    $schemaName = $param.schema.'$ref' -replace '#/components/schemas/', ''
                                    $tags[$tag].Types += $schemaName
                                }
                            }
                        }
                    }
                }
            }
        }
        
        # Remove duplicates
        foreach ($tag in $tags.Keys) {
            $tags[$tag].Types = $tags[$tag].Types | Select-Object -Unique
        }
        
        Write-Verbose "Extracted $($tags.Count) tags from OpenAPI spec:"
        foreach ($tagKey in $tags.Keys) {
            $tagInfo = $tags[$tagKey]
            Write-Verbose "  ${tagKey}: $($tagInfo.Types.Count) types"
        }
        
        return $tags
    }
    catch {
        Write-Warning "Failed to parse OpenAPI spec file: $_"
        Write-Warning "Using fallback: interface name pattern matching"
        return $null
    }
}

function Get-DefaultFallbackPatterns {
    # Generic patterns based on common CRUD naming conventions
    return @{
        # GraphQL types
        'GraphQl' = @('^GraphQl', '^Query')
        
        # Common utility types
        'Common' = @('^FileParameter$', '^PaginationInfo$', '^ApiError$', '^ValidationError$')
        
        # Generic CRUD patterns - will match if no specific pattern matches
        # These are intentionally broad to catch common naming conventions
    }
}

function Get-InterfaceToNamespaceMapping {
    param(
        [hashtable]$ControllerTags,
        [array]$AllInterfaces,
        [hashtable]$CustomFallbackPatterns
    )
    
    $mapping = @{}
    
    # First priority: Map based on OpenAPI spec
    if ($ControllerTags) {
        foreach ($tag in $ControllerTags.Keys) {
            foreach ($typeName in $ControllerTags[$tag].Types) {
                $mapping[$typeName] = $tag
            }
        }
    }
    
    # Second priority: Custom fallback patterns
    $fallbackPatterns = if ($CustomFallbackPatterns) {
        $CustomFallbackPatterns
    } else {
        Get-DefaultFallbackPatterns
    }
    
    # Third priority: Infer from naming conventions
    foreach ($interfaceName in $AllInterfaces) {
        if (-not $mapping.ContainsKey($interfaceName)) {
            $namespace = $DefaultNamespace
            $matched = $false
            
            # Try custom/default patterns first
            foreach ($patternNamespace in $fallbackPatterns.Keys) {
                foreach ($pattern in $fallbackPatterns[$patternNamespace]) {
                    if ($interfaceName -match $pattern) {
                        $namespace = $patternNamespace
                        $matched = $true
                        break
                    }
                }
                if ($matched) { break }
            }
            
            # If still not matched, try to infer from CRUD prefix + pluralization
            if (-not $matched -and $interfaceName -match '^(Create|Update|Delete|Get|List)(\w+)(Request|Response|Dto)?$') {
                $entityName = $matches[2]
                # Pluralize by adding 's' - simple heuristic
                $namespace = if ($entityName -match 'y$') {
                    $entityName -replace 'y$', 'ies'
                } elseif ($entityName -match 's$') {
                    $entityName
                } else {
                    "${entityName}s"
                }
                Write-Verbose "Inferred namespace '$namespace' for interface '$interfaceName'"
            }
            
            $mapping[$interfaceName] = $namespace
        }
    }
    
    return $mapping
}

function Organize-TypeScriptInterfaces {
    param(
        [string]$Content,
        [hashtable]$InterfaceMapping
    )
    
    # Remove existing namespace wrapper if present (from any previous run)
    $Content = $Content -replace 'namespace\s+[\w\.]+\s*\{\s*', ''
    $Content = $Content -replace '\s*\}\s*$', ''
    
    # Extract all interfaces and enums using a more robust approach
    # We'll parse manually to handle nested braces correctly
    $types = @()
    $lines = $Content -split "`n"
    $i = 0
    
    while ($i -lt $lines.Count) {
        $line = $lines[$i]
        
        # Look for interface or enum declaration
        if ($line -match '^\s*export\s+(interface|enum)\s+(\w+)(?:\s+extends\s+\w+)?\s*\{') {
            $typeKind = $matches[1]
            $typeName = $matches[2]
            $typeCode = $line + "`n"
            $braceCount = 1
            $i++
            
            # Collect lines until braces are balanced
            while ($i -lt $lines.Count -and $braceCount -gt 0) {
                $currentLine = $lines[$i]
                $typeCode += $currentLine + "`n"
                
                # Count braces
                $openBraces = ($currentLine.ToCharArray() | Where-Object { $_ -eq '{' }).Count
                $closeBraces = ($currentLine.ToCharArray() | Where-Object { $_ -eq '}' }).Count
                $braceCount += $openBraces - $closeBraces
                
                $i++
            }
            
            # Add the extracted type
            $types += @{
                Kind = $typeKind
                Name = $typeName
                Code = $typeCode.TrimEnd()
            }
        }
        else {
            $i++
        }
    }
    
    # --- New: expand mapping for referenced types so enums/deps move with their users ---
    # Collect all type names present in the generated content
    $allTypeNames = $types | ForEach-Object { $_.Name }
    # For each type, scan its code for capitalized symbol references and map referenced types to same namespace if mapping exists
    foreach ($type in $types) {
        # find candidate referenced type names (simple heuristic: capitalized identifiers)
        $refMatches = [regex]::Matches($type.Code, '\b([A-Z][A-Za-z0-9_]+)\b') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
        foreach ($ref in $refMatches) {
            if ($allTypeNames -contains $ref) {
                # if ref already has explicit mapping, keep it; otherwise assign it to the current type's namespace
                if (-not $InterfaceMapping.ContainsKey($ref) -or $InterfaceMapping[$ref] -eq $DefaultNamespace) {
                    if ($InterfaceMapping.ContainsKey($type.Name)) {
                        $InterfaceMapping[$ref] = $InterfaceMapping[$type.Name]
                    } else {
                        $InterfaceMapping[$ref] = $DefaultNamespace
                    }
                }
            }
        }
    }
    # --- End new mapping expansion ---
    
    # --- New: expand mapping for referenced types so enums/deps move with their users ---
    # Collect all type names present in the generated content
    $allTypeNames = $types | ForEach-Object { $_.Name }
    # For each type, scan its code for capitalized symbol references and map referenced types to same namespace if mapping exists
    foreach ($type in $types) {
        # find candidate referenced type names (simple heuristic: capitalized identifiers)
        $refMatches = [regex]::Matches($type.Code, '\b([A-Z][A-Za-z0-9_]+)\b') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
        foreach ($ref in $refMatches) {
            if ($allTypeNames -contains $ref) {
                # if ref already has explicit mapping, keep it; otherwise assign it to the current type's namespace
                if (-not $InterfaceMapping.ContainsKey($ref) -or $InterfaceMapping[$ref] -eq $DefaultNamespace) {
                    if ($InterfaceMapping.ContainsKey($type.Name)) {
                        $InterfaceMapping[$ref] = $InterfaceMapping[$type.Name]
                    } else {
                        $InterfaceMapping[$ref] = $DefaultNamespace
                    }
                }
            }
        }
    }
    # --- End new mapping expansion ---
    
    # Group by namespace
    $namespaceGroups = @{
    }
    
    foreach ($type in $types) {
        $namespace = if ($InterfaceMapping.ContainsKey($type.Name)) {
            $InterfaceMapping[$type.Name]
        } else {
            $DefaultNamespace
        }
        
        if (-not $namespaceGroups.ContainsKey($namespace)) {
            $namespaceGroups[$namespace] = @()
        }
        
        $namespaceGroups[$namespace] += $type
    }
    
    # Build new content
    $header = @"
//----------------------
// <auto-generated>
//     Generated using NSwag toolchain (http://NSwag.org)
//     Post-processed to organize into module namespaces based on OpenAPI tags
//
//     This file was automatically generated. Do not modify manually.
// </auto-generated>
//----------------------

/* tslint:disable */
/* eslint-disable */
// ReSharper disable InconsistentNaming

"@
    
    $newContent = $header
    
    foreach ($namespace in ($namespaceGroups.Keys | Sort-Object)) {
        $newContent += "`n`nexport namespace $namespace {`n"
        
        foreach ($type in $namespaceGroups[$namespace]) {
            # Indent the code properly
            $lines = $type.Code -split "`n"
            $indentedLines = @()
            foreach ($line in $lines) {
                if ($line -match '^\s*export') {
                    $indentedLines += "  $line"
                }
                elseif ($line.Trim() -ne '') {
                    $indentedLines += "  $line"
                }
            }
            $newContent += "`n" + ($indentedLines -join "`n") + "`n"
        }
        
        $newContent += "`n}`n"
    }
    
    return @{
        Content = $newContent
        Namespaces = $namespaceGroups
    }
}

# Main execution
try {
    Write-Host "Organizing TypeScript types into module namespaces..." -ForegroundColor Yellow
    Write-Host ""
    
    # Resolve input file path
    if (-not [System.IO.Path]::IsPathRooted($InputFile)) {
        $InputFile = Join-Path (Get-Location) $InputFile
    }
    
    if (-not (Test-Path $InputFile)) {
        throw "Input file not found: $InputFile"
    }
    
    Write-Verbose "Input file: $InputFile"
    
    # Read input file
    $content = Get-Content $InputFile -Raw
    
    # Get controller tags from OpenAPI spec
    Write-Verbose "Reading OpenAPI spec from: $SwaggerFile"
    $controllerTags = Get-ControllerTagsFromSwagger -SwaggerPath $SwaggerFile
    
    # Extract type names
    $typePattern = 'export (interface|enum) (\w+)'
    $allTypes = [regex]::Matches($content, $typePattern) | ForEach-Object { $_.Groups[2].Value }
    
    Write-Verbose "Found $($allTypes.Count) types to organize"
    
    # Build type to namespace mapping
    $mapping = Get-InterfaceToNamespaceMapping `
        -ControllerTags $controllerTags `
        -AllInterfaces $allTypes `
        -CustomFallbackPatterns $FallbackPatterns
    
    # Organize content
    $result = Organize-TypeScriptInterfaces -Content $content -InterfaceMapping $mapping
    
    # Write back
    $result.Content | Set-Content $InputFile -NoNewline
    
    Write-Host "? Types organized into module namespaces!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Available namespaces:" -ForegroundColor Cyan
    foreach ($namespace in ($result.Namespaces.Keys | Sort-Object)) {
        $count = $result.Namespaces[$namespace].Count
        Write-Host "  • $namespace ($count types)" -ForegroundColor White
        
        if ($VerbosePreference -eq 'Continue') {
            foreach ($type in $result.Namespaces[$namespace]) {
                Write-Host "    - $($type.Name) ($($type.Kind))" -ForegroundColor DarkGray
            }
        }
    }
    
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "? Error: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($VerbosePreference -eq 'Continue') {
        Write-Host ""
        Write-Host "Stack trace:" -ForegroundColor Gray
        Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    }
    
    exit 1
}
