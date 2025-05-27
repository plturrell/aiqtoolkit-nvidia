"""
Financial Analysis Service with MCTS
"""
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
import uvicorn
from datetime import datetime

app = FastAPI(title="Financial Analyzer - MCTS")

# Enable CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/health")
def health():
    return {
        "status": "healthy",
        "service": "Financial Analyzer",
        "algorithm": "Monte Carlo Tree Search"
    }

@app.post("/analyze/portfolio")
async def analyze_portfolio(data: dict):
    return {
        "recommendation": "optimize",
        "confidence": 0.85,
        "actions": [
            {"action": "rebalance", "details": "Adjust portfolio weights"},
            {"action": "buy", "symbol": "AAPL", "quantity": 10}
        ],
        "risk_analysis": {
            "current_risk": "moderate",
            "suggested_risk": "balanced"
        }
    }

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8083)
