# Vercel Deployment Guide for AIQToolkit

## ✅ **Fixed Vercel Configuration**

The invalid vercel.json files have been corrected. Here's the proper deployment setup:

## **Deployment Options**

### **Option 1: Deploy Main Next.js Frontend (Recommended)**

**Repository:** https://github.com/plturrell/aiqtoolkit-nvidia

**Vercel Settings:**
```
Framework Preset: Next.js
Root Directory: external/aiqtoolkit-opensource-ui
Build Command: npm install && npm run build
Output Directory: .next
Install Command: npm install
```

**Environment Variables (Optional):**
```
OPENAI_API_KEY=your_openai_key
NVIDIA_API_KEY=your_nvidia_key
NODE_ENV=production
```

### **Option 2: Deploy Simple Web UI**

**Repository:** https://github.com/plturrell/aiqtoolkit-nvidia

**Vercel Settings:**
```
Framework Preset: Other
Root Directory: web-ui
Build Command: echo "Static files ready"
Output Directory: public
```

### **Option 3: Auto-Deploy from Root**

**Repository:** https://github.com/plturrell/aiqtoolkit-nvidia

**Vercel Settings:**
```
Framework Preset: Next.js
Root Directory: / (leave empty)
Build Command: (auto-detected from vercel.json)
Output Directory: (auto-detected from vercel.json)
```

## **Fixed Configuration Files**

### **Root vercel.json** ✅
```json
{
  "framework": "nextjs",
  "buildCommand": "cd external/aiqtoolkit-opensource-ui && npm install && npm run build",
  "outputDirectory": "external/aiqtoolkit-opensource-ui/.next",
  "installCommand": "cd external/aiqtoolkit-opensource-ui && npm install"
}
```

### **Frontend vercel.json** ✅
```json
{
  "framework": "nextjs",
  "buildCommand": "npm run build",
  "outputDirectory": ".next",
  "installCommand": "npm install"
}
```

### **Web-UI vercel.json** ✅
```json
{
  "functions": {
    "api/index.py": {
      "runtime": "python3.9"
    }
  },
  "rewrites": [
    {
      "source": "/api/(.*)",
      "destination": "/api/index.py"
    }
  ]
}
```

## **Deployment Steps**

### **1. Go to Vercel**
- Visit: https://vercel.com/new
- Connect GitHub account if needed

### **2. Import Repository**
- Select: `plturrell/aiqtoolkit-nvidia`
- Click "Import"

### **3. Configure Project**
**For Main Frontend (Recommended):**
```
Project Name: aiqtoolkit-nvidia
Framework: Next.js
Root Directory: external/aiqtoolkit-opensource-ui
```

### **4. Deploy**
- Click "Deploy"
- Wait for build completion (2-5 minutes)
- Get deployment URL

## **Features Available After Deployment**

### **🧠 AI Reasoning Systems**
- Interactive chat with 8 reasoning systems
- ReAct, ReWOO, DSPy, MCTS, Neural-Symbolic, etc.
- Real-time reasoning visualization

### **⚡ Neural Supercomputing Interface**
- Performance monitoring dashboards
- Distributed training controls
- Resource utilization metrics

### **🎯 NVIDIA Integration**
- Digital human avatar interface
- GPU performance optimization
- NIM services integration

### **📊 Advanced Features**
- Multi-modal processing
- Real-time collaboration
- Workflow visualization
- Performance profiling

## **Troubleshooting**

### **Build Errors**
```bash
# If build fails, check:
1. Node.js version (requires 18+)
2. Package dependencies
3. Environment variables
4. Root directory setting
```

### **Runtime Errors**
```bash
# Common fixes:
1. Set NODE_ENV=production
2. Add API keys to environment
3. Check function timeout limits
4. Verify CORS settings
```

### **Performance Issues**
```bash
# Optimization:
1. Enable static optimization
2. Configure CDN caching
3. Optimize bundle size
4. Use edge functions
```

## **Post-Deployment**

### **Custom Domain (Optional)**
1. Go to Project Settings → Domains
2. Add your custom domain
3. Configure DNS records
4. Enable SSL certificate

### **Environment Setup**
```bash
# Add to Vercel Environment Variables:
OPENAI_API_KEY=sk-...
NVIDIA_API_KEY=nvapi-...
ANTHROPIC_API_KEY=sk-ant-...
```

### **Monitoring**
- Enable Vercel Analytics
- Set up error tracking
- Configure performance monitoring
- Enable real-time metrics

## **Expected Performance**

### **Build Time**
- Initial build: 3-5 minutes
- Incremental builds: 30-60 seconds
- Static optimization: Enabled

### **Runtime Performance**
- Cold start: <1 second
- Page load: <2 seconds
- API response: <500ms
- Global CDN: Enabled

## **Success Indicators**

✅ **Build completes without errors**
✅ **Frontend loads with chat interface**
✅ **API endpoints respond correctly**
✅ **Documentation pages accessible**
✅ **Neural supercomputing docs visible**
✅ **Reasoning systems functional**

## **URLs After Deployment**

```
Main App: https://your-project.vercel.app
Documentation: https://your-project.vercel.app/docs
API: https://your-project.vercel.app/api
Reasoning: https://your-project.vercel.app/reasoning
```

---

**Your AIQToolkit with Neural Supercomputing Framework is now ready for production deployment on Vercel! 🚀**