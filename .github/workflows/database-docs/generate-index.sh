#!/bin/bash
################################################################################
# Unified Index Page Generator
#
# Purpose:
#   Generates the master index.html page and per-database index pages
#
# Expected Environment Variables:
#   - DATABASE_COUNT: Total number of databases processed
#   - environment_input: Environment name (dev/prod)
#
# Expected Files:
#   - /tmp/database_info.txt: Pipe-delimited database metadata
#   - .github/workflows/database-docs/index.template.html: HTML template
#   - .github/workflows/database-docs/database.template.html: Database page template
#
# Output:
#   - docs/index.html: Root redirect to dbs/
#   - docs/dbs/index.html: Main index page listing all databases
#   - docs/dbs/<database>/index.html: Per-database index pages listing schemas
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

# Database template path
DB_TEMPLATE_PATH=".github/workflows/database-docs/database.template.html"

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

    # Generate per-database index page
    if [ -f "$DB_TEMPLATE_PATH" ]; then
      # Build schema cards for this database
      SCHEMA_CARDS=""
      for schema in $schemas; do
        schema_escaped=$(escape_html "$schema")
        schema_lower=$(echo "$schema" | tr '[:upper:]' '[:lower:]')
        SCHEMA_CARD="<div class=\"schema-card\">
          <h2>📋 $schema_escaped</h2>
          <p>View tables, relationships, and constraints for the <strong>$schema_escaped</strong> schema.</p>
          <a href=\"./${schema_lower}/\" class=\"btn\">View Schema →</a>
        </div>"
        SCHEMA_CARDS="$SCHEMA_CARDS$SCHEMA_CARD"
      done

      # Create database directory and generate index
      mkdir -p "docs/dbs/${db_name_lower}"
      cp "$DB_TEMPLATE_PATH" "docs/dbs/${db_name_lower}/index.html"

      # Export variables for Python replacement
      export DB_NAME_ESCAPED
      export ACTUAL_DB_NAME_ESCAPED
      export SCHEMA_COUNT="$schema_count"
      export SCHEMA_CARDS

      # Replace placeholders in database index
      python3 << PYTHON_DB_SCRIPT
import os

db_index_path = "docs/dbs/${db_name_lower}/index.html"

with open(db_index_path, 'r') as f:
    content = f.read()

content = content.replace('{{DB_NAME}}', os.environ.get('DB_NAME_ESCAPED', ''))
content = content.replace('{{ACTUAL_DB_NAME}}', os.environ.get('ACTUAL_DB_NAME_ESCAPED', ''))
content = content.replace('{{SCHEMA_COUNT}}', os.environ.get('SCHEMA_COUNT', '0'))
content = content.replace('{{ENVIRONMENT}}', os.environ.get('ENVIRONMENT_ESCAPED', ''))
content = content.replace('{{CURRENT_DATE}}', os.environ.get('CURRENT_DATE_ESCAPED', ''))
content = content.replace('{{SCHEMA_CARDS}}', os.environ.get('SCHEMA_CARDS', ''))

with open(db_index_path, 'w') as f:
    f.write(content)
PYTHON_DB_SCRIPT

      echo "✅ Database index page created at docs/dbs/${db_name_lower}/index.html"
    fi

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

# Create root redirect to dbs/
cat > docs/index.html << 'ROOT_REDIRECT'
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta http-equiv="refresh" content="0; url=dbs/">
    <title>Redirecting...</title>
</head>
<body>
    <p>Redirecting to <a href="dbs/">database documentation</a>...</p>
</body>
</html>
ROOT_REDIRECT

echo "✅ Root redirect created at docs/index.html"
