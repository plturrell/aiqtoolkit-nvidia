# Digital Human Unity MVP Production Docker Image

# Use nginx as base image for serving WebGL build
FROM nginx:alpine

# Install dependencies
RUN apk add --no-cache \
    python3 \
    py3-pip \
    nodejs \
    npm \
    curl \
    jq

# Create app directory
WORKDIR /app

# Copy Unity WebGL build
COPY Build/WebGL/ /usr/share/nginx/html/

# Copy configuration files
COPY Config/ /app/config/
COPY Monitoring/health_check.py /app/monitoring/

# Copy nginx configuration
COPY Deployment/nginx/nginx.conf /etc/nginx/nginx.conf

# Install Python dependencies for health monitoring
RUN pip3 install --no-cache-dir \
    aiohttp \
    prometheus-client \
    psutil

# Create log directory
RUN mkdir -p /app/logs

# Create entrypoint script
RUN echo '#!/bin/sh' > /app/entrypoint.sh && \
    echo 'echo "Starting Digital Human Unity MVP..."' >> /app/entrypoint.sh && \
    echo '' >> /app/entrypoint.sh && \
    echo '# Start health monitoring in background' >> /app/entrypoint.sh && \
    echo 'python3 /app/monitoring/health_check.py --prometheus-port 8000 &' >> /app/entrypoint.sh && \
    echo '' >> /app/entrypoint.sh && \
    echo '# Start nginx in foreground' >> /app/entrypoint.sh && \
    echo 'exec nginx -g "daemon off;"' >> /app/entrypoint.sh && \
    chmod +x /app/entrypoint.sh

# Expose ports
EXPOSE 80 8000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Set environment variables
ENV BACKEND_URL=ws://backend:8081/ws
ENV API_URL=http://backend:8081/api
ENV METRICS_ENDPOINT=http://metrics:9090/metrics
ENV LOG_LEVEL=info

# Volume for logs
VOLUME ["/app/logs"]

# Run the application
ENTRYPOINT ["/app/entrypoint.sh"]