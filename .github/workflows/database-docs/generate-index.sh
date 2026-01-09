#!/bin/bash
################################################################################
# Unified Index Page Generator
#
# Purpose:
#   Generates the master index.html page from template with database cards
#
# Expected Environment Variables:
#   - DATABASE_COUNT: Total number of databases processed
#   - environment_input: Environment name (dev/prod)
#
# Expected Files:
#   - /tmp/database_info.txt: Pipe-delimited database metadata
#   - .github/workflows/database-docs/index.template.html: HTML template
#
# Output:
#   - docs/dbs/index.html: Final rendered index page
#
# Security:
#   - All dynamic content HTML-escaped to prevent XSS
################################################################################

set -e
set -o pipefail

# HTML escape function (FIX #4)
escape_html() {
  local input="$1"
  input="${input//&/&amp;}"
  input="${input//</&lt;}"
  input="${input//>/&gt;}"
  input="${input//\"/&quot;}"
  input="${input//$'\x27'/&#39;}"
  echo "$input"
}

# Generate current date
CURRENT_DATE=$(date -u '+%Y-%m-%d %H:%M:%S UTC')
CURRENT_DATE_ESCAPED=$(escape_html "$CURRENT_DATE")
ENVIRONMENT_ESCAPED=$(escape_html "$environment_input")

# Build database cards from metadata file
DATABASE_CARDS=""

if [ -f /tmp/database_info.txt ]; then
  while IFS='|' read -r db_name actual_db_name schemas; do
    db_name_lower=$(echo "$db_name" | tr '[:upper:]' '[:lower:]')

    # Escape all dynamic content (FIX #4)
    DB_NAME_ESCAPED=$(escape_html "$db_name")
    ACTUAL_DB_NAME_ESCAPED=$(escape_html "$actual_db_name")

    # Build schema list with escaped content
    SCHEMA_LIST=""
    for schema in $schemas; do
      schema_escaped=$(escape_html "$schema")
      SCHEMA_LIST="$SCHEMA_LIST<li>$schema_escaped</li>"
    done

    # Count schemas
    schema_count=$(echo "$schemas" | wc -w)

    # Build card HTML (all content is escaped)
    CARD="<div class=\"database-card\">
      <h2>🗄️ $DB_NAME_ESCAPED</h2>
      <p><strong>Physical Database:</strong> $ACTUAL_DB_NAME_ESCAPED</p>
      <p><strong>Schemas ($schema_count):</strong></p>
      <ul>$SCHEMA_LIST</ul>
      <a href=\"./${db_name_lower}/\" class=\"btn\">View Documentation →</a>
    </div>"

    DATABASE_CARDS="$DATABASE_CARDS$CARD"
  done < /tmp/database_info.txt
else
  echo "⚠️  WARNING: No database info found at /tmp/database_info.txt"
  DATABASE_CARDS="<p>No databases processed</p>"
fi

# Read template file
TEMPLATE_PATH=".github/workflows/database-docs/index.template.html"

if [ ! -f "$TEMPLATE_PATH" ]; then
  echo "❌ ERROR: Template file not found: $TEMPLATE_PATH"
  exit 1
fi

# Copy template to output location
mkdir -p docs/dbs
cp "$TEMPLATE_PATH" docs/dbs/index.html

# Escape special characters in replacement strings for sed
CURRENT_DATE_ESCAPED_SED=$(printf '%s\n' "$CURRENT_DATE_ESCAPED" | sed -e 's/[\/&]/\\&/g')
DATABASE_COUNT_SED=$(printf '%s\n' "$DATABASE_COUNT" | sed -e 's/[\/&]/\\&/g')
ENVIRONMENT_ESCAPED_SED=$(printf '%s\n' "$ENVIRONMENT_ESCAPED" | sed -e 's/[\/&]/\\&/g')
DATABASE_CARDS_SED=$(printf '%s\n' "$DATABASE_CARDS" | sed -e 's/[\/&]/\\&/g')

# Replace placeholders using sed
sed -i "s|{{CURRENT_DATE}}|$CURRENT_DATE_ESCAPED_SED|g" docs/dbs/index.html
sed -i "s|{{DATABASE_COUNT}}|$DATABASE_COUNT_SED|g" docs/dbs/index.html
sed -i "s|{{ENVIRONMENT}}|$ENVIRONMENT_ESCAPED_SED|g" docs/dbs/index.html
sed -i "s|{{DATABASE_CARDS}}|$DATABASE_CARDS_SED|g" docs/dbs/index.html

echo "✅ Unified index page created at docs/dbs/index.html"
