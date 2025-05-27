using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace AIQToolkit.DigitalHuman.Production
{
    /// <summary>
    /// Main production controller for the Digital Human MVP
    /// Handles initialization, error recovery, and system orchestration
    /// </summary>
    public class ProductionMVP : MonoBehaviour
    {
        #region Singleton Pattern
        private static ProductionMVP _instance;
        public static ProductionMVP Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ProductionMVP>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ProductionMVP");
                        _instance = go.AddComponent<ProductionMVP>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Events
        public UnityEvent<SystemState> OnSystemStateChanged = new UnityEvent<SystemState>();
        public UnityEvent<string> OnErrorOccurred = new UnityEvent<string>();
        public UnityEvent OnInitializationComplete = new UnityEvent();
        #endregion

        #region State Management
        public enum SystemState
        {
            Uninitialized,
            Initializing,
            Ready,
            Processing,
            Error,
            Shutdown
        }

        private SystemState _currentState = SystemState.Uninitialized;
        public SystemState CurrentState
        {
            get => _currentState;
            private set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    OnSystemStateChanged?.Invoke(value);
                    ProductionLogger.Instance.LogSystemStateChange(value);
                }
            }
        }
        #endregion

        #region Component References
        [Header("Core Components")]
        [SerializeField] private ProductionBackendConnector backendConnector;
        [SerializeField] private ProductionAvatarManager avatarManager;
        [SerializeField] private ProductionLipSync lipSync;
        [SerializeField] private ProductionChatUI chatUI;
        [SerializeField] private ProductionConfig config;
        [SerializeField] private ProductionMetrics metrics;
        [SerializeField] private ProductionLogger logger;

        [Header("Settings")]
        [SerializeField] private float initializationTimeout = 30f;
        [SerializeField] private int maxRetryAttempts = 3;
        [SerializeField] private float retryDelay = 5f;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            ValidateComponents();
        }

        private void Start()
        {
            StartCoroutine(InitializeSystem());
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                metrics.PauseMetrics();
                backendConnector.Pause();
            }
            else
            {
                metrics.ResumeMetrics();
                backendConnector.Resume();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                metrics.LogFocusLost();
            }
            else
            {
                metrics.LogFocusGained();
            }
        }

        private void OnDestroy()
        {
            Shutdown();
        }
        #endregion

        #region Initialization
        private IEnumerator InitializeSystem()
        {
            CurrentState = SystemState.Initializing;
            logger.LogInfo("Starting system initialization");
            
            float startTime = Time.realtimeSinceStartup;
            int retryCount = 0;

            while (retryCount < maxRetryAttempts)
            {
                bool success = yield return TryInitializeCore();
                
                if (success)
                {
                    float initTime = Time.realtimeSinceStartup - startTime;
                    metrics.LogInitializationTime(initTime);
                    
                    CurrentState = SystemState.Ready;
                    OnInitializationComplete?.Invoke();
                    logger.LogInfo($"System initialized successfully in {initTime:F2} seconds");
                    yield break;
                }

                retryCount++;
                if (retryCount < maxRetryAttempts)
                {
                    logger.LogWarning($"Initialization failed, retrying ({retryCount}/{maxRetryAttempts})");
                    yield return new WaitForSeconds(retryDelay);
                }
            }

            HandleInitializationFailure();
        }

        private IEnumerator TryInitializeCore()
        {
            // Load configuration
            bool configLoaded = config.LoadConfiguration();
            if (!configLoaded)
            {
                LogError("Failed to load configuration");
                yield return false;
            }

            // Initialize logger with config
            logger.Initialize(config.LoggingConfig);
            
            // Initialize metrics
            metrics.Initialize(config.MetricsConfig);
            
            // Connect to backend
            bool backendConnected = false;
            backendConnector.OnConnectionEstablished.AddListener(() => backendConnected = true);
            
            backendConnector.Initialize(config.BackendConfig);
            
            float connectionTimeout = config.BackendConfig.connectionTimeout;
            float elapsedTime = 0;
            
            while (!backendConnected && elapsedTime < connectionTimeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsedTime += 0.1f;
            }
            
            if (!backendConnected)
            {
                LogError("Failed to connect to backend");
                yield return false;
            }

            // Initialize avatar
            bool avatarReady = false;
            avatarManager.OnAvatarLoaded.AddListener(() => avatarReady = true);
            avatarManager.Initialize(config.AvatarConfig);
            
            float avatarTimeout = config.AvatarConfig.loadTimeout;
            elapsedTime = 0;
            
            while (!avatarReady && elapsedTime < avatarTimeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsedTime += 0.1f;
            }
            
            if (!avatarReady)
            {
                LogError("Failed to load avatar");
                yield return false;
            }

            // Initialize lip sync
            lipSync.Initialize(config.LipSyncConfig, avatarManager.GetAvatarMouthBlendShapes());
            
            // Initialize UI
            chatUI.Initialize(config.UIConfig);
            
            // Setup event connections
            SetupEventConnections();
            
            yield return true;
        }

        private void SetupEventConnections()
        {
            // Connect backend to UI
            backendConnector.OnMessageReceived.AddListener(message =>
            {
                chatUI.AddMessage(message, false);
                ProcessAIResponse(message);
            });

            // Connect UI to backend
            chatUI.OnUserMessage.AddListener(message =>
            {
                backendConnector.SendMessage(message);
                metrics.LogUserMessage();
            });

            // Connect audio to lip sync
            backendConnector.OnAudioReceived.AddListener(audioData =>
            {
                lipSync.ProcessAudioData(audioData);
                metrics.LogAudioProcessed(audioData.Length);
            });

            // Error handling
            backendConnector.OnError.AddListener(HandleBackendError);
            avatarManager.OnError.AddListener(HandleAvatarError);
            lipSync.OnError.AddListener(HandleLipSyncError);
        }
        #endregion

        #region Message Processing
        private void ProcessAIResponse(string message)
        {
            CurrentState = SystemState.Processing;
            
            try
            {
                // Parse any commands or emotions from the message
                var parsedData = ParseMessage(message);
                
                // Update avatar expression based on emotion
                if (!string.IsNullOrEmpty(parsedData.emotion))
                {
                    avatarManager.SetEmotion(parsedData.emotion);
                }
                
                // Handle any special commands
                if (parsedData.commands != null)
                {
                    foreach (var command in parsedData.commands)
                    {
                        ExecuteCommand(command);
                    }
                }
                
                metrics.LogMessageProcessed();
                CurrentState = SystemState.Ready;
            }
            catch (Exception e)
            {
                LogError($"Error processing AI response: {e.Message}");
                CurrentState = SystemState.Error;
            }
        }

        private MessageData ParseMessage(string message)
        {
            // Simple parsing logic - in production, use proper JSON parsing
            var data = new MessageData
            {
                text = message,
                emotion = ExtractEmotion(message),
                commands = ExtractCommands(message)
            };
            
            return data;
        }

        private string ExtractEmotion(string message)
        {
            // Simple emotion detection - in production, use NLP
            if (message.Contains("happy") || message.Contains("😊"))
                return "happy";
            if (message.Contains("sad") || message.Contains("😢"))
                return "sad";
            if (message.Contains("angry") || message.Contains("😠"))
                return "angry";
            
            return "neutral";
        }

        private string[] ExtractCommands(string message)
        {
            // Extract commands marked with special syntax
            // Example: [command:wave]
            var commands = new System.Collections.Generic.List<string>();
            
            // Simple regex-like extraction
            if (message.Contains("[command:"))
            {
                // Extract command between brackets
                // In production, use proper regex
            }
            
            return commands.ToArray();
        }

        private void ExecuteCommand(string command)
        {
            switch (command.ToLower())
            {
                case "wave":
                    avatarManager.PlayAnimation("Wave");
                    break;
                case "smile":
                    avatarManager.SetExpression("Smile", 1.0f);
                    break;
                case "reset":
                    avatarManager.ResetToIdle();
                    break;
                default:
                    logger.LogWarning($"Unknown command: {command}");
                    break;
            }
        }
        #endregion

        #region Error Handling
        private void HandleInitializationFailure()
        {
            CurrentState = SystemState.Error;
            string errorMessage = "System initialization failed after all retry attempts";
            LogError(errorMessage);
            
            // Attempt fallback initialization
            StartCoroutine(FallbackInitialization());
        }

        private IEnumerator FallbackInitialization()
        {
            logger.LogInfo("Attempting fallback initialization");
            
            // Try to at least get UI working with offline mode
            chatUI.EnableOfflineMode();
            
            // Load a default avatar
            bool defaultAvatarLoaded = avatarManager.LoadDefaultAvatar();
            
            if (defaultAvatarLoaded)
            {
                CurrentState = SystemState.Ready;
                logger.LogInfo("Fallback initialization successful - running in offline mode");
            }
            else
            {
                logger.LogError("Fallback initialization failed");
                // Show error UI
                chatUI.ShowCriticalError("System initialization failed. Please restart the application.");
            }
            
            yield return null;
        }

        private void HandleBackendError(string error)
        {
            LogError($"Backend error: {error}");
            metrics.LogError("Backend", error);
            
            // Attempt reconnection
            StartCoroutine(AttemptBackendReconnection());
        }

        private void HandleAvatarError(string error)
        {
            LogError($"Avatar error: {error}");
            metrics.LogError("Avatar", error);
            
            // Try to reload avatar
            avatarManager.ReloadAvatar();
        }

        private void HandleLipSyncError(string error)
        {
            LogError($"LipSync error: {error}");
            metrics.LogError("LipSync", error);
            
            // Disable lip sync temporarily
            lipSync.SetEnabled(false);
            StartCoroutine(RestartLipSyncAfterDelay());
        }

        private IEnumerator AttemptBackendReconnection()
        {
            int attempts = 0;
            while (attempts < 3 && !backendConnector.IsConnected)
            {
                yield return new WaitForSeconds(2f);
                backendConnector.Reconnect();
                attempts++;
            }
            
            if (!backendConnector.IsConnected)
            {
                chatUI.EnableOfflineMode();
            }
        }

        private IEnumerator RestartLipSyncAfterDelay()
        {
            yield return new WaitForSeconds(5f);
            lipSync.SetEnabled(true);
            lipSync.Reinitialize();
        }

        private void LogError(string message)
        {
            logger.LogError(message);
            OnErrorOccurred?.Invoke(message);
            Debug.LogError($"[ProductionMVP] {message}");
        }
        #endregion

        #region Public API
        /// <summary>
        /// Send a message to the AI backend
        /// </summary>
        public void SendMessage(string message)
        {
            if (CurrentState != SystemState.Ready)
            {
                logger.LogWarning($"Cannot send message in state: {CurrentState}");
                return;
            }
            
            chatUI.AddMessage(message, true);
            backendConnector.SendMessage(message);
        }

        /// <summary>
        /// Change the avatar
        /// </summary>
        public void ChangeAvatar(string avatarUrl)
        {
            avatarManager.LoadAvatar(avatarUrl);
        }

        /// <summary>
        /// Set avatar emotion
        /// </summary>
        public void SetEmotion(string emotion)
        {
            avatarManager.SetEmotion(emotion);
        }

        /// <summary>
        /// Restart the system
        /// </summary>
        public void RestartSystem()
        {
            StartCoroutine(RestartSequence());
        }

        private IEnumerator RestartSequence()
        {
            Shutdown();
            yield return new WaitForSeconds(1f);
            StartCoroutine(InitializeSystem());
        }

        /// <summary>
        /// Shutdown the system gracefully
        /// </summary>
        public void Shutdown()
        {
            CurrentState = SystemState.Shutdown;
            
            // Disconnect all components gracefully
            backendConnector?.Disconnect();
            avatarManager?.Dispose();
            lipSync?.Dispose();
            chatUI?.Dispose();
            metrics?.Shutdown();
            logger?.Shutdown();
            
            logger?.LogInfo("System shutdown complete");
        }
        #endregion

        #region Utility
        private void ValidateComponents()
        {
            if (backendConnector == null)
                backendConnector = GetComponent<ProductionBackendConnector>() ?? 
                                  gameObject.AddComponent<ProductionBackendConnector>();
            
            if (avatarManager == null)
                avatarManager = GetComponent<ProductionAvatarManager>() ?? 
                              gameObject.AddComponent<ProductionAvatarManager>();
            
            if (lipSync == null)
                lipSync = GetComponent<ProductionLipSync>() ?? 
                         gameObject.AddComponent<ProductionLipSync>();
            
            if (chatUI == null)
                chatUI = GetComponent<ProductionChatUI>() ?? 
                        gameObject.AddComponent<ProductionChatUI>();
            
            if (config == null)
                config = GetComponent<ProductionConfig>() ?? 
                        gameObject.AddComponent<ProductionConfig>();
            
            if (metrics == null)
                metrics = GetComponent<ProductionMetrics>() ?? 
                         gameObject.AddComponent<ProductionMetrics>();
            
            if (logger == null)
                logger = GetComponent<ProductionLogger>() ?? 
                        gameObject.AddComponent<ProductionLogger>();
        }

        [System.Serializable]
        private class MessageData
        {
            public string text;
            public string emotion;
            public string[] commands;
        }
        #endregion
    }
}