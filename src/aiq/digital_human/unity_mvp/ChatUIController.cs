using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AIQToolkit.DigitalHuman.MVP
{
    /// <summary>
    /// Controls the chat interface UI
    /// </summary>
    public class ChatUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private ScrollRect chatScrollView;
        [SerializeField] private Transform messageContainer;
        [SerializeField] private InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private GameObject messagePrefab;
        [SerializeField] private GameObject userMessagePrefab;
        [SerializeField] private GameObject aiMessagePrefab;
        
        [Header("UI Settings")]
        [SerializeField] private float autoScrollDelay = 0.1f;
        [SerializeField] private int maxMessages = 50;
        [SerializeField] private Color userMessageColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color aiMessageColor = new Color(0.9f, 0.9f, 0.9f);
        
        [Header("Typing Indicator")]
        [SerializeField] private GameObject typingIndicator;
        [SerializeField] private float typingAnimationSpeed = 0.5f;
        
        // Events
        public event Action<string> OnMessageSubmitted;
        
        private List<GameObject> messageHistory = new List<GameObject>();
        private bool isEnabled = false;
        
        private void Start()
        {
            // Setup UI listeners
            if (sendButton != null)
            {
                sendButton.onClick.AddListener(SendMessage);
            }
            
            if (inputField != null)
            {
                inputField.onEndEdit.AddListener(OnInputEndEdit);
            }
            
            // Initially disable chat
            DisableChat();
        }
        
        public void EnableChat()
        {
            isEnabled = true;
            
            if (inputField != null)
            {
                inputField.interactable = true;
            }
            
            if (sendButton != null)
            {
                sendButton.interactable = true;
            }
            
            // Show welcome message
            AddMessage("Hello! I'm your digital assistant. How can I help you today?", false);
        }
        
        public void DisableChat()
        {
            isEnabled = false;
            
            if (inputField != null)
            {
                inputField.interactable = false;
            }
            
            if (sendButton != null)
            {
                sendButton.interactable = false;
            }
        }
        
        private void SendMessage()
        {
            if (!isEnabled || inputField == null || string.IsNullOrWhiteSpace(inputField.text))
                return;
            
            string message = inputField.text.Trim();
            inputField.text = "";
            
            // Invoke event
            OnMessageSubmitted?.Invoke(message);
            
            // Refocus input field
            inputField.Select();
            inputField.ActivateInputField();
        }
        
        private void OnInputEndEdit(string value)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SendMessage();
            }
        }
        
        public void AddMessage(string text, bool isUserMessage)
        {
            GameObject messagePrefabToUse = null;
            
            // Choose the appropriate prefab
            if (isUserMessage && userMessagePrefab != null)
            {
                messagePrefabToUse = userMessagePrefab;
            }
            else if (!isUserMessage && aiMessagePrefab != null)
            {
                messagePrefabToUse = aiMessagePrefab;
            }
            else if (messagePrefab != null)
            {
                messagePrefabToUse = messagePrefab;
            }
            
            if (messagePrefabToUse == null || messageContainer == null)
                return;
            
            // Create message
            GameObject newMessage = Instantiate(messagePrefabToUse, messageContainer);
            
            // Set message text
            Text messageText = newMessage.GetComponentInChildren<Text>();
            if (messageText != null)
            {
                messageText.text = text;
                messageText.color = isUserMessage ? userMessageColor : aiMessageColor;
            }
            
            // Set message alignment
            HorizontalLayoutGroup layoutGroup = newMessage.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup != null)
            {
                if (isUserMessage)
                {
                    layoutGroup.childAlignment = TextAnchor.MiddleRight;
                    layoutGroup.padding.right = 10;
                    layoutGroup.padding.left = 50;
                }
                else
                {
                    layoutGroup.childAlignment = TextAnchor.MiddleLeft;
                    layoutGroup.padding.left = 10;
                    layoutGroup.padding.right = 50;
                }
            }
            
            // Add to history
            messageHistory.Add(newMessage);
            
            // Remove old messages if limit exceeded
            if (messageHistory.Count > maxMessages)
            {
                GameObject oldMessage = messageHistory[0];
                messageHistory.RemoveAt(0);
                Destroy(oldMessage);
            }
            
            // Auto-scroll to bottom
            Invoke(nameof(ScrollToBottom), autoScrollDelay);
        }
        
        private void ScrollToBottom()
        {
            if (chatScrollView != null)
            {
                chatScrollView.verticalNormalizedPosition = 0f;
            }
        }
        
        public void ShowTypingIndicator(bool show)
        {
            if (typingIndicator != null)
            {
                typingIndicator.SetActive(show);
                
                if (show)
                {
                    // Animate typing indicator
                    StartCoroutine(AnimateTypingIndicator());
                }
            }
        }
        
        private System.Collections.IEnumerator AnimateTypingIndicator()
        {
            Text[] dots = typingIndicator.GetComponentsInChildren<Text>();
            int currentDot = 0;
            
            while (typingIndicator.activeSelf)
            {
                // Reset all dots
                foreach (Text dot in dots)
                {
                    dot.color = new Color(dot.color.r, dot.color.g, dot.color.b, 0.3f);
                }
                
                // Highlight current dot
                if (currentDot < dots.Length)
                {
                    dots[currentDot].color = new Color(dots[currentDot].color.r, dots[currentDot].color.g, dots[currentDot].color.b, 1f);
                }
                
                currentDot = (currentDot + 1) % dots.Length;
                yield return new WaitForSeconds(typingAnimationSpeed);
            }
        }
        
        public void ClearChat()
        {
            foreach (GameObject message in messageHistory)
            {
                Destroy(message);
            }
            messageHistory.Clear();
        }
        
        public void SetInputFieldFocus(bool focus)
        {
            if (inputField != null && isEnabled)
            {
                if (focus)
                {
                    inputField.Select();
                    inputField.ActivateInputField();
                }
                else
                {
                    inputField.DeactivateInputField();
                }
            }
        }
        
        public void SetPlaceholderText(string text)
        {
            if (inputField != null && inputField.placeholder != null)
            {
                Text placeholderText = inputField.placeholder.GetComponent<Text>();
                if (placeholderText != null)
                {
                    placeholderText.text = text;
                }
            }
        }
        
        private void Update()
        {
            // Handle keyboard shortcuts
            if (isEnabled)
            {
                // Focus input field on key press
                if (Input.GetKeyDown(KeyCode.T) && !inputField.isFocused)
                {
                    SetInputFieldFocus(true);
                }
                
                // Send message on Enter
                if (Input.GetKeyDown(KeyCode.Return) && inputField.isFocused && !Input.GetKey(KeyCode.LeftShift))
                {
                    SendMessage();
                }
            }
        }
        
        private void OnDestroy()
        {
            // Clean up
            if (sendButton != null)
            {
                sendButton.onClick.RemoveListener(SendMessage);
            }
            
            if (inputField != null)
            {
                inputField.onEndEdit.RemoveListener(OnInputEndEdit);
            }
        }
    }
}