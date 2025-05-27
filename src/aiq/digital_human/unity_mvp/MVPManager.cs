using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AIQToolkit.DigitalHuman.MVP
{
    /// <summary>
    /// Main manager for the Digital Human MVP Unity application
    /// </summary>
    public class MVPManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private AvatarController avatarController;
        [SerializeField] private LipSyncController lipSyncController;
        [SerializeField] private ChatUIController chatUIController;
        [SerializeField] private BackendConnector backendConnector;
        
        [Header("UI Elements")]
        [SerializeField] private Button startChatButton;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private Text statusText;
        
        [Header("Configuration")]
        [SerializeField] private string backendURL = "ws://localhost:8080/ws";
        [SerializeField] private float messageDelay = 0.5f;
        
        private bool isInitialized = false;
        private bool isProcessing = false;
        
        private void Start()
        {
            InitializeApplication();
        }
        
        private void InitializeApplication()
        {
            Debug.Log("Initializing Digital Human MVP...");
            
            // Initialize backend connection
            backendConnector.Initialize(backendURL);
            backendConnector.OnMessageReceived += HandleBackendMessage;
            backendConnector.OnConnectionStatusChanged += HandleConnectionStatus;
            
            // Initialize avatar controller
            avatarController.OnAvatarLoaded += HandleAvatarLoaded;
            avatarController.OnAvatarLoadError += HandleAvatarError;
            
            // Initialize chat UI
            chatUIController.OnMessageSubmitted += HandleUserMessage;
            
            // Initialize lip sync
            lipSyncController.Initialize();
            
            // Setup UI
            if (startChatButton != null)
            {
                startChatButton.onClick.AddListener(StartChatSession);
            }
            
            SetStatus("Waiting for user to start...");
        }
        
        private void StartChatSession()
        {
            if (isProcessing) return;
            
            isProcessing = true;
            SetLoading(true);
            SetStatus("Loading avatar...");
            
            // Load avatar from Ready Player Me
            avatarController.LoadAvatar();
        }
        
        private void HandleAvatarLoaded(GameObject avatar)
        {
            Debug.Log("Avatar loaded successfully!");
            
            // Setup lip sync on the avatar
            lipSyncController.SetupAvatar(avatar);
            
            // Enable chat interface
            chatUIController.EnableChat();
            
            // Connect to backend
            SetStatus("Connecting to backend...");
            backendConnector.Connect();
            
            isInitialized = true;
            isProcessing = false;
            SetLoading(false);
        }
        
        private void HandleAvatarError(string error)
        {
            Debug.LogError($"Avatar loading error: {error}");
            SetStatus($"Avatar error: {error}");
            isProcessing = false;
            SetLoading(false);
        }
        
        private void HandleConnectionStatus(bool isConnected)
        {
            if (isConnected)
            {
                SetStatus("Connected - Ready to chat!");
                avatarController.SetAvatarState(AvatarController.AvatarState.Idle);
            }
            else
            {
                SetStatus("Disconnected from backend");
                avatarController.SetAvatarState(AvatarController.AvatarState.Idle);
            }
        }
        
        private void HandleUserMessage(string message)
        {
            if (!isInitialized || isProcessing) return;
            
            isProcessing = true;
            
            // Show user message in chat
            chatUIController.AddMessage(message, true);
            
            // Set avatar to thinking state
            avatarController.SetAvatarState(AvatarController.AvatarState.Thinking);
            
            // Send message to backend
            backendConnector.SendMessage(message);
        }
        
        private void HandleBackendMessage(string message)
        {
            if (!isInitialized) return;
            
            // Parse the response (assuming it contains text and audio data)
            BackendResponse response = JsonUtility.FromJson<BackendResponse>(message);
            
            // Show AI response in chat
            chatUIController.AddMessage(response.text, false);
            
            // Set avatar to speaking state
            avatarController.SetAvatarState(AvatarController.AvatarState.Speaking);
            
            // Process audio for lip sync
            if (!string.IsNullOrEmpty(response.audioData))
            {
                byte[] audioBytes = System.Convert.FromBase64String(response.audioData);
                lipSyncController.ProcessAudioData(audioBytes, response.text);
            }
            else
            {
                // If no audio, use text-to-speech
                lipSyncController.ProcessTextToSpeech(response.text);
            }
            
            // Reset processing flag after animation completes
            StartCoroutine(ResetProcessingAfterDelay(response.estimatedDuration));
        }
        
        private IEnumerator ResetProcessingAfterDelay(float duration)
        {
            yield return new WaitForSeconds(duration);
            
            isProcessing = false;
            avatarController.SetAvatarState(AvatarController.AvatarState.Idle);
        }
        
        private void SetStatus(string status)
        {
            if (statusText != null)
            {
                statusText.text = status;
            }
            Debug.Log($"Status: {status}");
        }
        
        private void SetLoading(bool isLoading)
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(isLoading);
            }
        }
        
        private void OnDestroy()
        {
            // Cleanup
            if (backendConnector != null)
            {
                backendConnector.OnMessageReceived -= HandleBackendMessage;
                backendConnector.OnConnectionStatusChanged -= HandleConnectionStatus;
                backendConnector.Disconnect();
            }
            
            if (avatarController != null)
            {
                avatarController.OnAvatarLoaded -= HandleAvatarLoaded;
                avatarController.OnAvatarLoadError -= HandleAvatarError;
            }
            
            if (chatUIController != null)
            {
                chatUIController.OnMessageSubmitted -= HandleUserMessage;
            }
        }
    }
    
    [System.Serializable]
    public class BackendResponse
    {
        public string text;
        public string audioData;
        public float estimatedDuration;
        public string emotion;
    }
}