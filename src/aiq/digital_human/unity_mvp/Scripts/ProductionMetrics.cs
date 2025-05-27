using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AIQToolkit.DigitalHuman.Production
{
    /// <summary>
    /// Production performance monitoring and metrics collection
    /// Tracks system health, performance, and usage statistics
    /// </summary>
    public class ProductionMetrics : MonoBehaviour
    {
        #region Singleton Pattern
        private static ProductionMetrics _instance;
        public static ProductionMetrics Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ProductionMetrics>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ProductionMetrics");
                        _instance = go.AddComponent<ProductionMetrics>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Configuration
        [System.Serializable]
        public class MetricsConfig
        {
            public bool enabled = true;
            public float reportingInterval = 60f; // 1 minute
            public bool enablePerformanceMetrics = true;
            public bool enableNetworkMetrics = true;
            public bool enableUserMetrics = true;
            public bool enableErrorMetrics = true;
            public string metricsEndpoint = "http://localhost:8080/api/metrics";
            public Dictionary<string, string> customLabels = new Dictionary<string, string>();
        }

        private MetricsConfig config;
        #endregion

        #region Metrics Storage
        private class MetricData
        {
            public string name;
            public MetricType type;
            public double value;
            public Dictionary<string, string> labels;
            public DateTime timestamp;
        }

        private enum MetricType
        {
            Counter,
            Gauge,
            Histogram,
            Summary
        }

        // Performance metrics
        private PerformanceCounter frameRateCounter;
        private PerformanceCounter frameTimeCounter;
        private PerformanceCounter memoryUsageGauge;
        private PerformanceCounter cpuUsageGauge;
        private PerformanceCounter gpuUsageGauge;
        
        // Network metrics
        private PerformanceCounter messagesReceivedCounter;
        private PerformanceCounter messagesSentCounter;
        private PerformanceCounter bytesReceivedCounter;
        private PerformanceCounter bytesSentCounter;
        private PerformanceCounter connectionAttemptsCounter;
        private PerformanceCounter connectionFailuresCounter;
        private PerformanceCounter apiRequestsCounter;
        private PerformanceCounter apiLatencyHistogram;
        
        // User interaction metrics
        private PerformanceCounter userMessagesCounter;
        private PerformanceCounter aiResponsesCounter;
        private PerformanceCounter sessionDurationGauge;
        private PerformanceCounter emotionChangesCounter;
        private PerformanceCounter avatarLoadsCounter;
        
        // Error metrics
        private PerformanceCounter errorCounter;
        private Dictionary<string, int> errorCounts = new Dictionary<string, int>();
        
        // Audio/LipSync metrics
        private PerformanceCounter audioProcessedCounter;
        private PerformanceCounter lipSyncFramesCounter;
        private PerformanceCounter audioLatencyHistogram;
        
        // System events
        private DateTime sessionStartTime;
        private DateTime lastReportTime;
        private Queue<MetricData> metricsQueue = new Queue<MetricData>();
        private Coroutine reportingCoroutine;
        
        // Performance tracking
        private float[] frameTimeHistory = new float[60];
        private int frameTimeIndex = 0;
        private float lastFrameTime;
        #endregion

        #region Initialization
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Initialize(MetricsConfig metricsConfig)
        {
            config = metricsConfig;
            
            if (!config.enabled)
            {
                ProductionLogger.Instance.LogInfo("Metrics collection disabled");
                return;
            }
            
            ProductionLogger.Instance.LogInfo("Initializing metrics system");
            
            sessionStartTime = DateTime.UtcNow;
            lastReportTime = DateTime.UtcNow;
            
            InitializeCounters();
            
            // Start reporting coroutine
            reportingCoroutine = StartCoroutine(ReportingCoroutine());
            
            ProductionLogger.Instance.LogInfo("Metrics system initialized");
        }

        private void InitializeCounters()
        {
            // Performance metrics
            frameRateCounter = new PerformanceCounter("frame_rate", MetricType.Gauge);
            frameTimeCounter = new PerformanceCounter("frame_time_ms", MetricType.Histogram);
            memoryUsageGauge = new PerformanceCounter("memory_usage_mb", MetricType.Gauge);
            cpuUsageGauge = new PerformanceCounter("cpu_usage_percent", MetricType.Gauge);
            gpuUsageGauge = new PerformanceCounter("gpu_usage_percent", MetricType.Gauge);
            
            // Network metrics
            messagesReceivedCounter = new PerformanceCounter("messages_received_total", MetricType.Counter);
            messagesSentCounter = new PerformanceCounter("messages_sent_total", MetricType.Counter);
            bytesReceivedCounter = new PerformanceCounter("bytes_received_total", MetricType.Counter);
            bytesSentCounter = new PerformanceCounter("bytes_sent_total", MetricType.Counter);
            connectionAttemptsCounter = new PerformanceCounter("connection_attempts_total", MetricType.Counter);
            connectionFailuresCounter = new PerformanceCounter("connection_failures_total", MetricType.Counter);
            apiRequestsCounter = new PerformanceCounter("api_requests_total", MetricType.Counter);
            apiLatencyHistogram = new PerformanceCounter("api_latency_ms", MetricType.Histogram);
            
            // User interaction metrics
            userMessagesCounter = new PerformanceCounter("user_messages_total", MetricType.Counter);
            aiResponsesCounter = new PerformanceCounter("ai_responses_total", MetricType.Counter);
            sessionDurationGauge = new PerformanceCounter("session_duration_seconds", MetricType.Gauge);
            emotionChangesCounter = new PerformanceCounter("emotion_changes_total", MetricType.Counter);
            avatarLoadsCounter = new PerformanceCounter("avatar_loads_total", MetricType.Counter);
            
            // Error metrics
            errorCounter = new PerformanceCounter("errors_total", MetricType.Counter);
            
            // Audio/LipSync metrics
            audioProcessedCounter = new PerformanceCounter("audio_processed_seconds", MetricType.Counter);
            lipSyncFramesCounter = new PerformanceCounter("lipsync_frames_total", MetricType.Counter);
            audioLatencyHistogram = new PerformanceCounter("audio_latency_ms", MetricType.Histogram);
        }
        #endregion

        #region Performance Metrics
        private void Update()
        {
            if (!config.enabled || !config.enablePerformanceMetrics)
                return;
            
            // Track frame time
            float frameTime = Time.deltaTime * 1000f; // Convert to milliseconds
            frameTimeHistory[frameTimeIndex] = frameTime;
            frameTimeIndex = (frameTimeIndex + 1) % frameTimeHistory.Length;
            
            // Update frame metrics
            frameRateCounter.Set(1f / Time.deltaTime);
            frameTimeCounter.Observe(frameTime);
            
            // Update memory usage
            long totalMemory = GC.GetTotalMemory(false);
            memoryUsageGauge.Set(totalMemory / (1024f * 1024f)); // Convert to MB
            
            // Update session duration
            float sessionDuration = (float)(DateTime.UtcNow - sessionStartTime).TotalSeconds;
            sessionDurationGauge.Set(sessionDuration);
        }

        public void LogFrameMetrics(float cpuTime, float gpuTime)
        {
            cpuUsageGauge.Set(cpuTime);
            gpuUsageGauge.Set(gpuTime);
        }
        #endregion

        #region Network Metrics
        public void LogMessageReceived()
        {
            if (config.enableNetworkMetrics)
                messagesReceivedCounter.Increment();
        }

        public void LogMessageSent()
        {
            if (config.enableNetworkMetrics)
                messagesSentCounter.Increment();
        }

        public void LogBytesReceived(int bytes)
        {
            if (config.enableNetworkMetrics)
                bytesReceivedCounter.Increment(bytes);
        }

        public void LogBytesSent(int bytes)
        {
            if (config.enableNetworkMetrics)
                bytesSentCounter.Increment(bytes);
        }

        public void LogConnectionAttempt()
        {
            if (config.enableNetworkMetrics)
                connectionAttemptsCounter.Increment();
        }

        public void LogConnectionFailure()
        {
            if (config.enableNetworkMetrics)
                connectionFailuresCounter.Increment();
        }

        public void LogApiRequest(string method, string endpoint, bool success)
        {
            if (!config.enableNetworkMetrics)
                return;
            
            var labels = new Dictionary<string, string>
            {
                ["method"] = method,
                ["endpoint"] = endpoint,
                ["status"] = success ? "success" : "failure"
            };
            
            apiRequestsCounter.Increment(labels);
        }

        public void LogApiLatency(float latencyMs)
        {
            if (config.enableNetworkMetrics)
                apiLatencyHistogram.Observe(latencyMs);
        }

        public void LogBackendStatus(object statusUpdate)
        {
            // Log backend status metrics
            // Implementation depends on status update structure
        }
        #endregion

        #region User Interaction Metrics
        public void LogUserMessage()
        {
            if (config.enableUserMetrics)
                userMessagesCounter.Increment();
        }

        public void LogAiResponse()
        {
            if (config.enableUserMetrics)
                aiResponsesCounter.Increment();
        }

        public void LogEmotionChange(string emotion)
        {
            if (!config.enableUserMetrics)
                return;
            
            var labels = new Dictionary<string, string>
            {
                ["emotion"] = emotion
            };
            
            emotionChangesCounter.Increment(labels);
        }

        public void LogAvatarLoad(bool success)
        {
            if (!config.enableUserMetrics)
                return;
            
            var labels = new Dictionary<string, string>
            {
                ["status"] = success ? "success" : "failure"
            };
            
            avatarLoadsCounter.Increment(labels);
        }

        public void LogMessageAdded(bool isUser)
        {
            if (!config.enableUserMetrics)
                return;
            
            if (isUser)
                LogUserMessage();
            else
                LogAiResponse();
        }
        #endregion

        #region Error Metrics
        public void LogError(string component, string error)
        {
            if (!config.enableErrorMetrics)
                return;
            
            var labels = new Dictionary<string, string>
            {
                ["component"] = component,
                ["error_type"] = GetErrorType(error)
            };
            
            errorCounter.Increment(labels);
            
            // Track error counts by type
            string errorType = GetErrorType(error);
            if (!errorCounts.ContainsKey(errorType))
                errorCounts[errorType] = 0;
            errorCounts[errorType]++;
        }

        private string GetErrorType(string error)
        {
            if (error.Contains("connection", StringComparison.OrdinalIgnoreCase))
                return "connection_error";
            if (error.Contains("timeout", StringComparison.OrdinalIgnoreCase))
                return "timeout_error";
            if (error.Contains("auth", StringComparison.OrdinalIgnoreCase))
                return "auth_error";
            if (error.Contains("parse", StringComparison.OrdinalIgnoreCase) || 
                error.Contains("json", StringComparison.OrdinalIgnoreCase))
                return "parse_error";
            
            return "unknown_error";
        }
        #endregion

        #region Audio/LipSync Metrics
        public void LogAudioReceived(float duration)
        {
            if (config.enablePerformanceMetrics)
                audioProcessedCounter.Increment(duration);
        }

        public void LogAudioProcessed(int samples)
        {
            if (config.enablePerformanceMetrics)
            {
                float duration = samples / 44100f; // Assuming 44.1kHz sample rate
                audioProcessedCounter.Increment(duration);
            }
        }

        public void LogLipSyncStarted()
        {
            // Start tracking lip sync performance
        }

        public void LogLipSyncStopped()
        {
            // Stop tracking lip sync performance
        }

        public void LogLipSyncFrame()
        {
            if (config.enablePerformanceMetrics)
                lipSyncFramesCounter.Increment();
        }

        public void LogAudioLatency(float latencyMs)
        {
            if (config.enablePerformanceMetrics)
                audioLatencyHistogram.Observe(latencyMs);
        }
        #endregion

        #region System Events
        public void LogInitializationTime(float timeSeconds)
        {
            var metric = new MetricData
            {
                name = "initialization_time_seconds",
                type = MetricType.Gauge,
                value = timeSeconds,
                timestamp = DateTime.UtcNow
            };
            
            QueueMetric(metric);
        }

        public void LogSystemStateChange(ProductionMVP.SystemState state)
        {
            var labels = new Dictionary<string, string>
            {
                ["state"] = state.ToString()
            };
            
            var metric = new MetricData
            {
                name = "system_state",
                type = MetricType.Gauge,
                value = (int)state,
                labels = labels,
                timestamp = DateTime.UtcNow
            };
            
            QueueMetric(metric);
        }

        public void LogFocusLost()
        {
            LogEvent("app_focus_lost");
        }

        public void LogFocusGained()
        {
            LogEvent("app_focus_gained");
        }

        public void PauseMetrics()
        {
            LogEvent("metrics_paused");
        }

        public void ResumeMetrics()
        {
            LogEvent("metrics_resumed");
        }

        private void LogEvent(string eventName)
        {
            var metric = new MetricData
            {
                name = "system_events_total",
                type = MetricType.Counter,
                value = 1,
                labels = new Dictionary<string, string> { ["event"] = eventName },
                timestamp = DateTime.UtcNow
            };
            
            QueueMetric(metric);
        }
        #endregion

        #region Avatar Metrics
        public void StartAvatarLoad()
        {
            // Start timing avatar load
        }

        public void EndAvatarLoad(bool success)
        {
            LogAvatarLoad(success);
        }

        public void LogMessageProcessed()
        {
            // Log message processing completion
        }
        #endregion

        #region Reporting
        private IEnumerator ReportingCoroutine()
        {
            while (config.enabled)
            {
                yield return new WaitForSeconds(config.reportingInterval);
                
                if (metricsQueue.Count > 0)
                {
                    yield return SendMetrics();
                }
            }
        }

        private IEnumerator SendMetrics()
        {
            List<MetricData> metricsToSend = new List<MetricData>();
            
            // Collect current counter values
            CollectCounterMetrics(metricsToSend);
            
            // Add queued metrics
            while (metricsQueue.Count > 0)
            {
                metricsToSend.Add(metricsQueue.Dequeue());
            }
            
            if (metricsToSend.Count == 0)
                yield break;
            
            // Prepare JSON payload
            var payload = new
            {
                timestamp = DateTime.UtcNow.ToString("O"),
                metrics = metricsToSend,
                labels = config.customLabels
            };
            
            string json = JsonUtility.ToJson(payload);
            
            // Send to metrics endpoint
            using (UnityWebRequest request = new UnityWebRequest(config.metricsEndpoint, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                
                yield return request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    ProductionLogger.Instance.LogWarning($"Failed to send metrics: {request.error}");
                }
                else
                {
                    ProductionLogger.Instance.LogDebug($"Sent {metricsToSend.Count} metrics");
                }
            }
            
            lastReportTime = DateTime.UtcNow;
        }

        private void CollectCounterMetrics(List<MetricData> metrics)
        {
            // Add all counter values
            AddCounterMetric(metrics, frameRateCounter);
            AddCounterMetric(metrics, frameTimeCounter);
            AddCounterMetric(metrics, memoryUsageGauge);
            AddCounterMetric(metrics, cpuUsageGauge);
            AddCounterMetric(metrics, gpuUsageGauge);
            
            AddCounterMetric(metrics, messagesReceivedCounter);
            AddCounterMetric(metrics, messagesSentCounter);
            AddCounterMetric(metrics, bytesReceivedCounter);
            AddCounterMetric(metrics, bytesSentCounter);
            AddCounterMetric(metrics, connectionAttemptsCounter);
            AddCounterMetric(metrics, connectionFailuresCounter);
            AddCounterMetric(metrics, apiRequestsCounter);
            AddCounterMetric(metrics, apiLatencyHistogram);
            
            AddCounterMetric(metrics, userMessagesCounter);
            AddCounterMetric(metrics, aiResponsesCounter);
            AddCounterMetric(metrics, sessionDurationGauge);
            AddCounterMetric(metrics, emotionChangesCounter);
            AddCounterMetric(metrics, avatarLoadsCounter);
            
            AddCounterMetric(metrics, errorCounter);
            
            AddCounterMetric(metrics, audioProcessedCounter);
            AddCounterMetric(metrics, lipSyncFramesCounter);
            AddCounterMetric(metrics, audioLatencyHistogram);
        }

        private void AddCounterMetric(List<MetricData> metrics, PerformanceCounter counter)
        {
            if (counter.GetValue() <= 0)
                return;
            
            metrics.Add(new MetricData
            {
                name = counter.Name,
                type = counter.Type,
                value = counter.GetValue(),
                labels = counter.Labels,
                timestamp = DateTime.UtcNow
            });
        }

        private void QueueMetric(MetricData metric)
        {
            metricsQueue.Enqueue(metric);
        }
        #endregion

        #region Utility
        public void Shutdown()
        {
            if (reportingCoroutine != null)
            {
                StopCoroutine(reportingCoroutine);
                reportingCoroutine = null;
            }
            
            // Send final metrics
            StartCoroutine(SendMetrics());
            
            ProductionLogger.Instance.LogInfo("Metrics system shutdown");
        }

        public string GetMetricsSummary()
        {
            return $"Session Duration: {(DateTime.UtcNow - sessionStartTime).TotalMinutes:F1} minutes\n" +
                   $"Messages Sent: {messagesSentCounter.GetValue()}\n" +
                   $"Messages Received: {messagesReceivedCounter.GetValue()}\n" +
                   $"Average FPS: {GetAverageFrameRate():F1}\n" +
                   $"Memory Usage: {memoryUsageGauge.GetValue():F1} MB\n" +
                   $"Total Errors: {errorCounter.GetValue()}";
        }

        private float GetAverageFrameRate()
        {
            float totalFrameTime = 0;
            for (int i = 0; i < frameTimeHistory.Length; i++)
            {
                totalFrameTime += frameTimeHistory[i];
            }
            
            float avgFrameTime = totalFrameTime / frameTimeHistory.Length;
            return avgFrameTime > 0 ? 1000f / avgFrameTime : 0;
        }
        #endregion

        #region Performance Counter Class
        private class PerformanceCounter
        {
            public string Name { get; private set; }
            public MetricType Type { get; private set; }
            public Dictionary<string, string> Labels { get; private set; }
            
            private double value;
            private List<double> histogram = new List<double>();
            
            public PerformanceCounter(string name, MetricType type)
            {
                Name = name;
                Type = type;
                Labels = new Dictionary<string, string>();
            }
            
            public void Set(double val)
            {
                value = val;
            }
            
            public void Increment(double amount = 1, Dictionary<string, string> labels = null)
            {
                value += amount;
                if (labels != null)
                    Labels = labels;
            }
            
            public void Observe(double val)
            {
                histogram.Add(val);
            }
            
            public double GetValue()
            {
                switch (Type)
                {
                    case MetricType.Histogram:
                    case MetricType.Summary:
                        return histogram.Count > 0 ? GetAverage() : 0;
                    default:
                        return value;
                }
            }
            
            private double GetAverage()
            {
                if (histogram.Count == 0)
                    return 0;
                
                double sum = 0;
                foreach (var val in histogram)
                    sum += val;
                
                return sum / histogram.Count;
            }
        }
        #endregion
    }
}