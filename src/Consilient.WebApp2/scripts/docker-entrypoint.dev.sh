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

exec npm run dev -- --host 0.0.0.0
