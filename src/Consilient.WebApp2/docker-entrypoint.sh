#!/bin/sh
set -e

OUT=/usr/share/nginx/html/env.js

# Start the env file
cat > $OUT <<'EOF'
window.__ENV = {
EOF

first=true
# Export all environment variables that start with VITE_ or other expected keys
for var in $(env | awk -F= '{print $1}' | grep -E '^(VITE_|MSAL_|APP_)' || true); do
  val=$(printf '%s' "$(printenv $var)" | sed -e 's/"/\\"/g')
  if [ "$first" = true ] ; then
    first=false
  else
    printf ',\n' >> $OUT
  fi
  printf '  "%s": "%s"' "$var" "$val" >> $OUT
done

# Close the object
cat >> $OUT <<'EOF'
};
EOF

# Start nginx
exec nginx -g 'daemon off;'
