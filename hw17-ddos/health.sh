#!/bin/bash

# To make it executable: chmod +x health.sh

# URL to check
URL="http://localhost:8080/health"

while true; do
  # Send the HTTP request and capture the response
  RESPONSE=$(curl -s -o /dev/null -w "Status %{http_code}, Time %{time_total}s" $URL)

  # Print the response
  echo "$(date +"%H:%M:%S"): ${RESPONSE}"

  # Wait for 2 seconds before the next request
  sleep 2
done