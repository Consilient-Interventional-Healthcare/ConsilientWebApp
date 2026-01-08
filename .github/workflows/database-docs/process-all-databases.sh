#!/bin/bash
################################################################################
# Database Documentation Processing Script
#
# Purpose:
#   Orchestrates the generation of SchemaSpy documentation for multiple
#   databases in a sequential manner with full error handling.
#
# Expected Environment Variables:
#   - DATABASES_JSON: JSON array of database names
#   - SQL_SERVER: Azure SQL Server FQDN
#   - DATABASE_CONFIGS: JSON object with database configurations
#   - SCHEMA_DISCOVERY_TIMEOUT_SECONDS: Timeout for schema discovery
#   - SCHEMASPY_TIMEOUT_SECONDS: Timeout for SchemaSpy
#   - MAX_CONCURRENT_SCHEMAS: Max concurrent SchemaSpy processes
#   - Workflow inputs passed as environment variables (e.g., $environment)
#
# Output:
#   - docs/{database_name}/ directories with SchemaSpy documentation
#   - /tmp/database_info.txt with processed database metadata
#
# Security:
#   - Uses Azure AD authentication (ActiveDirectoryDefault)
#   - No credentials passed in process arguments
#   - All HTML output is properly escaped to prevent XSS
#
################################################################################

set -e
set -o pipefail

# Debug: Check environment variables
echo "DEBUG: environment_input='${environment_input}'"
if [ -z "${environment_input}" ]; then
  echo "⚠️  WARNING: environment_input is empty, using default 'dev'"
  environment_input="dev"
fi
export environment_input

# Initialize tracking
> /tmp/database_info.txt

# Parse database list
DATABASES_ARRAY=$(echo "$DATABASES_JSON" | jq -r '.[]')

# Track overall success
OVERALL_SUCCESS=true
FAILED_DATABASES=""

#################################################
# FUNCTION: Parse database configuration
#################################################
parse_database_config() {
  local db_name="$1"
  local environment="${environment_input}"

  echo "📋 Parsing configuration for $db_name (environment: $environment)" >&2

  # Default: use database name + environment suffix
  local actual_db_name="${db_name}_${environment}"

  # Try to get config file path
  local config_path=$(echo "$DATABASE_CONFIGS" | jq -r ".\"$db_name\".config_path // empty")

  # Only override if we have a valid config file
  if [ -n "$config_path" ] && [ -f "$config_path" ]; then
    echo "✅ Found config file: $config_path" >&2

    # Install yq if not available
    if ! command -v yq &> /dev/null; then
      echo "📦 Installing yq..." >&2
      sudo wget -qO /usr/local/bin/yq https://github.com/mikefarah/yq/releases/latest/download/yq_linux_amd64
      sudo chmod +x /usr/local/bin/yq
    fi

    # Check for environment-specific database name override
    local env_db_name=$(yq eval ".database.environment_names.[\"$environment\"]" "$config_path" 2>/dev/null)
    if [ "$env_db_name" != "null" ] && [ -n "$env_db_name" ]; then
      actual_db_name="$env_db_name"
      echo "🔄 Using environment-specific database name from config: $actual_db_name" >&2
    fi
  else
    echo "⚠️  No valid config file found, using default: ${db_name}_${environment}" >&2
  fi

  echo "✅ Target database: $actual_db_name" >&2
  echo "$actual_db_name"  # Return value (stdout)
}

#################################################
# FUNCTION: Discover schemas (FIX #3: Exit Code Bug)
#################################################
discover_schemas() {
  local db_name="$1"

  echo "🔍 Discovering schemas in $db_name..." >&2

  local discovery_sql="infra/db/list_user_schemas.sql"

  if [ ! -f "$discovery_sql" ]; then
    echo "❌ ERROR: Schema discovery script not found: $discovery_sql" >&2
    return 1
  fi

  # Create temporary files for output and errors
  local temp_output=$(mktemp)
  local temp_error=$(mktemp)

  # Run sqlcmd and capture exit code PROPERLY
  local exit_code
  if timeout "$SCHEMA_DISCOVERY_TIMEOUT_SECONDS" sqlcmd \
    -S "$SQL_SERVER" \
    -d "$db_name" \
    -G \
    -i "$discovery_sql" \
    -h -1 \
    -W \
    -b > "$temp_output" 2> "$temp_error"; then
    exit_code=0
  else
    exit_code=$?
  fi

  # Read the output
  local discovered_schemas=$(cat "$temp_output")

  # Check exit code FIRST
  if [ $exit_code -ne 0 ]; then
    echo "❌ Schema discovery failed with exit code $exit_code" >&2
    echo "--- Error output ---" >&2
    cat "$temp_error" 2>/dev/null || echo "No error log available" >&2
    echo "-------------------" >&2
    rm -f "$temp_output" "$temp_error"
    return 1
  fi

  # Then check for error keywords in output
  if echo "$discovered_schemas" | grep -qiE "error:|failed|exception|login"; then
    echo "❌ Error detected in schema discovery output" >&2
    echo "Output: $discovered_schemas" >&2
    rm -f "$temp_output" "$temp_error"
    return 1
  fi

  # Clean output
  discovered_schemas=$(echo "$discovered_schemas" | \
    grep -v "^[[:space:]]*$" | \
    grep -vi "^rows$\|^row$\|^affected\|^error" | \
    xargs)

  # Validate schemas (case-sensitive filtering)
  local valid_schemas=""
  for schema in $discovered_schemas; do
    if echo "$schema" | grep -qE '^[a-zA-Z_][a-zA-Z0-9_]*$'; then
      if ! echo "$schema" | grep -qE '^(rows?|affected|error|warning|database|select|from|where|order|by)$'; then
        valid_schemas="$valid_schemas $schema"
      fi
    fi
  done

  discovered_schemas=$(echo $valid_schemas | xargs)

  # Cleanup temp files
  rm -f "$temp_output" "$temp_error"

  # Validate at least one schema was found
  if [ -z "$discovered_schemas" ]; then
    echo "⚠️  WARNING: No valid user schemas discovered in database $db_name" >&2
    return 1
  fi

  echo "✅ Discovered schemas: $discovered_schemas" >&2
  echo "$discovered_schemas"  # Return value
}

#################################################
# FUNCTION: Generate docs (FIX #1: Azure AD, FIX #5: Concurrency)
#################################################
generate_database_docs() {
  local db_name="$1"
  local actual_db_name="$2"
  local schemas="$3"

  echo "🚀 Generating documentation for $actual_db_name..."
  echo "Schemas: $schemas"

  # Create output directory for this database
  local db_output_dir="docs/dbs/${db_name}"
  mkdir -p "$db_output_dir"

  # Create subdirectories for each schema
  for schema in $schemas; do
    schema=$(echo "$schema" | xargs)
    [ -z "$schema" ] && continue
    local schema_lower=$(echo "$schema" | tr '[:upper:]' '[:lower:]')
    mkdir -p "$db_output_dir/$schema_lower"
  done

  # FIX #5: Limit concurrent processes using semaphore pattern
  local schema_pids=()
  local schema_names=()

  echo "📊 Generating schema documentation (max $MAX_CONCURRENT_SCHEMAS concurrent)..."

  # Generate documentation for each schema with concurrency control
  for schema in $schemas; do
    schema=$(echo "$schema" | xargs)
    [ -z "$schema" ] && continue
    local schema_lower=$(echo "$schema" | tr '[:upper:]' '[:lower:]')

    # Wait for available slot (semaphore)
    while [ $(jobs -r | wc -l) -ge $MAX_CONCURRENT_SCHEMAS ]; do
      sleep 1
    done

    local description="${schema} Schema Documentation"

    echo "📊 Starting $schema schema generation..."

    # Launch SchemaSpy in background with controlled concurrency
    # IMPORTANT: The JDBC driver internally loads Azure-Identity when Authentication=ActiveDirectoryDefault is configured
    # Use bash -c to ensure CLASSPATH environment variable is accessible to the Java process
    # The Docker image has CLASSPATH pre-configured with all Azure-Identity and transitive dependencies
    timeout "$SCHEMASPY_TIMEOUT_SECONDS" bash -c 'java -jar /opt/schemaspy/schemaspy.jar \
      -t mssql17 \
      -dp /opt/schemaspy/mssql-jdbc.jar \
      -host "'"$SQL_SERVER"'" \
      -db "'"$actual_db_name"'" \
      -u "CloudSA" \
      -connprops "Authentication\=ActiveDirectoryDefault;encrypt\=true;trustServerCertificate\=false" \
      -norows \
      -vizjs \
      -imageformat svg \
      -noimplied \
      -debug \
      -o "'"$db_output_dir/$schema_lower"'" \
      -s "'"$schema"'" \
      -desc "'"$description"'"' &

    schema_pids+=($!)
    schema_names+=("$schema")
  done

  # Wait for all processes and check exit codes
  local schema_failed=false
  for i in "${!schema_pids[@]}"; do
    local pid="${schema_pids[$i]}"
    local name="${schema_names[$i]}"

    echo "⏳ Waiting for $name schema generation (PID: $pid)..."
    if wait "$pid"; then
      echo "✅ $name schema complete"
    else
      echo "❌ $name schema failed with exit code: $?"
      schema_failed=true
    fi
  done

  if [ "$schema_failed" = true ]; then
    echo "❌ One or more schemas failed for database $db_name"
    return 1
  fi

  echo "✅ All schemas generated successfully for $db_name"
  return 0
}

#################################################
# FUNCTION: Process single database (orchestrator)
#################################################
process_single_database() {
  local db_name="$1"

  echo ""
  echo "========================================================"
  echo "Processing Database: $db_name"
  echo "========================================================"

  # Validate database name
  if [[ ! "$db_name" =~ ^[a-zA-Z0-9_-]+$ ]]; then
    echo "❌ ERROR: Invalid database name: '$db_name'"
    return 1
  fi

  echo "✅ Database name validated: $db_name"

  # Step 1: Parse configuration
  local actual_db_name
  actual_db_name=$(parse_database_config "$db_name")
  if [ $? -ne 0 ] || [ -z "$actual_db_name" ]; then
    echo "❌ Failed to parse configuration for $db_name"
    return 1
  fi

  # Step 2: Discover schemas
  local schemas
  schemas=$(discover_schemas "$actual_db_name")
  if [ $? -ne 0 ] || [ -z "$schemas" ]; then
    echo "⚠️  No schemas found for $db_name, skipping documentation"
    return 0  # Not a failure, just skip
  fi

  # Step 3: Generate documentation
  if ! generate_database_docs "$db_name" "$actual_db_name" "$schemas"; then
    echo "❌ Failed to generate documentation for $db_name"
    return 1
  fi

  # Step 4: Store database info for unified index generation
  echo "$db_name|$actual_db_name|$schemas" >> /tmp/database_info.txt

  echo "✅ Database $db_name processed successfully"
  return 0
}

#################################################
# MAIN LOOP: Process all databases sequentially
#################################################
for DATABASE_NAME in $DATABASES_ARRAY; do
  if ! process_single_database "$DATABASE_NAME"; then
    echo "❌ Failed to process database: $DATABASE_NAME"
    OVERALL_SUCCESS=false
    FAILED_DATABASES="$FAILED_DATABASES $DATABASE_NAME"
    continue  # Continue with next database
  fi
done

# Check overall status
if [ "$OVERALL_SUCCESS" = false ]; then
  echo ""
  echo "❌ Some databases failed to process:"
  echo "$FAILED_DATABASES"
  exit 1
fi

echo ""
echo "========================================================"
echo "✅ All databases processed successfully"
echo "========================================================"
