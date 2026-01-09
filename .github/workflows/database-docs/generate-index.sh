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

# Export variables for Python script
export CURRENT_DATE_ESCAPED
export ENVIRONMENT_ESCAPED
export DATABASE_COUNT
export DATABASE_CARDS

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

# Export DATABASE_CARDS now that it's fully built
export DATABASE_CARDS

# Read template file
TEMPLATE_PATH=".github/workflows/database-docs/index.template.html"

if [ ! -f "$TEMPLATE_PATH" ]; then
  echo "❌ ERROR: Template file not found: $TEMPLATE_PATH"
  exit 1
fi

# Copy template to output location
mkdir -p docs/dbs
cp "$TEMPLATE_PATH" docs/dbs/index.html

# Replace placeholders using a Python one-liner (most reliable cross-platform approach)
# Python handles multi-line strings, special characters, and HTML content correctly
python3 << 'PYTHON_SCRIPT'
import os

# Read the template
with open('docs/dbs/index.html', 'r') as f:
    content = f.read()

# Get variables from environment
current_date = os.environ.get('CURRENT_DATE_ESCAPED', '')
database_count = os.environ.get('DATABASE_COUNT', '0')
environment = os.environ.get('ENVIRONMENT_ESCAPED', '')
database_cards = os.environ.get('DATABASE_CARDS', '')

# Replace placeholders
content = content.replace('{{CURRENT_DATE}}', current_date)
content = content.replace('{{DATABASE_COUNT}}', database_count)
content = content.replace('{{ENVIRONMENT}}', environment)
content = content.replace('{{DATABASE_CARDS}}', database_cards)

# Write the result
with open('docs/dbs/index.html', 'w') as f:
    f.write(content)
PYTHON_SCRIPT

echo "✅ Unified index page created at docs/dbs/index.html"
