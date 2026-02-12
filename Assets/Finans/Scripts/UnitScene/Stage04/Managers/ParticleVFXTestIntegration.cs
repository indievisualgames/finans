using UnityEngine;
using TMPro;
using ParticleVFXSystem;

namespace ParticleVFXSystem.Managers
{
    /// <summary>
    /// Test integration script for Particle VFX System
    /// This makes it easy to test particles without setting up complex configurations
    /// </summary>
    public class ParticleVFXTestIntegration : MonoBehaviour
    {
        [Header("Test UI Components")]
        [Tooltip("Score text component to test score particles")]
        public TextMeshProUGUI testScoreText;
        
        [Tooltip("XP text component to test XP particles")]
        public TextMeshProUGUI testXPText;
        
        [Tooltip("Coin text component to test coin particles")]
        public TextMeshProUGUI testCoinText;
        
        [Tooltip("Star display to test star particles")]
        public GameObject testStarDisplay;
        
        [Header("Test Settings")]
        [Tooltip("Auto-assign UI components from scene")]
        public bool autoFindUIComponents = true;
        
        [Tooltip("Test particles automatically on start")]
        public bool testOnStart = false;
        
        [Tooltip("Test interval in seconds")]
        [Range(1f, 10f)]
        public float testInterval = 3f;
        
        [Header("Integration")]
        [Tooltip("Reference to MinigameScoreManager for integration")]
        public MinigameScoreManager scoreManager;
        
        private float lastTestTime;
        
        void Start()
        {
            if (autoFindUIComponents)
            {
                FindUIComponents();
            }
            
            if (testOnStart)
            {
                Invoke("RunFullTest", 1f);
            }
            
            // Try to find score manager
            if (scoreManager == null)
            {
                scoreManager = FindFirstObjectByType<MinigameScoreManager>();
            }
        }
        
        void Update()
        {
            // Auto-test every interval
            if (testOnStart && Time.time - lastTestTime > testInterval)
            {
                RunFullTest();
                lastTestTime = Time.time;
            }
        }
        
        /// <summary>
        /// Find UI components automatically
        /// </summary>
        private void FindUIComponents()
        {
            // Find TextMeshPro components
            TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            
            foreach (var text in allTexts)
            {
                string textName = text.name.ToLower();
                
                if (testScoreText == null && (textName.Contains("score") || textName.Contains("points")))
                {
                    testScoreText = text;
                    Debug.Log($"🎆 Auto-assigned score text: {text.name}");
                }
                else if (testXPText == null && (textName.Contains("xp") || textName.Contains("experience")))
                {
                    testXPText = text;
                    Debug.Log($"🎆 Auto-assigned XP text: {text.name}");
                }
                else if (testCoinText == null && (textName.Contains("coin") || textName.Contains("money")))
                {
                    testCoinText = text;
                    Debug.Log($"🎆 Auto-assigned coin text: {text.name}");
                }
            }
            
            // Find star display
            if (testStarDisplay == null)
            {
                GameObject[] stars = GameObject.FindGameObjectsWithTag("Star");
                if (stars.Length > 0)
                {
                    testStarDisplay = stars[0];
                    Debug.Log($"🎆 Auto-assigned star display: {stars[0].name}");
                }
            }
        }
        
        /// <summary>
        /// Run full particle test
        /// </summary>
        [ContextMenu("Run Full Particle Test")]
        public void RunFullTest()
        {
            Debug.Log("🎆 === RUNNING FULL PARTICLE VFX TEST ===");
            
            // Test individual effects
            TestScoreParticles();
            TestXPParticles();
            TestCoinParticles();
            TestStarParticles();
            
            Debug.Log("🎆 === FULL PARTICLE VFX TEST COMPLETED ===");
        }
        
        /// <summary>
        /// Test score particles
        /// </summary>
        [ContextMenu("Test Score Particles")]
        public void TestScoreParticles()
        {
            if (GlobalParticleVFXManager.Instance == null)
            {
                Debug.LogError("🎆 GlobalParticleVFXManager not found! Make sure it's in the scene.");
                return;
            }
            
            Debug.Log("🎆 Testing Score Particles...");
            
            // Update UI components in GlobalParticleVFXManager
            UpdateGlobalManagerUIComponents();
            
            // Test particles
            GlobalParticleVFXManager.Instance.PlayScoreGainParticles();
            
            // Wait a bit and test loss particles
            Invoke("TestScoreLossParticles", 0.5f);
        }
        
        private void TestScoreLossParticles()
        {
            if (GlobalParticleVFXManager.Instance != null)
            {
                GlobalParticleVFXManager.Instance.PlayScoreLossParticles();
            }
        }
        
        /// <summary>
        /// Test XP particles
        /// </summary>
        [ContextMenu("Test XP Particles")]
        public void TestXPParticles()
        {
            if (GlobalParticleVFXManager.Instance == null)
            {
                Debug.LogError("🎆 GlobalParticleVFXManager not found!");
                return;
            }
            
            Debug.Log("🎆 Testing XP Particles...");
            
            // Update UI components in GlobalParticleVFXManager
            UpdateGlobalManagerUIComponents();
            
            // Test particles
            GlobalParticleVFXManager.Instance.PlayXPGainParticles();
        }
        
        /// <summary>
        /// Test coin particles
        /// </summary>
        [ContextMenu("Test Coin Particles")]
        public void TestCoinParticles()
        {
            if (GlobalParticleVFXManager.Instance == null)
            {
                Debug.LogError("🎆 GlobalParticleVFXManager not found!");
                return;
            }
            
            Debug.Log("🎆 Testing Coin Particles...");
            
            // Update UI components in GlobalParticleVFXManager
            UpdateGlobalManagerUIComponents();
            
            // Test particles
            GlobalParticleVFXManager.Instance.PlayCoinGainParticles();
        }
        
        /// <summary>
        /// Test star particles
        /// </summary>
        [ContextMenu("Test Star Particles")]
        public void TestStarParticles()
        {
            if (GlobalParticleVFXManager.Instance == null)
            {
                Debug.LogError("🎆 GlobalParticleVFXManager not found!");
                return;
            }
            
            Debug.Log("🎆 Testing Star Particles...");
            
            // Update UI components in GlobalParticleVFXManager
            UpdateGlobalManagerUIComponents();
            
            // Test particles
            GlobalParticleVFXManager.Instance.PlayStarAchievementParticles();
        }
        
        /// <summary>
        /// Update UI components in GlobalParticleVFXManager
        /// </summary>
        private void UpdateGlobalManagerUIComponents()
        {
            if (GlobalParticleVFXManager.Instance == null) return;
            
            // Update UI component references
            if (testScoreText != null)
            {
                GlobalParticleVFXManager.Instance.primaryScoreText = testScoreText;
            }
            
            if (testXPText != null)
            {
                GlobalParticleVFXManager.Instance.primaryXPText = testXPText;
            }
            
            if (testCoinText != null)
            {
                GlobalParticleVFXManager.Instance.primaryCoinText = testCoinText;
            }
            
            if (testStarDisplay != null)
            {
                GlobalParticleVFXManager.Instance.primaryStarDisplay = testStarDisplay;
            }
            
            Debug.Log("🎆 Updated GlobalParticleVFXManager UI components");
        }
        
        /// <summary>
        /// Test integration with DynamicScoreManager
        /// </summary>
        [ContextMenu("Test Score Manager Integration")]
        public void TestScoreManagerIntegration()
        {
            if (scoreManager == null)
            {
                Debug.LogError("🎆 ScoreManager not found! Assign it manually or it will be auto-found.");
                return;
            }
            
            Debug.Log("🎆 Testing Score Manager Integration...");
            
            // Simulate score changes
            scoreManager.AddScore(100);
            scoreManager.AddXP(50);
            
            Debug.Log("🎆 Score Manager Integration Test Completed!");
        }
        
        /// <summary>
        /// Get test status
        /// </summary>
        [ContextMenu("Print Test Status")]
        public void PrintTestStatus()
        {
            string status = "=== Particle VFX Test Integration Status ===\n";
            status += $"GlobalParticleVFXManager: {(GlobalParticleVFXManager.Instance != null ? "✅ Found" : "❌ Not Found")}\n";
            status += $"Test Score Text: {(testScoreText != null ? "✅ Assigned" : "❌ Not Assigned")}\n";
            status += $"Test XP Text: {(testXPText != null ? "✅ Assigned" : "❌ Not Assigned")}\n";
            status += $"Test Coin Text: {(testCoinText != null ? "✅ Assigned" : "❌ Not Assigned")}\n";
            status += $"Test Star Display: {(testStarDisplay != null ? "✅ Assigned" : "❌ Not Assigned")}\n";
            status += $"Score Manager: {(scoreManager != null ? "✅ Assigned" : "❌ Not Assigned")}\n";
            status += $"Auto Find UI: {autoFindUIComponents}\n";
            status += $"Test On Start: {testOnStart}\n";
            status += $"Test Interval: {testInterval}s";
            
            Debug.Log(status);
        }
        
        /// <summary>
        /// Force create fallback systems
        /// </summary>
        [ContextMenu("Force Create Fallback Systems")]
        public void ForceCreateFallbackSystems()
        {
            if (GlobalParticleVFXManager.Instance != null)
            {
                GlobalParticleVFXManager.Instance.ForceCreateFallbackSystems();
            }
        }
        
        /// <summary>
        /// Test all particle effects
        /// </summary>
        [ContextMenu("Test All Particle Effects")]
        public void TestAllParticleEffects()
        {
            if (GlobalParticleVFXManager.Instance != null)
            {
                GlobalParticleVFXManager.Instance.TestGlobalParticleSystem();
            }
        }
        
        /// <summary>
        /// Get system status
        /// </summary>
        [ContextMenu("Print System Status")]
        public void PrintSystemStatus()
        {
            if (GlobalParticleVFXManager.Instance != null)
            {
                Debug.Log(GlobalParticleVFXManager.Instance.GetSystemStatus());
            }
        }
    }
}
