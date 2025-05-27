# Digital Human - Vercel Deployment

This is the Vercel deployment of the NVIDIA Digital Human system integrated with Brev LangChain.

## Features

- NVIDIA ACE Avatar integration
- Real-time chat with NVIDIA NIM (Llama3-70B)
- Web search capabilities
- LangChain integration from Brev environment
- Responsive UI built with Next.js and Tailwind CSS

## Deployment Steps

1. **Clone and Navigate**
   ```bash
   cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human/vercel-digital-human
   ```

2. **Install Dependencies**
   ```bash
   npm install
   ```

3. **Set Environment Variables**
   Create a `.env.local` file:
   ```
   NVIDIA_API_KEY=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
   BREV_API_KEY=<your-brev-api-key>
   BREV_API_ENDPOINT=https://langchain-structured-report-generation-6d35aa.brev.dev/api
   LANGCHAIN_ENDPOINT=https://langchain-structured-report-generation-6d35aa.brev.dev/api
   ```

4. **Deploy to Vercel**
   ```bash
   npx vercel
   ```

   Or push to GitHub and connect to Vercel:
   ```bash
   git init
   git add .
   git commit -m "Digital Human Vercel deployment"
   git remote add origin https://github.com/plturrell/digital-human-vercel.git
   git push -u origin main
   ```

5. **Configure Vercel Environment Variables**
   In Vercel dashboard, add:
   - `NVIDIA_API_KEY`
   - `BREV_API_KEY`
   - `BREV_API_ENDPOINT`
   - `LANGCHAIN_ENDPOINT`

## Brev Integration

To connect with your Brev LangChain environment:

1. **In Brev Shell** (`langchain-structured-report-generation-6d35aa`):
   ```python
   # Add API endpoint to expose LangChain
   from fastapi import FastAPI
   from langchain import ...
   
   app = FastAPI()
   
   @app.post("/api/generate")
   async def generate_report(data: dict):
       # Your LangChain logic here
       return {"report": generated_report}
   ```

2. **Configure CORS** in Brev:
   ```python
   from fastapi.middleware.cors import CORSMiddleware
   
   app.add_middleware(
       CORSMiddleware,
       allow_origins=["https://your-vercel-app.vercel.app"],
       allow_credentials=True,
       allow_methods=["*"],
       allow_headers=["*"],
   )
   ```

3. **Deploy Brev API**:
   ```bash
   # In Brev shell
   uvicorn main:app --host 0.0.0.0 --port 8000
   ```

## Usage

Once deployed, your digital human will:
1. Accept user queries
2. Search the web for relevant information
3. Query your Brev LangChain instance
4. Generate comprehensive responses using NVIDIA AI
5. Display results with source attribution

## Architecture

```
User → Vercel App → API Routes
                    ├── Web Search API
                    ├── Brev LangChain API
                    └── NVIDIA NIM API
```

## Development

```bash
# Local development
npm run dev

# Build
npm run build

# Start production server
npm start
```

## Support

- NVIDIA API Documentation: https://docs.nvidia.com/ace
- Brev Documentation: https://docs.brev.dev
- Vercel Documentation: https://vercel.com/docs