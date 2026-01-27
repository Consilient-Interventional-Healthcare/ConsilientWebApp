#!/bin/sh
set -e

OUT=/app/public/env.js

cat > $OUT <<'EOF'
window.__ENV = {
EOF

first=true
for var in $(env | awk -F= '{print $1}' | grep -E '^(APP_|NODE_ENV)' || true); do
  val=$(printf '%s' "$(printenv $var)" | sed -e 's/"/\\"/g')
  if [ "$first" = true ]; then
    first=false
  else
    printf ',\n' >> $OUT
  fi
  printf '  "%s": "%s"' "$var" "$val" >> $OUT
done

cat >> $OUT <<'EOF'

};
console.log('window.__ENV loaded:', window.__ENV);
EOF

# Install packages only if package-lock.json has changed
HASH_FILE="/app/node_modules/.package-lock-hash"
CURRENT_HASH=$(md5sum /app/package-lock.json 2>/dev/null | cut -d' ' -f1)

if [ -f "$HASH_FILE" ] && [ "$(cat $HASH_FILE)" = "$CURRENT_HASH" ]; then
  echo "Dependencies up to date, skipping npm install"
else
  echo "Installing dependencies..."
  npm install
  echo "$CURRENT_HASH" > "$HASH_FILE"
fi

exec npm run dev -- --host 0.0.0.0
