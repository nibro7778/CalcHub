#!/bin/sh

# Replace the placeholder in config.js with the actual API URL from environment variable
if [ ! -z "$VITE_API_URL" ]; then
    echo "Configuring API URL: $VITE_API_URL"
    sed -i "s|__VITE_API_URL__|$VITE_API_URL|g" /home/site/wwwroot/config.js
fi

# Start serving the static files
pm2 serve /home/site/wwwroot --no-daemon --spa
