# Digital Human + Brev LangChain Integration Guide

## Quick Start

### 1. Deploy to Vercel

```bash
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human/vercel-digital-human
./deploy.sh
```

### 2. Setup Brev Environment

SSH into your Brev shell:
```bash
brev shell langchain-structured-report-generation-6d35aa
```

Copy the integration script:
```bash
# In Brev shell
curl -o api_server.py https://raw.githubusercontent.com/your-repo/brev-integration.py
```

Or create it manually with the provided `brev-integration.py` content.

### 3. Configure Environment Variables

In Vercel Dashboard, add:
- `NVIDIA_API_KEY`: Your NVIDIA API key
- `BREV_API_KEY`: Generated from Brev
- `BREV_API_ENDPOINT`: Your Brev instance URL
- `LANGCHAIN_ENDPOINT`: Same as BREV_API_ENDPOINT

### 4. Start Brev API Server

In Brev shell:
```bash
pip install fastapi uvicorn
python api_server.py
```

### 5. Test the Integration

Visit your Vercel deployment and try:
- "Generate a financial report for Q4 2023"
- "Analyze market trends for technology sector"
- "Create a comprehensive analysis of NVIDIA stock"

## Architecture

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│   Vercel App    │────▶│   API Routes     │────▶│  External APIs  │
│  (Next.js UI)   │     │                  │     │                 │
└─────────────────┘     └──────────────────┘     └─────────────────┘
                               │                          │
                               ├──────────────────────────┤
                               │                          │
                        ┌──────▼──────┐            ┌──────▼──────┐
                        │  NVIDIA NIM │            │ Brev/LangChain│
                        │   (LLM)     │            │  (Reports)    │
                        └─────────────┘            └──────────────┘
```

## API Endpoints

### Vercel App
- `GET /`: Main UI
- `POST /api/chat`: Process queries

### Brev API
- `POST /api/generate`: Generate reports
- `POST /api/analyze`: Analyze data
- `GET /api/health`: Health check

## Customization

### Add Custom Report Types

In `brev-integration.py`:
```python
@app.post("/api/custom-report")
async def custom_report(request: QueryRequest):
    # Your custom logic here
    return {"report": custom_report_content}
```

### Enhance UI

In `app/page.tsx`:
```typescript
// Add new features
const [reportType, setReportType] = useState('standard');

// Add to UI
<select onChange={(e) => setReportType(e.target.value)}>
  <option value="standard">Standard Report</option>
  <option value="financial">Financial Analysis</option>
  <option value="market">Market Research</option>
</select>
```

## Troubleshooting

### CORS Issues
Ensure your Brev API includes the Vercel domain in allowed origins.

### API Timeouts
Increase timeout in `vercel.json`:
```json
{
  "functions": {
    "app/api/chat/route.ts": {
      "maxDuration": 60
    }
  }
}
```

### Environment Variables
Verify all environment variables are set correctly in both Vercel and Brev.

## Next Steps

1. Customize the UI for your specific use case
2. Add authentication for secure access
3. Implement caching for better performance
4. Add more AI models and capabilities
5. Create custom report templates