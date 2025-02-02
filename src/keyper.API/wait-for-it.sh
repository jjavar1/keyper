#!/usr/bin/env bash
set -e

if [ "$#" -lt 2 ]; then
  echo "Usage: $0 host port [command args...]"
  exit 1
fi

HOST=$1
PORT=$2
shift 2
TIMEOUT=15
START_TS=$(date +%s)

echo "Waiting for $HOST:$PORT for up to $TIMEOUT seconds..."
while true; do
  if (echo > /dev/tcp/$HOST/$PORT) >/dev/null 2>&1; then
    echo "$HOST:$PORT is available"
    break
  fi
  NOW_TS=$(date +%s)
  ELAPSED=$((NOW_TS - START_TS))
  if [ $ELAPSED -ge $TIMEOUT ]; then
    echo "Timeout after $TIMEOUT seconds waiting for $HOST:$PORT"
    exit 1
  fi
  sleep 1
done

exec "$@"
