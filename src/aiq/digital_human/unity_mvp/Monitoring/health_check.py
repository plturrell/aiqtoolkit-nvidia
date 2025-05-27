#!/usr/bin/env python3
"""
Production health check monitoring for Digital Human Unity MVP
Monitors backend connectivity, system health, and performance metrics
"""

import asyncio
import aiohttp
import json
import logging
import sys
import time
from dataclasses import dataclass
from datetime import datetime
from typing import Dict, List, Optional
import argparse
from prometheus_client import start_http_server, Gauge, Counter

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# Prometheus metrics
health_status = Gauge('digital_human_health_status', 'Overall health status (1=healthy, 0=unhealthy)')
backend_latency = Gauge('digital_human_backend_latency_ms', 'Backend response latency in milliseconds')
websocket_status = Gauge('digital_human_websocket_status', 'WebSocket connection status (1=connected, 0=disconnected)')
health_check_total = Counter('digital_human_health_check_total', 'Total number of health checks', ['status'])
error_count = Counter('digital_human_health_check_errors', 'Number of health check errors', ['type'])

@dataclass
class HealthCheckResult:
    """Health check result data"""
    timestamp: datetime
    service: str
    status: str
    latency_ms: float
    details: Dict
    error: Optional[str] = None

class HealthChecker:
    """Production health checker for Digital Human Unity MVP"""
    
    def __init__(self, config: Dict):
        self.config = config
        self.backend_url = config.get('backend_url', 'http://localhost:8081')
        self.ws_url = config.get('ws_url', 'ws://localhost:8081/ws')
        self.timeout = config.get('timeout', 10)
        self.retry_count = config.get('retry_count', 3)
        self.results: List[HealthCheckResult] = []
        
    async def check_backend_api(self) -> HealthCheckResult:
        """Check backend API health"""
        start_time = time.time()
        
        try:
            async with aiohttp.ClientSession() as session:
                async with session.get(
                    f"{self.backend_url}/health",
                    timeout=aiohttp.ClientTimeout(total=self.timeout)
                ) as response:
                    latency_ms = (time.time() - start_time) * 1000
                    
                    if response.status == 200:
                        data = await response.json()
                        return HealthCheckResult(
                            timestamp=datetime.utcnow(),
                            service="backend_api",
                            status="healthy",
                            latency_ms=latency_ms,
                            details=data
                        )
                    else:
                        return HealthCheckResult(
                            timestamp=datetime.utcnow(),
                            service="backend_api",
                            status="unhealthy",
                            latency_ms=latency_ms,
                            details={"status_code": response.status},
                            error=f"HTTP {response.status}"
                        )
        except asyncio.TimeoutError:
            error_count.labels(type='timeout').inc()
            return HealthCheckResult(
                timestamp=datetime.utcnow(),
                service="backend_api",
                status="unhealthy",
                latency_ms=(time.time() - start_time) * 1000,
                details={},
                error="Request timeout"
            )
        except Exception as e:
            error_count.labels(type='exception').inc()
            return HealthCheckResult(
                timestamp=datetime.utcnow(),
                service="backend_api",
                status="unhealthy",
                latency_ms=(time.time() - start_time) * 1000,
                details={},
                error=str(e)
            )
    
    async def check_websocket(self) -> HealthCheckResult:
        """Check WebSocket connectivity"""
        start_time = time.time()
        
        try:
            async with aiohttp.ClientSession() as session:
                async with session.ws_connect(
                    self.ws_url,
                    timeout=aiohttp.ClientTimeout(total=self.timeout)
                ) as ws:
                    # Send ping message
                    await ws.send_json({"type": "ping"})
                    
                    # Wait for pong response
                    msg = await ws.receive(timeout=self.timeout)
                    latency_ms = (time.time() - start_time) * 1000
                    
                    if msg.type == aiohttp.WSMsgType.TEXT:
                        data = json.loads(msg.data)
                        if data.get("type") == "pong":
                            await ws.close()
                            return HealthCheckResult(
                                timestamp=datetime.utcnow(),
                                service="websocket",
                                status="healthy",
                                latency_ms=latency_ms,
                                details={"ping_pong": "success"}
                            )
                    
                    await ws.close()
                    return HealthCheckResult(
                        timestamp=datetime.utcnow(),
                        service="websocket",
                        status="unhealthy",
                        latency_ms=latency_ms,
                        details={"error": "Invalid response"},
                        error="No pong received"
                    )
        except asyncio.TimeoutError:
            error_count.labels(type='ws_timeout').inc()
            return HealthCheckResult(
                timestamp=datetime.utcnow(),
                service="websocket",
                status="unhealthy",
                latency_ms=(time.time() - start_time) * 1000,
                details={},
                error="WebSocket timeout"
            )
        except Exception as e:
            error_count.labels(type='ws_exception').inc()
            return HealthCheckResult(
                timestamp=datetime.utcnow(),
                service="websocket",
                status="unhealthy",
                latency_ms=(time.time() - start_time) * 1000,
                details={},
                error=str(e)
            )
    
    async def check_database(self) -> HealthCheckResult:
        """Check database connectivity through backend"""
        start_time = time.time()
        
        try:
            async with aiohttp.ClientSession() as session:
                async with session.get(
                    f"{self.backend_url}/api/health/database",
                    timeout=aiohttp.ClientTimeout(total=self.timeout)
                ) as response:
                    latency_ms = (time.time() - start_time) * 1000
                    
                    if response.status == 200:
                        data = await response.json()
                        return HealthCheckResult(
                            timestamp=datetime.utcnow(),
                            service="database",
                            status="healthy",
                            latency_ms=latency_ms,
                            details=data
                        )
                    else:
                        return HealthCheckResult(
                            timestamp=datetime.utcnow(),
                            service="database",
                            status="unhealthy",
                            latency_ms=latency_ms,
                            details={"status_code": response.status},
                            error=f"HTTP {response.status}"
                        )
        except Exception as e:
            error_count.labels(type='db_exception').inc()
            return HealthCheckResult(
                timestamp=datetime.utcnow(),
                service="database",
                status="unhealthy",
                latency_ms=(time.time() - start_time) * 1000,
                details={},
                error=str(e)
            )
    
    async def check_redis(self) -> HealthCheckResult:
        """Check Redis connectivity through backend"""
        start_time = time.time()
        
        try:
            async with aiohttp.ClientSession() as session:
                async with session.get(
                    f"{self.backend_url}/api/health/redis",
                    timeout=aiohttp.ClientTimeout(total=self.timeout)
                ) as response:
                    latency_ms = (time.time() - start_time) * 1000
                    
                    if response.status == 200:
                        data = await response.json()
                        return HealthCheckResult(
                            timestamp=datetime.utcnow(),
                            service="redis",
                            status="healthy",
                            latency_ms=latency_ms,
                            details=data
                        )
                    else:
                        return HealthCheckResult(
                            timestamp=datetime.utcnow(),
                            service="redis",
                            status="unhealthy",
                            latency_ms=latency_ms,
                            details={"status_code": response.status},
                            error=f"HTTP {response.status}"
                        )
        except Exception as e:
            error_count.labels(type='redis_exception').inc()
            return HealthCheckResult(
                timestamp=datetime.utcnow(),
                service="redis",
                status="unhealthy",
                latency_ms=(time.time() - start_time) * 1000,
                details={},
                error=str(e)
            )
    
    async def run_health_checks(self) -> Dict[str, HealthCheckResult]:
        """Run all health checks"""
        checks = [
            self.check_backend_api(),
            self.check_websocket(),
            self.check_database(),
            self.check_redis()
        ]
        
        results = await asyncio.gather(*checks, return_exceptions=True)
        
        health_results = {}
        all_healthy = True
        
        for result in results:
            if isinstance(result, Exception):
                logger.error(f"Health check failed with exception: {result}")
                all_healthy = False
                continue
            
            health_results[result.service] = result
            
            # Update Prometheus metrics
            if result.service == "backend_api":
                backend_latency.set(result.latency_ms)
            elif result.service == "websocket":
                websocket_status.set(1 if result.status == "healthy" else 0)
            
            if result.status != "healthy":
                all_healthy = False
            
            health_check_total.labels(status=result.status).inc()
        
        # Update overall health status
        health_status.set(1 if all_healthy else 0)
        
        # Store results for history
        self.results.extend(health_results.values())
        
        # Keep only last 1000 results
        if len(self.results) > 1000:
            self.results = self.results[-1000:]
        
        return health_results
    
    def get_summary(self) -> Dict:
        """Get health check summary"""
        if not self.results:
            return {"status": "no_data", "services": {}}
        
        # Group by service
        services = {}
        for result in self.results[-100:]:  # Last 100 results
            if result.service not in services:
                services[result.service] = {
                    "total": 0,
                    "healthy": 0,
                    "avg_latency_ms": 0,
                    "errors": []
                }
            
            service_stats = services[result.service]
            service_stats["total"] += 1
            if result.status == "healthy":
                service_stats["healthy"] += 1
            service_stats["avg_latency_ms"] += result.latency_ms
            
            if result.error:
                service_stats["errors"].append(result.error)
        
        # Calculate averages and health percentage
        overall_health = 0
        for service, stats in services.items():
            stats["avg_latency_ms"] /= stats["total"]
            stats["health_percentage"] = (stats["healthy"] / stats["total"]) * 100
            overall_health += stats["health_percentage"]
        
        overall_health /= len(services)
        
        return {
            "status": "healthy" if overall_health > 90 else "degraded" if overall_health > 70 else "unhealthy",
            "overall_health_percentage": overall_health,
            "services": services,
            "last_check": self.results[-1].timestamp.isoformat() if self.results else None
        }
    
    async def continuous_monitoring(self, interval: int = 30):
        """Run continuous health monitoring"""
        logger.info(f"Starting continuous health monitoring with {interval}s interval")
        
        while True:
            try:
                results = await self.run_health_checks()
                
                # Log results
                for service, result in results.items():
                    if result.status == "healthy":
                        logger.info(f"{service}: {result.status} (latency: {result.latency_ms:.2f}ms)")
                    else:
                        logger.warning(f"{service}: {result.status} - {result.error}")
                
                # Get and log summary
                summary = self.get_summary()
                logger.info(f"Overall health: {summary['status']} ({summary['overall_health_percentage']:.1f}%)")
                
                # Write summary to file for external monitoring
                with open('/tmp/digital_human_health.json', 'w') as f:
                    json.dump(summary, f, indent=2)
                
            except Exception as e:
                logger.error(f"Error in continuous monitoring: {e}")
            
            await asyncio.sleep(interval)

async def main():
    """Main function"""
    parser = argparse.ArgumentParser(description='Digital Human Unity MVP Health Checker')
    parser.add_argument('--backend-url', default='http://localhost:8081',
                        help='Backend API URL')
    parser.add_argument('--ws-url', default='ws://localhost:8081/ws',
                        help='WebSocket URL')
    parser.add_argument('--interval', type=int, default=30,
                        help='Check interval in seconds')
    parser.add_argument('--prometheus-port', type=int, default=8000,
                        help='Prometheus metrics port')
    parser.add_argument('--single-check', action='store_true',
                        help='Run single check and exit')
    args = parser.parse_args()
    
    # Start Prometheus metrics server
    start_http_server(args.prometheus_port)
    logger.info(f"Prometheus metrics available at http://localhost:{args.prometheus_port}/metrics")
    
    # Initialize health checker
    config = {
        'backend_url': args.backend_url,
        'ws_url': args.ws_url,
        'timeout': 10,
        'retry_count': 3
    }
    
    checker = HealthChecker(config)
    
    if args.single_check:
        # Run single check
        results = await checker.run_health_checks()
        summary = checker.get_summary()
        
        print(json.dumps(summary, indent=2))
        
        # Exit with appropriate code
        if summary['status'] == 'healthy':
            sys.exit(0)
        elif summary['status'] == 'degraded':
            sys.exit(1)
        else:
            sys.exit(2)
    else:
        # Run continuous monitoring
        await checker.continuous_monitoring(interval=args.interval)

if __name__ == '__main__':
    asyncio.run(main())