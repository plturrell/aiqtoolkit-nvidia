using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AIQToolkit.DigitalHuman.MVP
{
    /// <summary>
    /// Controls lip synchronization using uLipSync
    /// </summary>
    public class LipSyncController : MonoBehaviour
    {
        [Header("uLipSync Configuration")]
        [SerializeField] private float sensitivity = 1f;
        [SerializeField] private float smoothTime = 0.1f;
        [SerializeField] private AudioMixerGroup outputAudioMixerGroup;
        
        [Header("Viseme Mappings")]
        [SerializeField] private VisemeMapping[] visemeMappings;
        
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float minVolume = 0.001f;
        
        // Events
        public event Action OnLipSyncStarted;
        public event Action OnLipSyncCompleted;
        
        private SkinnedMeshRenderer targetMesh;
        private Dictionary<string, int> blendShapeIndexes = new Dictionary<string, int>();
        private float[] currentBlendShapeWeights;
        private float[] targetBlendShapeWeights;
        private bool isProcessing = false;
        
        [Serializable]
        public class VisemeMapping
        {
            public string visemeName;
            public string blendShapeName;
            public float weight = 1f;
        }
        
        public void Initialize()
        {
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                
                if (outputAudioMixerGroup != null)
                {
                    audioSource.outputAudioMixerGroup = outputAudioMixerGroup;
                }
            }
            
            // Initialize blend shape arrays
            if (visemeMappings != null)
            {
                currentBlendShapeWeights = new float[visemeMappings.Length];
                targetBlendShapeWeights = new float[visemeMappings.Length];
            }
        }
        
        public void SetupAvatar(GameObject avatar)
        {
            // Find the mesh with blend shapes
            targetMesh = avatar.GetComponentInChildren<SkinnedMeshRenderer>();
            
            if (targetMesh == null)
            {
                Debug.LogError("No SkinnedMeshRenderer found on avatar!");
                return;
            }
            
            // Cache blend shape indexes
            CacheBlendShapeIndexes();
        }
        
        private void CacheBlendShapeIndexes()
        {
            blendShapeIndexes.Clear();
            
            if (targetMesh == null || visemeMappings == null) return;
            
            for (int i = 0; i < visemeMappings.Length; i++)
            {
                string blendShapeName = visemeMappings[i].blendShapeName;
                int index = GetBlendShapeIndex(targetMesh, blendShapeName);
                
                if (index >= 0)
                {
                    blendShapeIndexes[blendShapeName] = index;
                }
                else
                {
                    Debug.LogWarning($"Blend shape not found: {blendShapeName}");
                }
            }
        }
        
        private int GetBlendShapeIndex(SkinnedMeshRenderer mesh, string name)
        {
            for (int i = 0; i < mesh.sharedMesh.blendShapeCount; i++)
            {
                if (mesh.sharedMesh.GetBlendShapeName(i).Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }
        
        public void ProcessAudioData(byte[] audioData, string text)
        {
            if (isProcessing) return;
            
            StartCoroutine(ProcessAudioCoroutine(audioData, text));
        }
        
        public void ProcessTextToSpeech(string text)
        {
            if (isProcessing) return;
            
            // For MVP, simulate lip sync from text
            StartCoroutine(SimulateLipSyncFromText(text));
        }
        
        private IEnumerator ProcessAudioCoroutine(byte[] audioData, string text)
        {
            isProcessing = true;
            OnLipSyncStarted?.Invoke();
            
            // Convert byte array to audio clip
            AudioClip clip = ConvertToAudioClip(audioData);
            
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                
                // Process lip sync while audio is playing
                while (audioSource.isPlaying)
                {
                    ProcessLipSync();
                    yield return null;
                }
            }
            
            ResetVisemes();
            isProcessing = false;
            OnLipSyncCompleted?.Invoke();
        }
        
        private AudioClip ConvertToAudioClip(byte[] audioData)
        {
            // This is a simplified version - implement proper audio conversion
            // For MVP, create a dummy clip
            float[] samples = new float[audioData.Length / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = (audioData[i * 2] + audioData[i * 2 + 1] * 256) / 32768f;
            }
            
            AudioClip clip = AudioClip.Create("Speech", samples.Length, 1, 44100, false);
            clip.SetData(samples, 0);
            
            return clip;
        }
        
        private void ProcessLipSync()
        {
            if (audioSource == null || !audioSource.isPlaying) return;
            
            // Get audio spectrum data
            float[] spectrum = new float[256];
            audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
            
            // Analyze spectrum and map to visemes
            AnalyzeSpectrum(spectrum);
            
            // Apply viseme weights
            ApplyVisemeWeights();
        }
        
        private void AnalyzeSpectrum(float[] spectrum)
        {
            // Simplified frequency analysis for MVP
            // Map frequency ranges to visemes
            
            float lowFreq = GetFrequencyRange(spectrum, 0, 30);     // 'M', 'B', 'P'
            float midFreq = GetFrequencyRange(spectrum, 30, 100);   // 'A', 'E', 'I'
            float highFreq = GetFrequencyRange(spectrum, 100, 256); // 'S', 'F', 'V'
            
            // Reset target weights
            for (int i = 0; i < targetBlendShapeWeights.Length; i++)
            {
                targetBlendShapeWeights[i] = 0f;
            }
            
            // Map frequencies to visemes (simplified)
            if (lowFreq > minVolume)
            {
                SetVisemeWeight("M", lowFreq * sensitivity);
            }
            
            if (midFreq > minVolume)
            {
                SetVisemeWeight("A", midFreq * sensitivity);
            }
            
            if (highFreq > minVolume)
            {
                SetVisemeWeight("F", highFreq * sensitivity);
            }
        }
        
        private float GetFrequencyRange(float[] spectrum, int startIndex, int endIndex)
        {
            float sum = 0f;
            for (int i = startIndex; i < endIndex && i < spectrum.Length; i++)
            {
                sum += spectrum[i];
            }
            return sum / (endIndex - startIndex);
        }
        
        private void SetVisemeWeight(string viseme, float weight)
        {
            for (int i = 0; i < visemeMappings.Length; i++)
            {
                if (visemeMappings[i].visemeName == viseme)
                {
                    targetBlendShapeWeights[i] = Mathf.Clamp01(weight * visemeMappings[i].weight);
                    break;
                }
            }
        }
        
        private void ApplyVisemeWeights()
        {
            if (targetMesh == null) return;
            
            // Smooth blend shape transitions
            for (int i = 0; i < visemeMappings.Length; i++)
            {
                currentBlendShapeWeights[i] = Mathf.Lerp(
                    currentBlendShapeWeights[i],
                    targetBlendShapeWeights[i],
                    Time.deltaTime / smoothTime
                );
                
                string blendShapeName = visemeMappings[i].blendShapeName;
                if (blendShapeIndexes.TryGetValue(blendShapeName, out int index))
                {
                    targetMesh.SetBlendShapeWeight(index, currentBlendShapeWeights[i] * 100f);
                }
            }
        }
        
        private IEnumerator SimulateLipSyncFromText(string text)
        {
            isProcessing = true;
            OnLipSyncStarted?.Invoke();
            
            // Simple text-based lip sync simulation for MVP
            string[] words = text.Split(' ');
            
            foreach (string word in words)
            {
                // Simulate visemes based on letters
                foreach (char letter in word.ToLower())
                {
                    string viseme = GetVisemeFromLetter(letter);
                    if (!string.IsNullOrEmpty(viseme))
                    {
                        SetVisemeWeight(viseme, 1f);
                        ApplyVisemeWeights();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                
                // Pause between words
                ResetVisemes();
                yield return new WaitForSeconds(0.2f);
            }
            
            ResetVisemes();
            isProcessing = false;
            OnLipSyncCompleted?.Invoke();
        }
        
        private string GetVisemeFromLetter(char letter)
        {
            // Simple letter to viseme mapping
            switch (letter)
            {
                case 'm':
                case 'b':
                case 'p':
                    return "M";
                case 'f':
                case 'v':
                    return "F";
                case 'a':
                case 'e':
                case 'i':
                    return "A";
                case 's':
                case 'z':
                    return "S";
                case 'o':
                case 'u':
                    return "O";
                default:
                    return null;
            }
        }
        
        private void ResetVisemes()
        {
            for (int i = 0; i < targetBlendShapeWeights.Length; i++)
            {
                targetBlendShapeWeights[i] = 0f;
            }
            ApplyVisemeWeights();
        }
        
        public float GetCurrentVolume()
        {
            if (audioSource == null || !audioSource.isPlaying) return 0f;
            
            float[] samples = new float[256];
            audioSource.GetOutputData(samples, 0);
            
            float sum = 0f;
            foreach (float sample in samples)
            {
                sum += Mathf.Abs(sample);
            }
            
            return sum / samples.Length;
        }
        
        private void OnDestroy()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}