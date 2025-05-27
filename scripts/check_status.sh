#!/bin/bash

echo "AIQToolkit Services Status Check"
echo "================================"
echo ""

# Check frontend
echo -n "Frontend UI: "
if curl -s -I http://localhost:3000 | head -1 | grep -q "200"; then
    echo "✅ Running on http://localhost:3000"
else
    echo "❌ Not responding"
fi

# Check backend
echo -n "Backend API: "
if curl -s http://localhost:8000/health | grep -q "healthy"; then
    echo "✅ Running on http://localhost:8000"
else
    echo "❌ Not responding"
fi

echo ""
echo "Quick Links:"
echo "- Frontend UI: http://localhost:3000"
echo "- Backend API: http://localhost:8000"
echo "- API Docs: http://localhost:8000/docs"
echo ""
echo "If you can't connect to the frontend, try:"
echo "1. Clear your browser cache"
echo "2. Try incognito/private mode"
echo "3. Check for browser extensions blocking localhost"
echo "4. Try http://127.0.0.1:3000 instead of localhost"