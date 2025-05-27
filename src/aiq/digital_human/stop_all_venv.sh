#!/bin/bash
echo "Stopping all services..."
for pid_file in pids/*.pid; do
    if [ -f "$pid_file" ]; then
        pid=$(cat "$pid_file")
        kill $pid 2>/dev/null || true
        rm "$pid_file"
    fi
done
echo "All services stopped."
