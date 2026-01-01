#!/bin/bash
#
# discover-databases.sh
#
# Discovers database directories in a given path and outputs structured data
# for downstream workflow consumption.
#
# Usage: discover-databases.sh
#
# Environment variables:
#   SCRIPTS_PATH  - Root path to scan for database directories
#   GITHUB_OUTPUT - GitHub Actions output file (set automatically)
#

set -e
set -o pipefail

#------------------------------------------------------------------------------
# LOGGING FUNCTIONS
#------------------------------------------------------------------------------

log_info() {
  echo "??  $*"
}

log_success() {
  echo "? $*"
}

log_warning() {
  echo "??  $*"
}

log_error() {
  echo "? ERROR: $*" >&2
}

log_section() {
  echo ""
  echo "$*"
}

#------------------------------------------------------------------------------
# VALIDATION FUNCTIONS
#------------------------------------------------------------------------------

validate_inputs() {
  log_section "?? Database Discovery Configuration"
  log_info "Scripts path: ${SCRIPTS_PATH}"

  if [ -z "$SCRIPTS_PATH" ]; then
    log_error "SCRIPTS_PATH environment variable is not set"
    exit 1
  fi
}

validate_tools() {
  if ! command -v jq &> /dev/null; then
    log_error "jq is required but not installed"
    exit 1
  fi
}

validate_path() {
  if [ ! -d "$SCRIPTS_PATH" ]; then
    log_warning "Scripts path not found: $SCRIPTS_PATH"
    log_info "Available paths in repository root:"
    ls -la . | grep -E "^d" | awk '{print "   - " $NF}' || true
    log_info "Proceeding with empty database discovery"
    return 1
  fi

  log_success "Scripts path validated"
  return 0
}

#------------------------------------------------------------------------------
# OUTPUT FUNCTIONS
#------------------------------------------------------------------------------

write_empty_outputs() {
  echo "databases=[]" >> "$GITHUB_OUTPUT"
  echo "database_directories=[]" >> "$GITHUB_OUTPUT"
  echo "database_configs={}" >> "$GITHUB_OUTPUT"
  echo "count=0" >> "$GITHUB_OUTPUT"
}

print_outputs() {
  local databases="$1"
  local count="$2"

  log_section "?? Output Formats (for comparison):"
  echo ""
  echo "1??  databases (full metadata array):"
  echo "$databases" | jq -c .
  echo ""
  echo "2??  database_directories (simple array of names):"
  echo "$databases" | jq -c '[.[] | .directory]'
  echo ""
  echo "3??  database_configs (object mapping names to config):"

  # Build configs object
  local configs="{}"
  while IFS= read -r db_entry; do
    [ -z "$db_entry" ] && continue

    local db_name config_data
    db_name=$(echo "$db_entry" | jq -r '.name')
    config_data=$(echo "$db_entry" | jq -c '{directory: .directory, config_path: .config_path}')
    configs=$(echo "$configs" | jq -c --argjson data "$config_data" --arg name "$db_name" '.[$name] = $data')
  done < <(echo "$databases" | jq -c '.[]')

  echo "$configs" | jq -c .
  echo ""
  echo "4??  count (number of databases):"
  echo "$count"
  echo ""
}

write_outputs() {
  local databases="$1"
  local count="$2"

  log_section "?? Writing outputs to GITHUB_OUTPUT"

  # Output 1: Full database array
  echo "databases=$(echo "$databases" | jq -c .)" >> "$GITHUB_OUTPUT"

  # Output 2: Array of directory names
  local directories
  directories=$(echo "$databases" | jq -c '[.[] | .directory]')
  echo "database_directories=$directories" >> "$GITHUB_OUTPUT"

  # Output 3: Object mapping names to config data
  local configs="{}"
  while IFS= read -r db_entry; do
    [ -z "$db_entry" ] && continue

    local db_name config_data
    db_name=$(echo "$db_entry" | jq -r '.name')
    config_data=$(echo "$db_entry" | jq -c '{directory: .directory, config_path: .config_path}')
    configs=$(echo "$configs" | jq -c --argjson data "$config_data" --arg name "$db_name" '.[$name] = $data')
  done < <(echo "$databases" | jq -c '.[]')

  echo "database_configs=$configs" >> "$GITHUB_OUTPUT"

  # Output 4: Count
  echo "count=$count" >> "$GITHUB_OUTPUT"

  log_success "All outputs written to GITHUB_OUTPUT"
}

#------------------------------------------------------------------------------
# DISCOVERY FUNCTIONS
#------------------------------------------------------------------------------

discover_directories() {
  local scripts_path="$1"

  log_section "?? Scanning for database directories in: $scripts_path"

  # Change to scripts path for relative path discovery
  cd "$scripts_path" || return 1

  local all_dirs
  if ! all_dirs=$(find . -mindepth 1 -maxdepth 1 -type d 2>&1 | sed 's|^\./||' | sort); then
    log_error "Failed to search for directories"
    return 1
  fi

  if [ -z "$all_dirs" ]; then
    log_warning "No directories found in $scripts_path"
    return 1
  fi

  log_info "Found $(echo "$all_dirs" | wc -l) director(ies)"

  # Return to original directory
  cd - > /dev/null

  echo "$all_dirs"
}

build_database_objects() {
  local scripts_path="$1"
  local all_dirs="$2"

  local db_array="[]"
  local count=0

  while IFS= read -r dir; do
    [ -z "$dir" ] && continue

    log_success "$dir - included"

    # Build database object (simplified - no config file parsing)
    local db_obj
    db_obj=$(jq -n \
      --arg directory "$dir" \
      '{directory: $directory, name: $directory, has_config: false, config_path: null}')

    if ! db_array=$(echo "$db_array" | jq --argjson obj "$db_obj" '. += [$obj]' 2>&1); then
      log_error "Failed to add database object to array"
      log_error "   JSON: $db_obj"
      return 1
    fi
    count=$((count + 1))
  done <<< "$all_dirs"

  echo "$db_array|$count"
}

#------------------------------------------------------------------------------
# MAIN LOGIC
#------------------------------------------------------------------------------

main() {
  # Validate inputs and tools
  validate_inputs
  validate_tools

  # Validate path exists
  if ! validate_path; then
    write_empty_outputs
    exit 0
  fi

  # Discover directories
  local all_dirs
  if ! all_dirs=$(discover_directories "$SCRIPTS_PATH"); then
    write_empty_outputs
    exit 0
  fi

  # Build database objects and count
  local result
  if ! result=$(build_database_objects "$SCRIPTS_PATH" "$all_dirs"); then
    log_error "Failed to build database objects"
    exit 1
  fi

  # Parse result (format: "json_array|count")
  local db_array="${result%|*}"
  local count="${result##*|}"

  # Summary
  log_section "?? Discovery Summary"
  log_info "Total directories: $(echo "$all_dirs" | wc -l)"
  log_info "Databases to process: $count"
  echo ""

  if [ "$count" -gt 0 ]; then
    log_success "Discovered databases:"
    echo "$db_array" | jq -r '.[] | "   - \(.directory)"'
  else
    log_warning "No databases discovered"
  fi

  # Print outputs for comparison
  print_outputs "$db_array" "$count"

  # Write outputs to GITHUB_OUTPUT
  write_outputs "$db_array" "$count"

  log_success "Database discovery completed"
}

# Execute main function
main "$@"
