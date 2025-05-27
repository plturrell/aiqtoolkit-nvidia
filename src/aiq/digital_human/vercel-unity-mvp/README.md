# Unity Digital Human MVP - Vercel Deployment

## Overview

Production-ready Unity WebGL application with AI-powered digital human, deployed on Vercel with:
- Ready Player Me avatars
- Real-time lip synchronization
- WebSocket/REST API backend
- Brev LangChain integration
- MCP server tools

## Quick Deployment

### 1. Prepare Unity Build
1. Build your Unity project as WebGL
2. Copy build files to `public/unity/`:
   - `DigitalHumanMVP.loader.js`
   - `DigitalHumanMVP.data`
   - `DigitalHumanMVP.framework.js`
   - `DigitalHumanMVP.wasm`

### 2. Configure Environment Variables
Copy `.env.example` to `.env.local` and update:
```bash
BACKEND_URL=https://your-backend-url.com
BREV_API_KEY=your-brev-api-key
BREV_SHELL_ID=langchain-structured-report-generation-6d35aa
LANGCHAIN_API_KEY=your-langchain-api-key
OPENAI_API_KEY=sk-your-openai-api-key
```

### 3. Deploy to Vercel
```bash
npm install -g vercel
vercel
```

Or using the deployment script:
```bash
./deploy-to-vercel.sh
```

## Features

### Unity Integration
- Unity WebGL with React wrapper
- Real-time avatar control
- Lip sync animation
- Gesture system support

### AI Backend
- LangChain integration
- OpenAI GPT models
- MCP tool support
- Brev environment connection

### UI/UX
- Material-UI components
- Real-time chat interface
- Settings panel
- Connection status

## Architecture

```
┌─────────────────┐     ┌─────────────────┐
│  Vercel Edge    │────▶│  Unity WebGL    │
│  Functions      │     │  Application    │
└─────────────────┘     └─────────────────┘
         │                       │
         ▼                       ▼
┌─────────────────┐     ┌─────────────────┐
│  Backend API    │────▶│  WebSocket      │
│  (Your Server)  │     │  Server         │
└─────────────────┘     └─────────────────┘
         │                       │
         ▼                       ▼
┌─────────────────┐     ┌─────────────────┐
│  Brev Shell     │────▶│  LangChain      │
│  Integration    │     │  Reports        │
└─────────────────┘     └─────────────────┘
```

## Configuration

### Vercel Settings
1. Go to Vercel Dashboard
2. Add environment variables
3. Configure domains
4. Set function regions

### Unity Build Settings
1. Enable compression
2. Configure CORS headers
3. Optimize for web
4. Set WebAssembly settings

## Local Development

```bash
# Install dependencies
npm install

# Run development server
npm run dev

# Build for production
npm run build

# Start production server
npm start
```

## API Endpoints

### Chat API
```typescript
POST /api/chat
{
  "query": "user message",
  "use_mcp": true,
  "use_brev": true
}
```

### WebSocket Events
- `connect`: Establish connection
- `chat_message`: Send message
- `chat_response`: Receive response
- `error`: Handle errors

## Troubleshooting

### Unity Not Loading
- Check Unity build files in `public/unity/`
- Verify CORS headers in `next.config.js`
- Check browser console for errors

### WebSocket Connection Failed
- Verify backend URL in environment variables
- Check WebSocket server is running
- Try REST API fallback

### Brev Integration Issues
- Confirm Brev shell ID
- Check API key permissions
- Verify shell is running

## Performance Optimization

### Unity Optimization
- Enable GPU acceleration
- Reduce texture sizes
- Optimize mesh count
- Use LOD systems

### Vercel Optimization
- Enable caching
- Use CDN for assets
- Optimize images
- Minimize bundle size

## Security

### API Security
- Use environment variables
- Implement rate limiting
- Add authentication
- Enable CORS properly

### Data Protection
- Encrypt sensitive data
- Use secure connections
- Implement user sessions
- Follow privacy laws

## Monitoring

### Vercel Analytics
- Page load times
- API performance
- Error tracking
- User metrics

### Custom Metrics
- Unity performance
- WebSocket latency
- AI response times
- User engagement

## Support

- GitHub: [AIQToolkit Issues](https://github.com/aiqtoolkit/issues)
- Discord: [Community Support](https://discord.gg/aiqtoolkit)
- Email: support@aiqtoolkit.com

---

**Version**: 1.0.0  
**License**: MIT  
**Last Updated**: May 2025