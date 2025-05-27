using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace AIQToolkit.DigitalHuman.Production
{
    /// <summary>
    /// Structured logging system for production use
    /// Supports multiple log targets and configurable log levels
    /// </summary>
    public class ProductionLogger : MonoBehaviour
    {
        #region Singleton Pattern
        private static ProductionLogger _instance;
        public static ProductionLogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ProductionLogger>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ProductionLogger");
                        _instance = go.AddComponent<ProductionLogger>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Configuration
        [System.Serializable]
        public class LoggingConfig
        {
            public LogLevel minLogLevel = LogLevel.Info;
            public bool enableFileLogging = true;
            public string logFilePath = "Logs/production.log";
            public int maxLogFileSize = 10485760; // 10MB
            public int maxLogFiles = 5;
            public bool enableConsoleLogging = true;
            public bool enableRemoteLogging = false;
            public string remoteLoggingUrl = "";
            public bool includeStackTrace = true;
            public bool includeTimestamp = true;
        }

        [System.Serializable]
        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
            Critical = 4
        }

        private LoggingConfig config = new LoggingConfig();
        #endregion

        #region Log Management
        private Queue<LogEntry> logQueue = new Queue<LogEntry>();
        private StreamWriter fileWriter;
        private string currentLogFile;
        private long currentFileSize = 0;
        private object logLock = new object();
        private bool isInitialized = false;
        
        [System.Serializable]
        private class LogEntry
        {
            public DateTime timestamp;
            public LogLevel level;
            public string message;
            public string component;
            public string stackTrace;
            public Dictionary<string, object> metadata;
        }
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

        public void Initialize(LoggingConfig loggingConfig)
        {
            config = loggingConfig;
            
            if (config.enableFileLogging)
            {
                InitializeFileLogging();
            }
            
            // Subscribe to Unity's log messages
            Application.logMessageReceived += HandleUnityLog;
            
            isInitialized = true;
            LogInfo("Production logger initialized");
        }

        private void InitializeFileLogging()
        {
            try
            {
                string logDirectory = Path.GetDirectoryName(GetFullLogPath());
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                
                currentLogFile = GetCurrentLogFileName();
                string fullPath = GetFullLogPath();
                
                // Check if we need to rotate logs
                if (File.Exists(fullPath))
                {
                    FileInfo fileInfo = new FileInfo(fullPath);
                    currentFileSize = fileInfo.Length;
                    
                    if (currentFileSize >= config.maxLogFileSize)
                    {
                        RotateLogs();
                    }
                }
                
                fileWriter = new StreamWriter(fullPath, true, Encoding.UTF8);
                fileWriter.AutoFlush = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize file logging: {e.Message}");
                config.enableFileLogging = false;
            }
        }

        private string GetFullLogPath()
        {
            return Path.Combine(Application.persistentDataPath, config.logFilePath);
        }

        private string GetCurrentLogFileName()
        {
            return Path.GetFileNameWithoutExtension(config.logFilePath) + Path.GetExtension(config.logFilePath);
        }
        #endregion

        #region Logging Methods
        public void LogDebug(string message, string component = "System", Dictionary<string, object> metadata = null)
        {
            Log(LogLevel.Debug, message, component, metadata);
        }

        public void LogInfo(string message, string component = "System", Dictionary<string, object> metadata = null)
        {
            Log(LogLevel.Info, message, component, metadata);
        }

        public void LogWarning(string message, string component = "System", Dictionary<string, object> metadata = null)
        {
            Log(LogLevel.Warning, message, component, metadata);
        }

        public void LogError(string message, string component = "System", Dictionary<string, object> metadata = null)
        {
            Log(LogLevel.Error, message, component, metadata);
        }

        public void LogCritical(string message, string component = "System", Dictionary<string, object> metadata = null)
        {
            Log(LogLevel.Critical, message, component, metadata);
        }

        public void LogException(Exception exception, string component = "System", Dictionary<string, object> metadata = null)
        {
            var exceptionMetadata = new Dictionary<string, object>(metadata ?? new Dictionary<string, object>())
            {
                ["exception_type"] = exception.GetType().Name,
                ["exception_message"] = exception.Message,
                ["exception_stacktrace"] = exception.StackTrace
            };
            
            LogError($"Exception occurred: {exception.Message}", component, exceptionMetadata);
        }

        private void Log(LogLevel level, string message, string component, Dictionary<string, object> metadata)
        {
            if (!isInitialized || level < config.minLogLevel)
                return;
            
            var entry = new LogEntry
            {
                timestamp = DateTime.UtcNow,
                level = level,
                message = message,
                component = component,
                metadata = metadata ?? new Dictionary<string, object>()
            };
            
            if (config.includeStackTrace && (level >= LogLevel.Error))
            {
                entry.stackTrace = Environment.StackTrace;
            }
            
            lock (logLock)
            {
                logQueue.Enqueue(entry);
            }
            
            ProcessLogEntry(entry);
        }

        private void ProcessLogEntry(LogEntry entry)
        {
            // Console logging
            if (config.enableConsoleLogging)
            {
                LogToConsole(entry);
            }
            
            // File logging
            if (config.enableFileLogging)
            {
                LogToFile(entry);
            }
            
            // Remote logging
            if (config.enableRemoteLogging)
            {
                LogToRemote(entry);
            }
        }

        private void LogToConsole(LogEntry entry)
        {
            string formattedMessage = FormatLogEntry(entry);
            
            switch (entry.level)
            {
                case LogLevel.Debug:
                    Debug.Log(formattedMessage);
                    break;
                case LogLevel.Info:
                    Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    Debug.LogError(formattedMessage);
                    break;
            }
        }

        private void LogToFile(LogEntry entry)
        {
            if (fileWriter == null)
                return;
            
            try
            {
                string formattedMessage = FormatLogEntry(entry, true);
                
                lock (logLock)
                {
                    fileWriter.WriteLine(formattedMessage);
                    currentFileSize += Encoding.UTF8.GetByteCount(formattedMessage + Environment.NewLine);
                }
                
                // Check if we need to rotate logs
                if (currentFileSize >= config.maxLogFileSize)
                {
                    RotateLogs();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to log file: {e.Message}");
            }
        }

        private void LogToRemote(LogEntry entry)
        {
            // Queue for remote logging
            // This would typically send logs to a remote logging service
            // Implementation depends on the specific service being used
        }

        private string FormatLogEntry(LogEntry entry, bool includeMetadata = false)
        {
            StringBuilder sb = new StringBuilder();
            
            if (config.includeTimestamp)
            {
                sb.Append($"[{entry.timestamp:yyyy-MM-dd HH:mm:ss.fff}] ");
            }
            
            sb.Append($"[{entry.level}] ");
            sb.Append($"[{entry.component}] ");
            sb.Append(entry.message);
            
            if (includeMetadata && entry.metadata.Count > 0)
            {
                sb.Append(" | Metadata: ");
                foreach (var kvp in entry.metadata)
                {
                    sb.Append($"{kvp.Key}={kvp.Value} ");
                }
            }
            
            if (!string.IsNullOrEmpty(entry.stackTrace))
            {
                sb.Append("\nStack Trace:\n");
                sb.Append(entry.stackTrace);
            }
            
            return sb.ToString();
        }
        #endregion

        #region Log Rotation
        private void RotateLogs()
        {
            try
            {
                lock (logLock)
                {
                    if (fileWriter != null)
                    {
                        fileWriter.Close();
                        fileWriter.Dispose();
                    }
                    
                    // Rotate existing log files
                    string logDirectory = Path.GetDirectoryName(GetFullLogPath());
                    string baseFileName = Path.GetFileNameWithoutExtension(config.logFilePath);
                    string extension = Path.GetExtension(config.logFilePath);
                    
                    // Delete oldest log if we're at max
                    string oldestLog = Path.Combine(logDirectory, $"{baseFileName}.{config.maxLogFiles - 1}{extension}");
                    if (File.Exists(oldestLog))
                    {
                        File.Delete(oldestLog);
                    }
                    
                    // Rotate existing logs
                    for (int i = config.maxLogFiles - 2; i >= 0; i--)
                    {
                        string currentFile = i == 0 ? 
                            GetFullLogPath() : 
                            Path.Combine(logDirectory, $"{baseFileName}.{i}{extension}");
                            
                        string newFile = Path.Combine(logDirectory, $"{baseFileName}.{i + 1}{extension}");
                        
                        if (File.Exists(currentFile))
                        {
                            File.Move(currentFile, newFile);
                        }
                    }
                    
                    // Create new log file
                    fileWriter = new StreamWriter(GetFullLogPath(), false, Encoding.UTF8);
                    fileWriter.AutoFlush = true;
                    currentFileSize = 0;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to rotate logs: {e.Message}");
            }
        }
        #endregion

        #region Unity Log Handler
        private void HandleUnityLog(string logString, string stackTrace, LogType type)
        {
            // Skip our own logs to avoid recursion
            if (logString.Contains("[ProductionLogger]"))
                return;
            
            LogLevel level = LogLevel.Info;
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    level = LogLevel.Error;
                    break;
                case LogType.Warning:
                    level = LogLevel.Warning;
                    break;
                case LogType.Log:
                    level = LogLevel.Info;
                    break;
            }
            
            var metadata = new Dictionary<string, object>
            {
                ["unity_log_type"] = type.ToString()
            };
            
            Log(level, logString, "Unity", metadata);
        }
        #endregion

        #region Special Logging Methods
        public void LogSystemStateChange(ProductionMVP.SystemState state)
        {
            var metadata = new Dictionary<string, object>
            {
                ["system_state"] = state.ToString()
            };
            
            LogInfo($"System state changed to: {state}", "SystemState", metadata);
        }

        public void LogPerformanceMetric(string metric, double value)
        {
            var metadata = new Dictionary<string, object>
            {
                ["metric"] = metric,
                ["value"] = value
            };
            
            LogInfo($"Performance metric - {metric}: {value}", "Performance", metadata);
        }

        public void LogNetworkEvent(string eventType, string endpoint, bool success)
        {
            var metadata = new Dictionary<string, object>
            {
                ["event_type"] = eventType,
                ["endpoint"] = endpoint,
                ["success"] = success
            };
            
            LogInfo($"Network event - {eventType} to {endpoint}: {(success ? "Success" : "Failed")}", "Network", metadata);
        }

        public void LogUserInteraction(string action, Dictionary<string, object> details = null)
        {
            var metadata = new Dictionary<string, object>(details ?? new Dictionary<string, object>())
            {
                ["action"] = action
            };
            
            LogInfo($"User interaction: {action}", "UserInteraction", metadata);
        }
        #endregion

        #region Cleanup
        public void Shutdown()
        {
            LogInfo("Shutting down logger");
            
            // Flush any remaining logs
            lock (logLock)
            {
                while (logQueue.Count > 0)
                {
                    ProcessLogEntry(logQueue.Dequeue());
                }
            }
            
            // Close file writer
            if (fileWriter != null)
            {
                fileWriter.Flush();
                fileWriter.Close();
                fileWriter.Dispose();
                fileWriter = null;
            }
            
            // Unsubscribe from Unity logs
            Application.logMessageReceived -= HandleUnityLog;
            
            isInitialized = false;
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Flush logs when app is paused
                if (fileWriter != null)
                {
                    fileWriter.Flush();
                }
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // Flush logs when app loses focus
                if (fileWriter != null)
                {
                    fileWriter.Flush();
                }
            }
        }
        #endregion

        #region Log Analysis
        public List<LogEntry> GetRecentLogs(int count = 100)
        {
            lock (logLock)
            {
                return new List<LogEntry>(logQueue.ToArray()).GetRange(0, Math.Min(count, logQueue.Count));
            }
        }

        public Dictionary<LogLevel, int> GetLogStatistics()
        {
            var stats = new Dictionary<LogLevel, int>();
            foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)))
            {
                stats[level] = 0;
            }
            
            lock (logLock)
            {
                foreach (var entry in logQueue)
                {
                    stats[entry.level]++;
                }
            }
            
            return stats;
        }
        #endregion
    }
}