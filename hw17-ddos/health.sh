#!/bin/bash

# To make it executable: chmod +x health.sh

# URL to check
URL="http://localhost:8080/health"

while true; do
  # Capture the start time
  START_TIME=$(date +%s%N)

  # Send the HTTP request and capture the response
  RESPONSE=$(curl -s -o /dev/null -w "Status %{http_code}, Time %{time_total}s" $URL)

  # Capture the end time
  END_TIME=$(date +%s%N)

  # Calculate the response time in milliseconds
  RESPONSE_TIME=$(echo "scale=3; ($END_TIME - $START_TIME) / 1000000" | bc)

  # Print the response
  echo "$(date +"%H:%M:%S"): ${RESPONSE}"

  # Wait for 2 seconds before the next request
  sleep 2
done