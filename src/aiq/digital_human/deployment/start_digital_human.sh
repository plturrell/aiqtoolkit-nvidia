#!/bin/bash

echo "🚀 Starting Digital Human Services"
echo "================================"

# Set up environment
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human

# Create logs directory
mkdir -p logs

# Start main API server
echo "Starting main API server..."
NVIDIA_API_KEY="${NVIDIA_API_KEY:-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}" \
python3 ui/api_server.py > logs/api_server.log 2>&1 &
API_PID=$!
echo "API server started (PID: $API_PID)"

# Start digital human avatar server
echo "Starting avatar renderer..."
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human/avatar
python3 avatar_renderer_server.py > ../logs/avatar_renderer.log 2>&1 &
AVATAR_PID=$!
echo "Avatar renderer started (PID: $AVATAR_PID)"

# Start financial analysis server
echo "Starting financial analyzer..."
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human/financial
python3 mcts_analyzer_server.py > ../logs/mcts_analyzer.log 2>&1 &
MCTS_PID=$!
echo "Financial analyzer started (PID: $MCTS_PID)"

# Start neural reasoning server
echo "Starting neural reasoning..."
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human/neural
python3 neural_supercomputer_connector_server.py > ../logs/neural_connector.log 2>&1 &
NEURAL_PID=$!
echo "Neural reasoning started (PID: $NEURAL_PID)"

# Start conversation engine
echo "Starting conversation engine..."
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human/conversation
python3 sglang_engine_server.py > ../logs/sglang_engine.log 2>&1 &
SGLANG_PID=$!
echo "Conversation engine started (PID: $SGLANG_PID)"

# Save PIDs for later shutdown
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human
mkdir -p pids
echo $API_PID > pids/api_server.pid
echo $AVATAR_PID > pids/avatar_renderer.pid
echo $MCTS_PID > pids/mcts_analyzer.pid
echo $NEURAL_PID > pids/neural_connector.pid
echo $SGLANG_PID > pids/sglang_engine.pid

echo -e "\n✅ Digital Human services started!"
echo "Access the application at: http://localhost:8080"
echo ""
echo "To stop all services, run: ./stop_digital_human.sh"
echo ""
echo "Logs are available in: /Users/apple/projects/AIQToolkit/src/aiq/digital_human/logs/"