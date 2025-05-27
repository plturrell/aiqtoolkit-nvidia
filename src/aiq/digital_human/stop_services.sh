#!/bin/bash
echo "Stopping all services..."
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human
for pid_file in pids/*.pid; do
    if [ -f "$pid_file" ]; then
        kill $(cat "$pid_file") 2>/dev/null || true
        rm "$pid_file"
    fi
done
echo "All services stopped."
