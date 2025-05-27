using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AIQToolkit.DigitalHuman.Production
{
    /// <summary>
    /// Production configuration management system
    /// Handles environment-specific settings and runtime configuration
    /// </summary>
    public class ProductionConfig : MonoBehaviour
    {
        #region Configuration Classes
        [System.Serializable]
        public class BackendConfig
        {
            public string webSocketUrl = "ws://localhost:8080/ws";
            public string restApiUrl = "http://localhost:8080/api";
            public float connectionTimeout = 10f;
            public float reconnectDelay = 5f;
            public int maxReconnectAttempts = 5;
            public bool enableHeartbeat = true;
            public float heartbeatInterval = 30f;
            public Dictionary<string, string> headers = new Dictionary<string, string>();
            public AuthConfig auth = new AuthConfig();
        }

        [System.Serializable]
        public class AuthConfig
        {
            public bool enabled = false;
            public string authToken = "";
            public string apiKey = "";
            public string refreshToken = "";
            public float tokenRefreshInterval = 3600f; // 1 hour
        }

        [System.Serializable]
        public class AvatarConfig
        {
            public string defaultAvatarUrl = "https://api.readyplayer.me/v1/avatars/default.glb";
            public float loadTimeout = 30f;
            public bool enableCaching = true;
            public int maxCacheSize = 5;
            public ProductionAvatarManager.AvatarQuality quality = ProductionAvatarManager.AvatarQuality.High;
            public bool enableLOD = true;
            public float[] lodDistances = { 5f, 10f, 20f };
        }

        [System.Serializable]
        public class LipSyncConfig
        {
            public float volumeMultiplier = 1.0f;
            public float smoothing = 0.1f;
            public bool enableNoiseGate = true;
            public float noiseGateThreshold = 0.01f;
            public bool enableVisemeSmoothing = true;
            public float visemeSmoothTime = 0.1f;
            public ProductionLipSync.AudioProcessingMode processingMode = ProductionLipSync.AudioProcessingMode.Realtime;
            public int audioBufferSize = 2048;
            public float updateInterval = 0.016f;
        }

        [System.Serializable]
        public class UIConfig
        {
            public Color userMessageColor = new Color(0.2f, 0.5f, 1f);
            public Color aiMessageColor = new Color(0.3f, 0.8f, 0.3f);
            public Color systemMessageColor = new Color(0.8f, 0.8f, 0.8f);
            public float messageAnimationDuration = 0.3f;
            public int maxVisibleMessages = 50;
            public bool enableMessageAnimation = true;
            public bool enableTypingIndicator = true;
            public float typingIndicatorDelay = 0.5f;
            public bool enableAutoScroll = true;
            public float autoScrollSpeed = 5f;
        }

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

        [System.Serializable]
        public class PerformanceConfig
        {
            public int targetFrameRate = 60;
            public bool enableVSync = true;
            public int vSyncCount = 1;
            public QualityLevel qualityLevel = QualityLevel.High;
            public bool enableDynamicQuality = true;
            public float minFrameTime = 0.016f; // 60 FPS
            public float maxFrameTime = 0.033f; // 30 FPS
        }

        [System.Serializable]
        public enum QualityLevel
        {
            Low = 0,
            Medium = 1,
            High = 2,
            Ultra = 3
        }

        [System.Serializable]
        public class EnvironmentConfig
        {
            public Environment environment = Environment.Development;
            public string configPath = "Config/";
            public bool overrideWithEnvironmentVariables = true;
            public Dictionary<string, string> customSettings = new Dictionary<string, string>();
        }

        [System.Serializable]
        public enum Environment
        {
            Development,
            Staging,
            Production
        }
        #endregion

        #region Properties
        public BackendConfig Backend { get; private set; }
        public AvatarConfig Avatar { get; private set; }
        public LipSyncConfig LipSync { get; private set; }
        public UIConfig UI { get; private set; }
        public LoggingConfig Logging { get; private set; }
        public MetricsConfig Metrics { get; private set; }
        public PerformanceConfig Performance { get; private set; }
        public EnvironmentConfig Environment { get; private set; }
        #endregion

        #region Configuration Loading
        [Header("Configuration Settings")]
        [SerializeField] private EnvironmentConfig environmentConfig = new EnvironmentConfig();
        [SerializeField] private bool loadFromFile = true;
        [SerializeField] private string configFileName = "production_config.json";
        
        // Default configurations
        [Header("Default Configurations")]
        [SerializeField] private BackendConfig defaultBackendConfig = new BackendConfig();
        [SerializeField] private AvatarConfig defaultAvatarConfig = new AvatarConfig();
        [SerializeField] private LipSyncConfig defaultLipSyncConfig = new LipSyncConfig();
        [SerializeField] private UIConfig defaultUIConfig = new UIConfig();
        [SerializeField] private LoggingConfig defaultLoggingConfig = new LoggingConfig();
        [SerializeField] private MetricsConfig defaultMetricsConfig = new MetricsConfig();
        [SerializeField] private PerformanceConfig defaultPerformanceConfig = new PerformanceConfig();

        private Dictionary<string, object> configCache = new Dictionary<string, object>();
        #endregion

        #region Initialization
        public bool LoadConfiguration()
        {
            try
            {
                ProductionLogger.Instance.LogInfo("Loading production configuration");
                
                // Start with defaults
                Backend = CloneConfig(defaultBackendConfig);
                Avatar = CloneConfig(defaultAvatarConfig);
                LipSync = CloneConfig(defaultLipSyncConfig);
                UI = CloneConfig(defaultUIConfig);
                Logging = CloneConfig(defaultLoggingConfig);
                Metrics = CloneConfig(defaultMetricsConfig);
                Performance = CloneConfig(defaultPerformanceConfig);
                Environment = CloneConfig(environmentConfig);
                
                // Load from file if enabled
                if (loadFromFile)
                {
                    LoadFromFile();
                }
                
                // Override with environment variables
                if (Environment.overrideWithEnvironmentVariables)
                {
                    LoadFromEnvironmentVariables();
                }
                
                // Apply environment-specific settings
                ApplyEnvironmentSettings();
                
                // Validate configuration
                if (!ValidateConfiguration())
                {
                    ProductionLogger.Instance.LogError("Configuration validation failed");
                    return false;
                }
                
                // Apply performance settings
                ApplyPerformanceSettings();
                
                ProductionLogger.Instance.LogInfo("Configuration loaded successfully");
                return true;
            }
            catch (Exception e)
            {
                ProductionLogger.Instance.LogError($"Failed to load configuration: {e.Message}");
                return false;
            }
        }

        private void LoadFromFile()
        {
            string configPath = Path.Combine(Application.persistentDataPath, Environment.configPath, configFileName);
            
            if (!File.Exists(configPath))
            {
                // Try loading from Resources
                configPath = Path.Combine(Application.streamingAssetsPath, Environment.configPath, configFileName);
            }
            
            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    ConfigurationData data = JsonUtility.FromJson<ConfigurationData>(json);
                    
                    if (data.backend != null) Backend = data.backend;
                    if (data.avatar != null) Avatar = data.avatar;
                    if (data.lipSync != null) LipSync = data.lipSync;
                    if (data.ui != null) UI = data.ui;
                    if (data.logging != null) Logging = data.logging;
                    if (data.metrics != null) Metrics = data.metrics;
                    if (data.performance != null) Performance = data.performance;
                    
                    ProductionLogger.Instance.LogInfo($"Configuration loaded from file: {configPath}");
                }
                catch (Exception e)
                {
                    ProductionLogger.Instance.LogWarning($"Failed to load configuration file: {e.Message}");
                }
            }
            else
            {
                ProductionLogger.Instance.LogWarning($"Configuration file not found: {configPath}");
            }
        }

        private void LoadFromEnvironmentVariables()
        {
            // Backend configuration
            string wsUrl = System.Environment.GetEnvironmentVariable("DIGITAL_HUMAN_WS_URL");
            if (!string.IsNullOrEmpty(wsUrl))
                Backend.webSocketUrl = wsUrl;
            
            string apiUrl = System.Environment.GetEnvironmentVariable("DIGITAL_HUMAN_API_URL");
            if (!string.IsNullOrEmpty(apiUrl))
                Backend.restApiUrl = apiUrl;
            
            string authToken = System.Environment.GetEnvironmentVariable("DIGITAL_HUMAN_AUTH_TOKEN");
            if (!string.IsNullOrEmpty(authToken))
            {
                Backend.auth.enabled = true;
                Backend.auth.authToken = authToken;
            }
            
            string apiKey = System.Environment.GetEnvironmentVariable("DIGITAL_HUMAN_API_KEY");
            if (!string.IsNullOrEmpty(apiKey))
            {
                Backend.auth.apiKey = apiKey;
            }
            
            // Avatar configuration
            string avatarUrl = System.Environment.GetEnvironmentVariable("DIGITAL_HUMAN_AVATAR_URL");
            if (!string.IsNullOrEmpty(avatarUrl))
                Avatar.defaultAvatarUrl = avatarUrl;
            
            // Environment
            string env = System.Environment.GetEnvironmentVariable("DIGITAL_HUMAN_ENVIRONMENT");
            if (!string.IsNullOrEmpty(env))
            {
                if (Enum.TryParse<Environment>(env, true, out Environment environment))
                {
                    Environment.environment = environment;
                }
            }
            
            ProductionLogger.Instance.LogInfo("Environment variables loaded");
        }

        private void ApplyEnvironmentSettings()
        {
            switch (Environment.environment)
            {
                case Environment.Development:
                    Logging.minLogLevel = LogLevel.Debug;
                    Logging.enableConsoleLogging = true;
                    Metrics.reportingInterval = 10f; // More frequent in dev
                    Performance.targetFrameRate = 30; // Lower for development
                    break;
                    
                case Environment.Staging:
                    Logging.minLogLevel = LogLevel.Info;
                    Logging.enableRemoteLogging = true;
                    Metrics.reportingInterval = 30f;
                    Performance.targetFrameRate = 60;
                    break;
                    
                case Environment.Production:
                    Logging.minLogLevel = LogLevel.Warning;
                    Logging.enableRemoteLogging = true;
                    Logging.enableFileLogging = true;
                    Metrics.reportingInterval = 60f;
                    Performance.targetFrameRate = 60;
                    Performance.enableDynamicQuality = true;
                    break;
            }
            
            ProductionLogger.Instance.LogInfo($"Applied {Environment.environment} environment settings");
        }

        private void ApplyPerformanceSettings()
        {
            Application.targetFrameRate = Performance.targetFrameRate;
            QualitySettings.vSyncCount = Performance.enableVSync ? Performance.vSyncCount : 0;
            
            // Apply quality level
            switch (Performance.qualityLevel)
            {
                case QualityLevel.Low:
                    QualitySettings.SetQualityLevel(0);
                    break;
                case QualityLevel.Medium:
                    QualitySettings.SetQualityLevel(1);
                    break;
                case QualityLevel.High:
                    QualitySettings.SetQualityLevel(2);
                    break;
                case QualityLevel.Ultra:
                    QualitySettings.SetQualityLevel(3);
                    break;
            }
            
            ProductionLogger.Instance.LogInfo($"Applied performance settings: {Performance.qualityLevel} quality, {Performance.targetFrameRate} FPS target");
        }
        #endregion

        #region Validation
        private bool ValidateConfiguration()
        {
            bool isValid = true;
            
            // Validate Backend
            if (string.IsNullOrEmpty(Backend.webSocketUrl))
            {
                ProductionLogger.Instance.LogError("WebSocket URL is required");
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(Backend.restApiUrl))
            {
                ProductionLogger.Instance.LogError("REST API URL is required");
                isValid = false;
            }
            
            // Validate Avatar
            if (string.IsNullOrEmpty(Avatar.defaultAvatarUrl))
            {
                ProductionLogger.Instance.LogError("Default avatar URL is required");
                isValid = false;
            }
            
            // Validate Performance
            if (Performance.targetFrameRate < 15 || Performance.targetFrameRate > 120)
            {
                ProductionLogger.Instance.LogWarning($"Unusual target frame rate: {Performance.targetFrameRate}");
            }
            
            return isValid;
        }
        #endregion

        #region Runtime Configuration
        public void UpdateSetting(string key, object value)
        {
            configCache[key] = value;
            ProductionLogger.Instance.LogInfo($"Updated configuration: {key} = {value}");
            
            // Apply specific settings immediately
            switch (key)
            {
                case "performance.targetFrameRate":
                    if (value is int frameRate)
                    {
                        Performance.targetFrameRate = frameRate;
                        Application.targetFrameRate = frameRate;
                    }
                    break;
                    
                case "backend.webSocketUrl":
                    if (value is string url)
                    {
                        Backend.webSocketUrl = url;
                        // Trigger reconnection if needed
                    }
                    break;
            }
        }

        public T GetSetting<T>(string key, T defaultValue = default)
        {
            if (configCache.ContainsKey(key))
            {
                try
                {
                    return (T)configCache[key];
                }
                catch
                {
                    return defaultValue;
                }
            }
            
            return defaultValue;
        }

        public void SaveConfiguration()
        {
            try
            {
                ConfigurationData data = new ConfigurationData
                {
                    backend = Backend,
                    avatar = Avatar,
                    lipSync = LipSync,
                    ui = UI,
                    logging = Logging,
                    metrics = Metrics,
                    performance = Performance
                };
                
                string json = JsonUtility.ToJson(data, true);
                string configPath = Path.Combine(Application.persistentDataPath, Environment.configPath, configFileName);
                
                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(configPath));
                
                File.WriteAllText(configPath, json);
                ProductionLogger.Instance.LogInfo($"Configuration saved to: {configPath}");
            }
            catch (Exception e)
            {
                ProductionLogger.Instance.LogError($"Failed to save configuration: {e.Message}");
            }
        }
        #endregion

        #region Utility Methods
        private T CloneConfig<T>(T source) where T : new()
        {
            string json = JsonUtility.ToJson(source);
            return JsonUtility.FromJson<T>(json);
        }

        public void RefreshConfiguration()
        {
            LoadConfiguration();
        }

        public string GetConfigurationSummary()
        {
            return $"Environment: {Environment.environment}\n" +
                   $"Backend: {Backend.webSocketUrl}\n" +
                   $"Avatar Quality: {Avatar.quality}\n" +
                   $"Target FPS: {Performance.targetFrameRate}\n" +
                   $"Logging Level: {Logging.minLogLevel}";
        }
        #endregion

        #region Data Models
        [System.Serializable]
        private class ConfigurationData
        {
            public BackendConfig backend;
            public AvatarConfig avatar;
            public LipSyncConfig lipSync;
            public UIConfig ui;
            public LoggingConfig logging;
            public MetricsConfig metrics;
            public PerformanceConfig performance;
        }
        #endregion
    }
}