# Vercel Deployment Guide - Unity Digital Human MVP

## Overview

This guide deploys the Unity Digital Human MVP to Vercel with full integration to:
- Unity WebGL application
- Backend API services  
- Brev LangChain environment
- WebSocket real-time communication
- Ready Player Me avatars

## Prerequisites

1. **Vercel Account**: Sign up at [vercel.com](https://vercel.com)
2. **Unity Build**: WebGL build of your Unity project
3. **Backend Server**: Running AIQToolkit backend
4. **API Keys**: 
   - Brev API key
   - LangChain API key  
   - OpenAI API key

## Step 1: Prepare Unity Build

### Build Unity Project for WebGL
1. Open Unity project
2. File > Build Settings
3. Select "WebGL" platform
4. Build Settings:
   - Compression: Gzip
   - Enable exceptions: Full
   - WebAssembly: Enabled
5. Build

### Copy Build Files
Copy these files to `vercel-unity-mvp/public/unity/`:
- `DigitalHumanMVP.loader.js`
- `DigitalHumanMVP.data`
- `DigitalHumanMVP.framework.js`
- `DigitalHumanMVP.wasm`

## Step 2: Configure Environment

### Create Environment Variables File
```bash
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human/vercel-unity-mvp
cp .env.example .env.local
```

### Update .env.local
```env
# Your Backend Server
BACKEND_URL=https://your-backend.com
NEXT_PUBLIC_API_URL=https://your-backend.com
NEXT_PUBLIC_WS_URL=wss://your-backend.com/ws

# Brev Integration
BREV_API_KEY=your-brev-api-key
BREV_SHELL_ID=langchain-structured-report-generation-6d35aa

# API Keys
LANGCHAIN_API_KEY=your-langchain-key
OPENAI_API_KEY=sk-your-openai-key
```

## Step 3: Deploy to Vercel

### Option A: Automatic Deployment
```bash
cd /Users/apple/projects/AIQToolkit/src/aiq/digital_human/vercel-unity-mvp
./deploy-to-vercel.sh
```

### Option B: Manual Deployment

1. **Install Vercel CLI**
   ```bash
   npm install -g vercel
   ```

2. **Login to Vercel**
   ```bash
   vercel login
   ```

3. **Deploy**
   ```bash
   vercel --prod
   ```

4. **Follow prompts**:
   - Project name: unity-digital-human-mvp
   - Framework: Next.js
   - Build command: npm run build
   - Output directory: .next
   - Development command: npm run dev

## Step 4: Configure Vercel Dashboard

### Add Environment Variables
1. Go to [Vercel Dashboard](https://vercel.com/dashboard)
2. Select your project
3. Settings > Environment Variables
4. Add these variables:

| Variable | Value | Environment |
|----------|-------|-------------|
| BACKEND_URL | Your backend URL | Production |
| BREV_API_KEY | Your Brev API | Production |
| BREV_SHELL_ID | langchain-structured-report-generation-6d35aa | Production |
| LANGCHAIN_API_KEY | Your LangChain key | Production |
| OPENAI_API_KEY | Your OpenAI key | Production |
| NEXT_PUBLIC_WS_URL | wss://your-backend/ws | Production |
| NEXT_PUBLIC_API_URL | https://your-backend | Production |

### Configure Domains
1. Settings > Domains
2. Add custom domain (optional)
3. Configure SSL certificate

### Set Function Regions
1. Settings > Functions
2. Region: Select closest to your backend
3. Timeout: 30 seconds

## Step 5: Backend Connection

### Configure CORS
Ensure your backend allows requests from Vercel domain:
```python
# In your backend API
CORS_ORIGINS = [
    "https://your-app.vercel.app",
    "https://your-custom-domain.com"
]
```

### WebSocket Configuration
Update WebSocket server to accept connections:
```python
# In your WebSocket handler
allowed_origins = [
    "https://your-app.vercel.app",
    "https://your-custom-domain.com"
]
```

## Step 6: Test Deployment

### Verify Unity Loading
1. Visit your Vercel URL
2. Check Unity WebGL loads
3. Verify avatar appears

### Test Chat Functionality
1. Type message in chat
2. Verify AI response
3. Check lip sync animation

### Monitor Logs
1. Vercel Dashboard > Functions > Logs
2. Check for errors
3. Monitor performance

## Architecture

```
┌─────────────────┐     ┌─────────────────┐
│  Browser        │────▶│  Vercel Edge    │
│  (Unity WebGL)  │     │  Network        │
└─────────────────┘     └─────────────────┘
         │                       │
         ▼                       ▼
┌─────────────────┐     ┌─────────────────┐
│  Vercel         │────▶│  Your Backend   │
│  Functions      │     │  Server         │
└─────────────────┘     └─────────────────┘
         │                       │
         ▼                       ▼
┌─────────────────┐     ┌─────────────────┐
│  Brev Shell     │────▶│  LangChain      │
│  Integration    │     │  Processing     │
└─────────────────┘     └─────────────────┘
```

## Performance Optimization

### Unity Optimization
- Compress textures
- Reduce polygon count
- Enable GPU instancing
- Use LOD system

### Vercel Optimization
- Enable caching
- Use CDN for assets
- Optimize images
- Minimize JavaScript

## Troubleshooting

### Common Issues

1. **Unity Build Not Loading**
   - Check file paths in `public/unity/`
   - Verify CORS headers
   - Check browser console

2. **WebSocket Connection Failed**
   - Verify backend URL
   - Check SSL certificate
   - Test with HTTP first

3. **Slow Performance**
   - Enable Vercel caching
   - Optimize Unity build
   - Use CDN for assets

### Debug Mode
```javascript
// In your code
console.log('Unity loaded:', unityLoaded);
console.log('WebSocket connected:', isConnected);
console.log('Backend response:', response);
```

## Monitoring

### Vercel Analytics
- Page load times
- API latency
- Error rates
- User sessions

### Custom Metrics
- Unity frame rate
- WebSocket latency
- AI response time
- Avatar load time

## Security Best Practices

1. **Environment Variables**
   - Never commit keys to git
   - Use Vercel secrets
   - Rotate keys regularly

2. **API Security**
   - Implement rate limiting
   - Add authentication
   - Validate inputs

3. **CORS Configuration**
   - Whitelist domains
   - Use HTTPS only
   - Check origins

## Cost Optimization

### Vercel Pricing
- Free tier: 100GB bandwidth
- Pro: $20/month unlimited
- Enterprise: Custom pricing

### Optimization Tips
1. Cache static assets
2. Optimize images
3. Minimize API calls
4. Use edge functions

## Next Steps

1. **Custom Domain**: Add your domain
2. **Analytics**: Set up monitoring
3. **Scaling**: Configure auto-scaling
4. **Features**: Add new capabilities

## Support

- Vercel Docs: [vercel.com/docs](https://vercel.com/docs)
- AIQToolkit: support@aiqtoolkit.com
- Discord: [Join community](https://discord.gg/aiqtoolkit)

---

**Deployment URL**: `https://unity-digital-human-mvp.vercel.app`  
**Status**: Production Ready  
**Version**: 1.0.0