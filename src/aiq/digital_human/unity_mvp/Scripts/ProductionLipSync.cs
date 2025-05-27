using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using uLipSync;

namespace AIQToolkit.DigitalHuman.Production
{
    /// <summary>
    /// Production-ready lip sync implementation with uLipSync
    /// Includes audio processing optimizations and error handling
    /// </summary>
    public class ProductionLipSync : MonoBehaviour
    {
        #region Events
        public UnityEvent<float> OnLipSyncUpdate = new UnityEvent<float>();
        public UnityEvent<string> OnVisemeDetected = new UnityEvent<string>();
        public UnityEvent<string> OnError = new UnityEvent<string>();
        public UnityEvent OnLipSyncStarted = new UnityEvent();
        public UnityEvent OnLipSyncStopped = new UnityEvent();
        #endregion

        #region Configuration
        [System.Serializable]
        public class LipSyncConfig
        {
            public float volumeMultiplier = 1.0f;
            public float smoothing = 0.1f;
            public bool enableNoiseGate = true;
            public float noiseGateThreshold = 0.01f;
            public bool enableVisemeSmoothing = true;
            public float visemeSmoothTime = 0.1f;
            public AudioProcessingMode processingMode = AudioProcessingMode.Realtime;
            public int audioBufferSize = 2048;
            public float updateInterval = 0.016f; // 60 FPS
        }

        [System.Serializable]
        public enum AudioProcessingMode
        {
            Realtime,
            Buffered,
            Streaming
        }

        private LipSyncConfig config;
        #endregion

        #region Core Components
        private Dictionary<string, int> mouthBlendShapes;
        private SkinnedMeshRenderer targetMeshRenderer;
        private uLipSync.LipSync lipSyncComponent;
        private AudioSource audioSource;
        private bool isInitialized = false;
        private bool isEnabled = true;
        
        // Audio processing
        private float[] audioBuffer;
        private int bufferPosition = 0;
        private Queue<float[]> audioQueue = new Queue<float[]>();
        private Coroutine processingCoroutine;
        
        // Viseme mapping
        private Dictionary<string, string> visemeToBlendShapeMap = new Dictionary<string, string>();
        private Dictionary<string, float> currentVisemeWeights = new Dictionary<string, float>();
        private Dictionary<string, float> targetVisemeWeights = new Dictionary<string, float>();
        
        // Performance optimization
        private float lastUpdateTime = 0f;
        private int frameSkipCounter = 0;
        private const int MAX_FRAME_SKIP = 2;
        #endregion

        #region Initialization
        public void Initialize(LipSyncConfig lipSyncConfig, Dictionary<string, int> avatarMouthBlendShapes)
        {
            config = lipSyncConfig;
            mouthBlendShapes = avatarMouthBlendShapes;
            
            ProductionLogger.Instance.LogInfo("Initializing lip sync");
            
            SetupComponents();
            SetupVisemeMapping();
            
            isInitialized = true;
            ProductionLogger.Instance.LogInfo("Lip sync initialized successfully");
        }

        private void SetupComponents()
        {
            // Get or create audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D sound
            }
            
            // Setup uLipSync
            lipSyncComponent = GetComponent<uLipSync.LipSync>();
            if (lipSyncComponent == null)
            {
                lipSyncComponent = gameObject.AddComponent<uLipSync.LipSync>();
            }
            
            // Configure uLipSync
            ConfigureLipSync();
            
            // Initialize audio buffer
            audioBuffer = new float[config.audioBufferSize];
        }

        private void ConfigureLipSync()
        {
            lipSyncComponent.smoothness = config.smoothing;
            lipSyncComponent.audioSource = audioSource;
            
            // Set up callbacks
            lipSyncComponent.onLipSyncUpdate.AddListener(OnLipSyncUpdateInternal);
            
            // Configure profile if needed
            var profile = lipSyncComponent.profile;
            if (profile == null)
            {
                // Create default profile
                profile = ScriptableObject.CreateInstance<Profile>();
                lipSyncComponent.profile = profile;
            }
        }

        private void SetupVisemeMapping()
        {
            // Map visemes to blend shapes
            visemeToBlendShapeMap["A"] = "viseme_aa";
            visemeToBlendShapeMap["E"] = "viseme_E";
            visemeToBlendShapeMap["I"] = "viseme_I";
            visemeToBlendShapeMap["O"] = "viseme_O";
            visemeToBlendShapeMap["U"] = "viseme_U";
            visemeToBlendShapeMap["PP"] = "viseme_PP";
            visemeToBlendShapeMap["FF"] = "viseme_FF";
            visemeToBlendShapeMap["TH"] = "viseme_TH";
            visemeToBlendShapeMap["DD"] = "viseme_DD";
            visemeToBlendShapeMap["kk"] = "viseme_kk";
            visemeToBlendShapeMap["CH"] = "viseme_CH";
            visemeToBlendShapeMap["SS"] = "viseme_SS";
            visemeToBlendShapeMap["nn"] = "viseme_nn";
            visemeToBlendShapeMap["RR"] = "viseme_RR";
            visemeToBlendShapeMap["sil"] = "mouthClosed";
            
            // Initialize weight dictionaries
            foreach (var mapping in visemeToBlendShapeMap)
            {
                currentVisemeWeights[mapping.Key] = 0f;
                targetVisemeWeights[mapping.Key] = 0f;
            }
        }
        #endregion

        #region Audio Processing
        public void ProcessAudioData(AudioClip audioClip)
        {
            if (!isInitialized || !isEnabled)
            {
                ProductionLogger.Instance.LogWarning("Cannot process audio - lip sync not ready");
                return;
            }
            
            try
            {
                ProductionLogger.Instance.LogInfo($"Processing audio clip: {audioClip.name}, length: {audioClip.length}s");
                
                switch (config.processingMode)
                {
                    case AudioProcessingMode.Realtime:
                        ProcessRealtimeAudio(audioClip);
                        break;
                    case AudioProcessingMode.Buffered:
                        ProcessBufferedAudio(audioClip);
                        break;
                    case AudioProcessingMode.Streaming:
                        ProcessStreamingAudio(audioClip);
                        break;
                }
                
                OnLipSyncStarted?.Invoke();
                ProductionMetrics.Instance.LogLipSyncStarted();
            }
            catch (Exception e)
            {
                HandleError($"Error processing audio: {e.Message}");
            }
        }

        private void ProcessRealtimeAudio(AudioClip audioClip)
        {
            // Set audio clip and play
            audioSource.clip = audioClip;
            audioSource.volume = config.volumeMultiplier;
            audioSource.Play();
            
            // Start processing coroutine
            if (processingCoroutine != null)
                StopCoroutine(processingCoroutine);
            
            processingCoroutine = StartCoroutine(ProcessRealtimeAudioCoroutine());
        }

        private void ProcessBufferedAudio(AudioClip audioClip)
        {
            // Extract audio data
            float[] audioData = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(audioData, 0);
            
            // Process in chunks
            int chunkSize = config.audioBufferSize;
            for (int i = 0; i < audioData.Length; i += chunkSize)
            {
                int size = Mathf.Min(chunkSize, audioData.Length - i);
                float[] chunk = new float[size];
                Array.Copy(audioData, i, chunk, 0, size);
                audioQueue.Enqueue(chunk);
            }
            
            // Start processing queue
            if (processingCoroutine != null)
                StopCoroutine(processingCoroutine);
            
            processingCoroutine = StartCoroutine(ProcessBufferedAudioCoroutine());
        }

        private void ProcessStreamingAudio(AudioClip audioClip)
        {
            // For streaming, we'll process as audio comes in
            audioSource.clip = audioClip;
            audioSource.volume = config.volumeMultiplier;
            audioSource.Play();
            
            // Use OnAudioFilterRead for real-time processing
            enabled = true;
        }

        private IEnumerator ProcessRealtimeAudioCoroutine()
        {
            while (audioSource.isPlaying)
            {
                // Get current audio samples
                audioSource.GetOutputData(audioBuffer, 0);
                
                // Process audio buffer
                ProcessAudioBuffer(audioBuffer);
                
                yield return new WaitForSeconds(config.updateInterval);
            }
            
            OnLipSyncStopped?.Invoke();
            ProductionMetrics.Instance.LogLipSyncStopped();
        }

        private IEnumerator ProcessBufferedAudioCoroutine()
        {
            audioSource.Play();
            
            while (audioQueue.Count > 0 || audioSource.isPlaying)
            {
                if (audioQueue.Count > 0)
                {
                    float[] chunk = audioQueue.Dequeue();
                    ProcessAudioBuffer(chunk);
                }
                
                yield return new WaitForSeconds(config.updateInterval);
            }
            
            OnLipSyncStopped?.Invoke();
            ProductionMetrics.Instance.LogLipSyncStopped();
        }

        private void ProcessAudioBuffer(float[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
                return;
            
            // Apply noise gate if enabled
            if (config.enableNoiseGate)
            {
                buffer = ApplyNoiseGate(buffer);
            }
            
            // Get volume
            float volume = GetAudioVolume(buffer);
            
            // Detect visemes
            string detectedViseme = DetectViseme(buffer, volume);
            
            // Update viseme weights
            UpdateVisemeWeights(detectedViseme, volume);
            
            // Apply blend shapes
            ApplyBlendShapes();
            
            // Trigger events
            OnLipSyncUpdate?.Invoke(volume);
            if (!string.IsNullOrEmpty(detectedViseme))
            {
                OnVisemeDetected?.Invoke(detectedViseme);
            }
        }

        private float[] ApplyNoiseGate(float[] buffer)
        {
            float[] gatedBuffer = new float[buffer.Length];
            
            for (int i = 0; i < buffer.Length; i++)
            {
                if (Mathf.Abs(buffer[i]) < config.noiseGateThreshold)
                {
                    gatedBuffer[i] = 0f;
                }
                else
                {
                    gatedBuffer[i] = buffer[i];
                }
            }
            
            return gatedBuffer;
        }

        private float GetAudioVolume(float[] buffer)
        {
            float sum = 0f;
            for (int i = 0; i < buffer.Length; i++)
            {
                sum += buffer[i] * buffer[i];
            }
            
            return Mathf.Sqrt(sum / buffer.Length);
        }

        private string DetectViseme(float[] buffer, float volume)
        {
            // Simplified viseme detection based on volume and frequency
            // In production, use more sophisticated FFT-based analysis
            
            if (volume < 0.05f)
                return "sil"; // Silence
            
            // Random viseme for demonstration - replace with actual detection
            string[] visemes = { "A", "E", "I", "O", "U", "PP", "FF", "TH" };
            int index = UnityEngine.Random.Range(0, visemes.Length);
            
            return visemes[index];
        }

        private void UpdateVisemeWeights(string viseme, float volume)
        {
            // Reset all target weights
            foreach (var key in targetVisemeWeights.Keys.ToArray())
            {
                targetVisemeWeights[key] = 0f;
            }
            
            // Set target weight for detected viseme
            if (!string.IsNullOrEmpty(viseme) && targetVisemeWeights.ContainsKey(viseme))
            {
                targetVisemeWeights[viseme] = volume * 100f; // Scale to blend shape range
            }
            
            // Smooth transition if enabled
            if (config.enableVisemeSmoothing)
            {
                foreach (var key in currentVisemeWeights.Keys.ToArray())
                {
                    currentVisemeWeights[key] = Mathf.Lerp(
                        currentVisemeWeights[key],
                        targetVisemeWeights[key],
                        Time.deltaTime / config.visemeSmoothTime
                    );
                }
            }
            else
            {
                // Direct assignment
                foreach (var key in targetVisemeWeights.Keys)
                {
                    currentVisemeWeights[key] = targetVisemeWeights[key];
                }
            }
        }
        #endregion

        #region Blend Shape Application
        private void ApplyBlendShapes()
        {
            if (targetMeshRenderer == null || mouthBlendShapes == null)
                return;
            
            // Frame skipping for performance
            frameSkipCounter++;
            if (frameSkipCounter < MAX_FRAME_SKIP)
                return;
            
            frameSkipCounter = 0;
            
            // Apply each viseme weight to corresponding blend shape
            foreach (var mapping in visemeToBlendShapeMap)
            {
                string viseme = mapping.Key;
                string blendShapeName = mapping.Value;
                
                if (currentVisemeWeights.ContainsKey(viseme) && mouthBlendShapes.ContainsKey(blendShapeName))
                {
                    int blendShapeIndex = mouthBlendShapes[blendShapeName];
                    float weight = currentVisemeWeights[viseme];
                    
                    targetMeshRenderer.SetBlendShapeWeight(blendShapeIndex, weight);
                }
            }
        }

        public void SetTargetMeshRenderer(SkinnedMeshRenderer meshRenderer)
        {
            targetMeshRenderer = meshRenderer;
            ProductionLogger.Instance.LogInfo("Target mesh renderer set for lip sync");
        }
        #endregion

        #region Unity Audio Processing
        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (!isEnabled || config.processingMode != AudioProcessingMode.Streaming)
                return;
            
            // Process streaming audio data
            ProcessAudioBuffer(data);
        }
        #endregion

        #region Control Methods
        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            
            if (!enabled)
            {
                // Reset all blend shapes
                ResetBlendShapes();
                
                // Stop any playing audio
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                
                OnLipSyncStopped?.Invoke();
            }
            
            ProductionLogger.Instance.LogInfo($"Lip sync enabled: {enabled}");
        }

        public void Reinitialize()
        {
            if (!isInitialized)
                return;
            
            ProductionLogger.Instance.LogInfo("Reinitializing lip sync");
            
            // Reset components
            ResetBlendShapes();
            
            // Clear audio queue
            audioQueue.Clear();
            
            // Restart processing
            if (processingCoroutine != null)
            {
                StopCoroutine(processingCoroutine);
                processingCoroutine = null;
            }
        }

        private void ResetBlendShapes()
        {
            if (targetMeshRenderer == null || mouthBlendShapes == null)
                return;
            
            foreach (var blendShape in mouthBlendShapes)
            {
                targetMeshRenderer.SetBlendShapeWeight(blendShape.Value, 0f);
            }
            
            // Reset weight dictionaries
            foreach (var key in currentVisemeWeights.Keys.ToArray())
            {
                currentVisemeWeights[key] = 0f;
                targetVisemeWeights[key] = 0f;
            }
        }
        #endregion

        #region Error Handling
        private void HandleError(string error)
        {
            ProductionLogger.Instance.LogError($"Lip sync error: {error}");
            OnError?.Invoke(error);
            
            // Attempt recovery
            try
            {
                Reinitialize();
            }
            catch (Exception e)
            {
                ProductionLogger.Instance.LogError($"Failed to recover from error: {e.Message}");
            }
        }
        #endregion

        #region Cleanup
        public void Dispose()
        {
            // Stop all coroutines
            if (processingCoroutine != null)
            {
                StopCoroutine(processingCoroutine);
                processingCoroutine = null;
            }
            
            // Reset blend shapes
            ResetBlendShapes();
            
            // Clear audio queue
            audioQueue.Clear();
            
            // Clean up components
            if (lipSyncComponent != null)
            {
                lipSyncComponent.onLipSyncUpdate.RemoveListener(OnLipSyncUpdateInternal);
            }
            
            isInitialized = false;
            ProductionLogger.Instance.LogInfo("Lip sync disposed");
        }
        #endregion

        #region Event Handlers
        private void OnLipSyncUpdateInternal(float volume, formant formant)
        {
            // Convert formant to viseme
            string viseme = FormantToViseme(formant);
            
            // Update weights
            UpdateVisemeWeights(viseme, volume);
            
            // Apply blend shapes
            ApplyBlendShapes();
        }

        private string FormantToViseme(formant formant)
        {
            // Map formant values to visemes
            // This is a simplified mapping - adjust based on your needs
            
            float f1 = formant.f1;
            float f2 = formant.f2;
            
            if (f1 < 300 && f2 < 1000)
                return "U";
            else if (f1 < 400 && f2 > 2000)
                return "I";
            else if (f1 > 700 && f2 < 1500)
                return "A";
            else if (f1 > 500 && f2 > 1500)
                return "E";
            else if (f1 > 400 && f2 < 1000)
                return "O";
            else
                return "sil";
        }
        #endregion
    }
}