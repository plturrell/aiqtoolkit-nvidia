from fastapi import FastAPI
import uvicorn
import os
import glob

app = FastAPI(title="MCP File Browser Server")

@app.post("/search")
async def search_files(data: dict):
    query = data.get("query", "")
    path = data.get("path", "/Users/apple/projects/AIQToolkit")
    
    # Search for files matching query
    files = []
    
    for root, dirs, filenames in os.walk(path):
        for filename in filenames:
            if query.lower() in filename.lower():
                files.append({
                    "path": os.path.join(root, filename),
                    "name": filename,
                    "size": os.path.getsize(os.path.join(root, filename))
                })
        
        if len(files) > 10:  # Limit results
            break
    
    return {"files": files}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8092)
