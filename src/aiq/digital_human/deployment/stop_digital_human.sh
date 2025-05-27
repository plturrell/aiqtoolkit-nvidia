#!/bin/bash

echo "Stopping Digital Human services..."

cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human

# Stop all services using saved PIDs
for pid_file in pids/*.pid; do
    if [ -f "$pid_file" ]; then
        pid=$(cat "$pid_file")
        echo "Stopping PID: $pid"
        kill $pid 2>/dev/null || true
        rm "$pid_file"
    fi
done

echo "✅ All Digital Human services stopped."