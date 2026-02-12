using UnityEngine;
using System.Collections.Generic;
using ParticleVFXSystem;

namespace ParticleVFXSystem.Managers
{
    /// <summary>
    /// Custom particle settings for comprehensive control
    /// </summary>
    [System.Serializable]
    public class ParticleSettings
    {
        [Header("Gravity Settings")]
        [Tooltip("Gravity strength (0 = no gravity, 1 = normal, 2 = double gravity)")]
        [Range(0f, 2f)]
        public float gravityModifier = 1.0f;
        
        [Header("Size Settings")]
        [Tooltip("Base particle size (0.1 = tiny, 1.0 = normal, 3.0 = huge)")]
        [Range(0.1f, 5.0f)]
        public float particleSize = 1.0f;
        
        [Tooltip("Random variation in size")]
        [Range(0f, 1f)]
        public float sizeVariation = 0.2f;
        
        [Tooltip("Enable size over lifetime")]
        public bool useSizeOverLifetime = false;
        
        [Tooltip("Initial size for size over lifetime")]
        [Range(0.1f, 5.0f)]
        public float startSize = 1.0f;
        
        [Tooltip("Final size for size over lifetime")]
        [Range(0.1f, 5.0f)]
        public float endSize = 0.5f;
        
        [Header("Movement Settings")]
        [Tooltip("Vertical movement speed (negative = down, positive = up)")]
        [Range(-10f, 10f)]
        public float fallSpeed = 0f;
        
        [Header("Visual Settings")]
        [Tooltip("Use custom particle color")]
        public bool useCustomColor = false;
        
        [Tooltip("Custom particle color")]
        public Color particleColor = Color.white;
        
        [Header("Emission Settings")]
        [Tooltip("Custom burst count (0 = use default)")]
        [Range(0, 100)]
        public int customBurstCount = 0;
    }

    /// <summary>
    /// Global Particle VFX Manager for cross-game compatibility
    /// This manager can be used in any game or mini-game
    /// </summary>
    public class GlobalParticleVFXManager : MonoBehaviour
    {
        [Header("Global Particle VFX System")]
        [Tooltip("Global particle VFX configuration asset")]
        public ParticleVFXConfig globalConfig;
        
        [Tooltip("Global particle VFX prefab collection")]
        public ParticleVFXPrefabCollection prefabCollection;
        
        [Tooltip("Global particle VFX event system")]
        public ParticleVFXEventSystem eventSystem;
        
        [Tooltip("Enable global particle VFX system")]
        public bool enableGlobalSystem = true;
        
        [Tooltip("Auto-initialize from global config")]
        public bool autoInitializeFromConfig = true;
        
        [Header("Particle Pooling")]
        [Tooltip("Enable particle pooling for performance")]
        public bool enableParticlePooling = true;
        
        [Tooltip("Particle pool size")]
        [Range(10, 100)]
        public int particlePoolSize = 50;
        
        [Header("Target UI Components")]
        [Tooltip("Primary score text component")]
        public TMPro.TextMeshProUGUI primaryScoreText;
        
        [Tooltip("Primary XP text component")]
        public TMPro.TextMeshProUGUI primaryXPText;
        
        [Tooltip("Primary coin text component")]
        public TMPro.TextMeshProUGUI primaryCoinText;
        
        [Tooltip("Primary star display component")]
        public GameObject primaryStarDisplay;
        
        [Tooltip("Primary level text component")]
        public TMPro.TextMeshProUGUI primaryLevelText;
        
        [Tooltip("Primary streak text component")]
        public TMPro.TextMeshProUGUI primaryStreakText;
        
        [Tooltip("Primary milestone text component")]
        public TMPro.TextMeshProUGUI primaryMilestoneText;
        
        [Header("Particle Systems")]
        [Tooltip("Active particle systems")]
        public List<ParticleSystem> activeParticleSystems = new List<ParticleSystem>();
        
        [Tooltip("Particle system pool")]
        public Queue<ParticleSystem> particlePool = new Queue<ParticleSystem>();
        
        // Singleton instance
        public static GlobalParticleVFXManager Instance { get; private set; }
        
        // Configuration cache
        private Dictionary<ParticleEffectType, GameObject> particlePrefabCache = new Dictionary<ParticleEffectType, GameObject>();
        private Dictionary<ParticleEffectType, ParticleSystem> activeParticleCache = new Dictionary<ParticleEffectType, ParticleSystem>();
        
        // Fallback particle systems
        private Dictionary<ParticleEffectType, ParticleSystem> fallbackParticleSystems = new Dictionary<ParticleEffectType, ParticleSystem>();
        
        void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGlobalSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            if (autoInitializeFromConfig && globalConfig != null)
            {
                ApplyGlobalConfig();
            }
            
            // Create fallback particle systems if no prefabs are available
            if (particlePrefabCache.Count == 0)
            {
                CreateFallbackParticleSystems();
            }
        }
        
        /// <summary>
        /// Initialize the global particle VFX system
        /// </summary>
        private void InitializeGlobalSystem()
        {
            if (!enableGlobalSystem) return;
            
            Debug.Log("🎆 Initializing Global Particle VFX System...");
            
            // Initialize prefab cache
            if (prefabCollection != null)
            {
                InitializePrefabCache();
            }
            else
            {
                Debug.LogWarning("🎆 No prefab collection assigned - using fallback systems");
            }
            
            // Initialize particle pooling
            if (enableParticlePooling)
            {
                InitializeParticlePool();
            }
            
            // Initialize event system
            if (eventSystem != null)
            {
                InitializeEventSystem();
            }
            
            Debug.Log("🎆 Global Particle VFX System initialized successfully!");
        }
        
        /// <summary>
        /// Initialize prefab cache for quick access
        /// </summary>
        private void InitializePrefabCache()
        {
            particlePrefabCache.Clear();
            
            foreach (ParticleEffectType effectType in System.Enum.GetValues(typeof(ParticleEffectType)))
            {
                GameObject prefab = prefabCollection.GetParticlePrefab(effectType);
                if (prefab != null)
                {
                    particlePrefabCache[effectType] = prefab;
                    Debug.Log($"🎆 Cached prefab for {effectType}: {prefab.name}");
                }
                else
                {
                    Debug.LogWarning($"🎆 No prefab found for {effectType}");
                }
            }
            
            Debug.Log($"🎆 Prefab cache initialized with {particlePrefabCache.Count} prefabs");
        }
        
        /// <summary>
        /// Create fallback particle systems when no prefabs are available
        /// </summary>
        private void CreateFallbackParticleSystems()
        {
            Debug.Log("🎆 Creating fallback particle systems...");
            
            foreach (ParticleEffectType effectType in System.Enum.GetValues(typeof(ParticleEffectType)))
            {
                if (!fallbackParticleSystems.ContainsKey(effectType))
                {
                    ParticleSystem fallbackSystem = CreateFallbackParticleSystem(effectType);
                    if (fallbackSystem != null)
                    {
                        fallbackParticleSystems[effectType] = fallbackSystem;
                        Debug.Log($"🎆 Created fallback system for {effectType}");
                    }
                }
            }
            
            Debug.Log($"🎆 Created {fallbackParticleSystems.Count} fallback particle systems");
        }
        
        /// <summary>
        /// Create a simple fallback particle system for a specific effect type
        /// </summary>
        private ParticleSystem CreateFallbackParticleSystem(ParticleEffectType effectType)
        {
            GameObject particleObj = new GameObject($"Fallback_{effectType}");
            particleObj.transform.SetParent(transform);
            
            ParticleSystem particleSystem = particleObj.AddComponent<ParticleSystem>();
            var main = particleSystem.main;
            var emission = particleSystem.emission;
            var shape = particleSystem.shape;
            var colorOverLifetime = particleSystem.colorOverLifetime;
            var sizeOverLifetime = particleSystem.sizeOverLifetime;
            var velocityOverLifetime = particleSystem.velocityOverLifetime;
            var rotationOverLifetime = particleSystem.rotationOverLifetime;
            
            // Configure based on effect type with enhanced settings
            switch (effectType)
            {
                case ParticleEffectType.ScoreGain:
                    main.startColor = Color.green;
                    main.startSpeed = 4f;
                    main.startSize = 0.8f; // Bigger particles
                    main.startLifetime = 2f;
                    main.startRotation = 0f;
                    main.maxParticles = 100;
                    emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 25) });
                    
                    // Enhanced effects
                    sizeOverLifetime.enabled = true;
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
                    
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-2f, 2f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(3f, 6f);
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
                    break;
                    
                case ParticleEffectType.ScoreLoss:
                    main.startColor = Color.red;
                    main.startSpeed = 3f;
                    main.startSize = 0.6f; // Bigger particles
                    main.startLifetime = 1.8f;
                    main.startRotation = 0f;
                    main.maxParticles = 80;
                    emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 18) });
                    
                    // Enhanced effects
                    sizeOverLifetime.enabled = true;
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
                    
                                         velocityOverLifetime.enabled = true;
                     velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);
                     velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-2f, 1f); // Fall down
                     velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
                    break;
                    
                case ParticleEffectType.XPGain:
                    main.startColor = Color.blue;
                    main.startSpeed = 5f;
                    main.startSize = 1.0f; // Bigger particles
                    main.startLifetime = 2.5f;
                    main.startRotation = 0f;
                    main.maxParticles = 120;
                    emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 30) });
                    
                    // Enhanced effects
                    sizeOverLifetime.enabled = true;
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(0.6f, 1.8f);
                    
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-3f, 3f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(4f, 8f); // Shoot up
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
                    
                    // Use start rotation instead for simple rotation
                    main.startRotation = new ParticleSystem.MinMaxCurve(-90f, 90f);
                    break;
                    
                case ParticleEffectType.XPLoss:
                    main.startColor = Color.orange;
                    main.startSpeed = 3.5f;
                    main.startSize = 0.7f;
                    main.startLifetime = 2f;
                    main.startRotation = 0f;
                    main.maxParticles = 90;
                    emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 22) });
                    
                    // Enhanced effects
                    sizeOverLifetime.enabled = true;
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(0.5f, 1.4f);
                    
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-1f, 1f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-1f, 2f);
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
                    break;
                    
                case ParticleEffectType.CoinGain:
                    main.startColor = Color.yellow;
                    main.startSpeed = 4.5f;
                    main.startSize = 0.9f; // Bigger particles
                    main.startLifetime = 2.2f;
                    main.startRotation = 0f;
                    main.maxParticles = 110;
                    emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 28) });
                    
                    // Enhanced effects
                    sizeOverLifetime.enabled = true;
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(0.6f, 1.6f);
                    
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-2.5f, 2.5f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(3f, 7f);
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
                    
                    // Use start rotation instead for coin spinning effect
                    main.startRotation = new ParticleSystem.MinMaxCurve(-180f, 180f);
                    break;
                    
                case ParticleEffectType.CoinLoss:
                    main.startColor = Color.red;
                    main.startSpeed = 3f;
                    main.startSize = 0.6f;
                    main.startLifetime = 1.8f;
                    main.startRotation = 0f;
                    main.maxParticles = 85;
                    emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 20) });
                    
                    // Enhanced effects
                    sizeOverLifetime.enabled = true;
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(0.4f, 1.3f);
                    
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-1f, 1f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-2f, 1f);
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
                    break;
                    
                case ParticleEffectType.StarAchievement:
                    main.startColor = Color.magenta;
                    main.startSpeed = 6f;
                    main.startSize = 1.2f; // Bigger particles
                    main.startLifetime = 3f;
                    main.startRotation = 0f;
                    main.maxParticles = 150;
                    emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 40) });
                    
                    // Enhanced effects
                    sizeOverLifetime.enabled = true;
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(0.8f, 2.0f);
                    
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-4f, 4f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(5f, 10f);
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);
                    
                    // Use start rotation instead for star rotation effect
                    main.startRotation = new ParticleSystem.MinMaxCurve(-360f, 360f);
                    break;
                    
                case ParticleEffectType.StarLoss:
                    main.startColor = Color.red;
                    main.startSpeed = 4f;
                    main.startSize = 0.8f;
                    main.startLifetime = 2.2f;
                    main.startRotation = 0f;
                    main.maxParticles = 100;
                    emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 25) });
                    
                    // Enhanced effects
                    sizeOverLifetime.enabled = true;
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
                    
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-3f, 2f);
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
                    break;
                    
                case ParticleEffectType.LevelUp:
                    main.startColor = Color.cyan;
                    main.startSpeed = 7f;
                    main.startSize = 1.5f; // Much bigger particles
                    main.startLifetime = 3.5f;
                    main.startRotation = 0f;
                    main.maxParticles = 200;
                    emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 60) });
                    
                    // Enhanced effects
                    sizeOverLifetime.enabled = true;
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, 2.5f);
                    
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-5f, 5f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(6f, 12f);
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-2f, 2f);
                    
                    // Use start rotation for particle rotation
                    main.startRotation = new ParticleSystem.MinMaxCurve(-720f, 720f); // Double rotation
                    break;
                    
                case ParticleEffectType.Milestone:
                    main.startColor = Color.purple;
                    main.startSpeed = 5.5f;
                    main.startSize = 1.3f; // Bigger particles
                    main.startLifetime = 3f;
                    main.startRotation = 0f;
                    main.maxParticles = 180;
                    emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 50) });
                    
                    // Enhanced effects
                    sizeOverLifetime.enabled = true;
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(0.8f, 2.2f);
                    
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-4f, 4f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(4f, 9f);
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);
                    
                    // Use start rotation for particle rotation
                    main.startRotation = new ParticleSystem.MinMaxCurve(-540f, 540f);
                    break;
                    
                case ParticleEffectType.StreakBonus:
                    main.startColor = Color.yellow;
                    main.startSpeed = 6f;
                    main.startSize = 1.1f; // Bigger particles
                    main.startLifetime = 2.8f;
                    main.startRotation = 0f;
                    main.maxParticles = 160;
                    emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 45) });
                    
                    // Enhanced effects
                    sizeOverLifetime.enabled = true;
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(0.7f, 1.9f);
                    
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-4f, 4f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(5f, 10f);
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);
                    
                    // Use start rotation for particle rotation
                    main.startRotation = new ParticleSystem.MinMaxCurve(-450f, 450f);
                    break;
                    
                case ParticleEffectType.SpeedTier:
                    main.startColor = Color.cyan;
                    main.startSpeed = 8f;
                    main.startSize = 1.4f; // Bigger particles
                    main.startLifetime = 3f;
                    main.startRotation = 0f;
                    main.maxParticles = 180;
                    emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 55) });
                    
                    // Enhanced effects
                    sizeOverLifetime.enabled = true;
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(0.9f, 2.3f);
                    
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-5f, 5f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(6f, 11f);
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-2f, 2f);
                    
                    // Use start rotation for particle rotation
                    main.startRotation = new ParticleSystem.MinMaxCurve(-600f, 600f);
                    break;
                    
                case ParticleEffectType.PerfectAccuracy:
                    main.startColor = Color.green;
                    main.startSpeed = 5f;
                    main.startSize = 1.0f; // Bigger particles
                    main.startLifetime = 2.5f;
                    main.startRotation = 0f;
                    main.maxParticles = 140;
                    emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 35) });
                    
                    // Enhanced effects
                    sizeOverLifetime.enabled = true;
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(0.6f, 1.7f);
                    
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-3f, 3f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(4f, 8f);
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
                    
                    // Use start rotation for particle rotation
                    main.startRotation = new ParticleSystem.MinMaxCurve(-270f, 270f);
                    break;
                    
                case ParticleEffectType.PersonalBest:
                    main.startColor = Color.gold;
                    main.startSpeed = 7f;
                    main.startSize = 1.6f; // Much bigger particles
                    main.startLifetime = 4f;
                    main.startRotation = 0f;
                    main.maxParticles = 250;
                    emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 80) });
                    
                    // Enhanced effects
                    sizeOverLifetime.enabled = true;
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.2f, 3.0f);
                    
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-6f, 6f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(7f, 14f);
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-2.5f, 2.5f);
                    
                    // Use start rotation for particle rotation
                    main.startRotation = new ParticleSystem.MinMaxCurve(-900f, 900f); // Triple rotation
                    break;
                    
                default:
                    main.startColor = Color.white;
                    main.startSpeed = 4f;
                    main.startSize = 0.8f; // Bigger default particles
                    main.startLifetime = 2f;
                    main.startRotation = 0f;
                    main.maxParticles = 100;
                    emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 25) });
                    
                    // Enhanced effects
                    sizeOverLifetime.enabled = true;
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
                    
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-2f, 2f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(3f, 6f);
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
                    break;
            }
            
            // Common enhanced settings
            main.duration = 3f;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            // Enhanced shape
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f; // Bigger spawn area
            shape.arc = 360f;
            shape.randomDirectionAmount = 0.3f; // Add some randomness
            
            // Enhanced color over lifetime with better gradients
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            
            // Create more interesting color transitions
            switch (effectType)
            {
                case ParticleEffectType.ScoreGain:
                    gradient.SetKeys(
                        new GradientColorKey[] { 
                            new GradientColorKey(Color.green, 0.0f), 
                            new GradientColorKey(Color.yellow, 0.5f), 
                            new GradientColorKey(Color.green, 1.0f) 
                        },
                        new GradientAlphaKey[] { 
                            new GradientAlphaKey(1.0f, 0.0f), 
                            new GradientAlphaKey(0.8f, 0.5f), 
                            new GradientAlphaKey(0.0f, 1.0f) 
                        }
                    );
                    break;
                    
                case ParticleEffectType.XPGain:
                    gradient.SetKeys(
                        new GradientColorKey[] { 
                            new GradientColorKey(Color.blue, 0.0f), 
                            new GradientColorKey(Color.cyan, 0.5f), 
                            new GradientColorKey(Color.blue, 1.0f) 
                        },
                        new GradientAlphaKey[] { 
                            new GradientAlphaKey(1.0f, 0.0f), 
                            new GradientAlphaKey(0.9f, 0.5f), 
                            new GradientAlphaKey(0.0f, 1.0f) 
                        }
                    );
                    break;
                    
                case ParticleEffectType.CoinGain:
                    gradient.SetKeys(
                        new GradientColorKey[] { 
                            new GradientColorKey(Color.yellow, 0.0f), 
                            new GradientColorKey(Color.gold, 0.5f), 
                            new GradientColorKey(Color.yellow, 1.0f) 
                        },
                        new GradientAlphaKey[] { 
                            new GradientAlphaKey(1.0f, 0.0f), 
                            new GradientAlphaKey(0.8f, 0.5f), 
                            new GradientAlphaKey(0.0f, 1.0f) 
                        }
                    );
                    break;
                    
                case ParticleEffectType.StarAchievement:
                    gradient.SetKeys(
                        new GradientColorKey[] { 
                            new GradientColorKey(Color.magenta, 0.0f), 
                            new GradientColorKey(Color.pink, 0.3f), 
                            new GradientColorKey(Color.white, 0.7f), 
                            new GradientColorKey(Color.magenta, 1.0f) 
                        },
                        new GradientAlphaKey[] { 
                            new GradientAlphaKey(1.0f, 0.0f), 
                            new GradientAlphaKey(0.9f, 0.3f), 
                            new GradientAlphaKey(0.7f, 0.7f), 
                            new GradientAlphaKey(0.0f, 1.0f) 
                        }
                    );
                    break;
                    
                case ParticleEffectType.LevelUp:
                    gradient.SetKeys(
                        new GradientColorKey[] { 
                            new GradientColorKey(Color.cyan, 0.0f), 
                            new GradientColorKey(Color.blue, 0.3f), 
                            new GradientColorKey(Color.white, 0.7f), 
                            new GradientColorKey(Color.cyan, 1.0f) 
                        },
                        new GradientAlphaKey[] { 
                            new GradientAlphaKey(1.0f, 0.0f), 
                            new GradientAlphaKey(0.9f, 0.3f), 
                            new GradientAlphaKey(0.8f, 0.7f), 
                            new GradientAlphaKey(0.0f, 1.0f) 
                        }
                    );
                    break;
                    
                default:
                    gradient.SetKeys(
                        new GradientColorKey[] { 
                            new GradientColorKey(main.startColor.color, 0.0f), 
                            new GradientColorKey(main.startColor.color, 1.0f) 
                        },
                        new GradientAlphaKey[] { 
                            new GradientAlphaKey(1.0f, 0.0f), 
                            new GradientAlphaKey(0.0f, 1.0f) 
                        }
                    );
                    break;
            }
            
            colorOverLifetime.color = gradient;
            
            // Stop and clear initially
            particleSystem.Stop();
            particleSystem.Clear();
            
            return particleSystem;
        }
        
        /// <summary>
        /// Initialize particle pooling system
        /// </summary>
        private void InitializeParticlePool()
        {
            particlePool.Clear();
            
            // If we have prefabs, use them for pooling
            if (particlePrefabCache.Count > 0)
            {
                foreach (var kvp in particlePrefabCache)
                {
                    for (int i = 0; i < particlePoolSize / particlePrefabCache.Count; i++)
                    {
                        GameObject particleObj = Instantiate(kvp.Value);
                        ParticleSystem particleSystem = particleObj.GetComponent<ParticleSystem>();
                        
                        if (particleSystem != null)
                        {
                            particleSystem.Stop();
                            particleSystem.Clear();
                            particleObj.SetActive(false);
                            particlePool.Enqueue(particleSystem);
                        }
                    }
                }
            }
            // Otherwise, use fallback systems for pooling
            else if (fallbackParticleSystems.Count > 0)
            {
                foreach (var kvp in fallbackParticleSystems)
                {
                    for (int i = 0; i < particlePoolSize / fallbackParticleSystems.Count; i++)
                    {
                        GameObject particleObj = Instantiate(kvp.Value.gameObject);
                        ParticleSystem particleSystem = particleObj.GetComponent<ParticleSystem>();
                        
                        if (particleSystem != null)
                        {
                            particleSystem.Stop();
                            particleSystem.Clear();
                            particleObj.SetActive(false);
                            particlePool.Enqueue(particleSystem);
                        }
                    }
                }
            }
            
            Debug.Log($"🎆 Particle pool initialized with {particlePool.Count} particles");
        }
        
        /// <summary>
        /// Initialize event system
        /// </summary>
        private void InitializeEventSystem()
        {
            if (eventSystem != null)
            {
                Debug.Log("🎆 Event system initialized");
            }
        }
        
        /// <summary>
        /// Apply global configuration settings
        /// </summary>
        private void ApplyGlobalConfig()
        {
            if (globalConfig == null) return;
            
            Debug.Log("🎆 Applying global particle VFX configuration...");
            
            // Apply configuration settings
            enableParticlePooling = globalConfig.defaultEnablePooling;
            particlePoolSize = globalConfig.defaultParticlePoolSize;
            
            Debug.Log("🎆 Global configuration applied successfully!");
        }
        
        /// <summary>
        /// Play particle effect at targeted UI component
        /// </summary>
        public void PlayParticleEffect(ParticleEffectType effectType, Vector3 position = default)
        {
            if (!enableGlobalSystem) 
            {
                Debug.LogWarning("🎆 Global particle system is disabled!");
                return;
            }
            
            // Get target position if not specified
            if (position == default)
            {
                position = GetTargetPositionForEffect(effectType);
            }
            
            Debug.Log($"🎆 Attempting to play {effectType} at position {position}");
            
            // Get particle system from pool or create new one
            ParticleSystem particleSystem = GetParticleSystemFromPool(effectType);
            
            if (particleSystem != null)
            {
                // Configure and play particle system
                ConfigureParticleSystem(particleSystem, effectType, position);
                particleSystem.Play();
                
                // Add to active systems
                if (!activeParticleSystems.Contains(particleSystem))
                {
                    activeParticleSystems.Add(particleSystem);
                }
                
                // Return to pool after delay
                StartCoroutine(ReturnParticleToPool(particleSystem, globalConfig?.defaultParticleDuration ?? 2f));
                
                // Trigger event
                if (eventSystem != null)
                {
                    eventSystem.TriggerParticleEvent(effectType, position);
                }
                
                Debug.Log($"🎆 Successfully playing {effectType} particle effect at {position}");
            }
            else
            {
                Debug.LogError($"🎆 Failed to get particle system for {effectType}!");
            }
        }
        
        /// <summary>
        /// Get target position for specific effect type
        /// </summary>
        private Vector3 GetTargetPositionForEffect(ParticleEffectType effectType)
        {
            Vector3 targetPosition = Vector3.zero;
            
            switch (effectType)
            {
                case ParticleEffectType.ScoreGain:
                case ParticleEffectType.ScoreLoss:
                    targetPosition = GetUIPosition(primaryScoreText);
                    break;
                    
                case ParticleEffectType.XPGain:
                case ParticleEffectType.XPLoss:
                    targetPosition = GetUIPosition(primaryXPText);
                    break;
                    
                case ParticleEffectType.CoinGain:
                case ParticleEffectType.CoinLoss:
                    targetPosition = GetUIPosition(primaryCoinText);
                    break;
                    
                case ParticleEffectType.StarAchievement:
                case ParticleEffectType.StarLoss:
                    targetPosition = GetUIPosition(primaryStarDisplay);
                    break;
                    
                case ParticleEffectType.LevelUp:
                    targetPosition = GetUIPosition(primaryLevelText);
                    break;
                    
                case ParticleEffectType.StreakBonus:
                    targetPosition = GetUIPosition(primaryStreakText);
                    break;
                    
                case ParticleEffectType.Milestone:
                    targetPosition = GetUIPosition(primaryMilestoneText);
                    break;
                    
                default:
                    targetPosition = GetDefaultPosition();
                    break;
            }
            
            Debug.Log($"🎆 Target position for {effectType}: {targetPosition}");
            return targetPosition;
        }
        
        /// <summary>
        /// Get UI component position in world space
        /// </summary>
        private Vector3 GetUIPosition(Component uiComponent)
        {
            if (uiComponent == null) 
            {
                Debug.LogWarning("🎆 UI Component is null, using default position");
                return GetDefaultPosition();
            }
            
            Vector3 worldPos = uiComponent.transform.position;
            
            // Handle different canvas render modes
            Canvas canvas = uiComponent.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                switch (canvas.renderMode)
                {
                    case RenderMode.ScreenSpaceOverlay:
                        if (Camera.main != null)
                        {
                            Vector3 screenPos = uiComponent.transform.position;
                            // Ensure particles spawn in front of camera, not behind
                            worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f));
                        }
                        break;
                        
                    case RenderMode.ScreenSpaceCamera:
                        if (canvas.worldCamera != null)
                        {
                            Vector3 canvasScreenPos = canvas.worldCamera.WorldToScreenPoint(uiComponent.transform.position);
                            // Ensure particles spawn in front of camera, not behind
                            worldPos = canvas.worldCamera.ScreenToWorldPoint(new Vector3(canvasScreenPos.x, canvasScreenPos.y, 5f));
                        }
                        break;
                        
                    case RenderMode.WorldSpace:
                        // Already in world space, but ensure it's visible
                        worldPos = uiComponent.transform.position;
                        break;
                }
            }
            
            Debug.Log($"🎆 UI Component {uiComponent.name} position: {worldPos}");
            return worldPos;
        }
        
        /// <summary>
        /// Get UI GameObject position in world space
        /// </summary>
        private Vector3 GetUIPosition(GameObject uiGameObject)
        {
            if (uiGameObject == null) 
            {
                Debug.LogWarning("🎆 UI GameObject is null, using default position");
                return GetDefaultPosition();
            }
            
            Vector3 worldPos = uiGameObject.transform.position;
            
            // Handle different canvas render modes
            Canvas canvas = uiGameObject.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                switch (canvas.renderMode)
                {
                    case RenderMode.ScreenSpaceOverlay:
                        if (Camera.main != null)
                        {
                            Vector3 screenPos = uiGameObject.transform.position;
                            // Ensure particles spawn in front of camera, not behind
                            worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f));
                        }
                        break;
                        
                    case RenderMode.ScreenSpaceCamera:
                        if (canvas.worldCamera != null)
                        {
                            Vector3 canvasScreenPos = canvas.worldCamera.WorldToScreenPoint(uiGameObject.transform.position);
                            // Ensure particles spawn in front of camera, not behind
                            worldPos = canvas.worldCamera.ScreenToWorldPoint(new Vector3(canvasScreenPos.x, canvasScreenPos.y, 5f));
                        }
                        break;
                        
                    case RenderMode.WorldSpace:
                        // Already in world space, but ensure it's visible
                        worldPos = uiGameObject.transform.position;
                        break;
                }
            }
            
            Debug.Log($"🎆 UI GameObject {uiGameObject.name} position: {worldPos}");
            return worldPos;
        }
        
        /// <summary>
        /// Get default particle position
        /// </summary>
        private Vector3 GetDefaultPosition()
        {
            Vector3 defaultPos;
            if (Camera.main != null)
            {
                // Position particles in front of camera, not behind
                defaultPos = Camera.main.transform.position + Camera.main.transform.forward * 5f;
            }
            else
            {
                defaultPos = Vector3.zero;
            }
            
            Debug.Log($"🎆 Using default position: {defaultPos}");
            return defaultPos;
        }
        
        /// <summary>
        /// Get particle system from pool
        /// </summary>
        private ParticleSystem GetParticleSystemFromPool(ParticleEffectType effectType)
        {
            // Try to get from pool first
            if (enableParticlePooling && particlePool.Count > 0)
            {
                ParticleSystem pooledSystem = particlePool.Dequeue();
                Debug.Log($"🎆 Got particle system from pool for {effectType}");
                return pooledSystem;
            }
            
            // Try to create from prefab cache
            if (particlePrefabCache.ContainsKey(effectType))
            {
                GameObject particleObj = Instantiate(particlePrefabCache[effectType]);
                ParticleSystem particleSystem = particleObj.GetComponent<ParticleSystem>();
                Debug.Log($"🎆 Created particle system from prefab for {effectType}");
                return particleSystem;
            }
            
            // Use fallback system
            if (fallbackParticleSystems.ContainsKey(effectType))
            {
                GameObject particleObj = Instantiate(fallbackParticleSystems[effectType].gameObject);
                ParticleSystem particleSystem = particleObj.GetComponent<ParticleSystem>();
                Debug.Log($"🎆 Created particle system from fallback for {effectType}");
                return particleSystem;
            }
            
            Debug.LogError($"🎆 No particle system available for {effectType}!");
            return null;
        }
        
        /// <summary>
        /// Ensure particles are visible by setting proper layer and renderer settings
        /// </summary>
        private void EnsureParticleVisibility(ParticleSystem particleSystem)
        {
            if (particleSystem == null) return;
            
            // Set particle system to default layer (0) for proper rendering
            particleSystem.gameObject.layer = 0;
            
            // Get the particle system renderer and ensure it's visible
            var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                // Ensure renderer is enabled
                renderer.enabled = true;
                
                // Set proper sorting order for UI particles
                renderer.sortingOrder = 100; // High sorting order to appear above UI
                
                // Set proper sorting layer
                renderer.sortingLayerName = "Default";
                
                // Ensure particles are not culled
                renderer.allowRoll = true;
                
                Debug.Log($"🎆 Configured particle renderer for visibility: {particleSystem.name}");
            }
            
            // Ensure the particle system itself is enabled
            var emission = particleSystem.emission;
            emission.enabled = true;
            particleSystem.playOnAwake = false; // We'll control this manually
            
            Debug.Log($"🎆 Ensured particle visibility for: {particleSystem.name}");
        }
        
        /// <summary>
        /// Configure particle gravity and falling speed
        /// </summary>
        private void ConfigureParticleGravity(ParticleSystem particleSystem, ParticleEffectType effectType)
        {
            if (particleSystem == null) return;
            
            var main = particleSystem.main;
            var externalForces = particleSystem.externalForces;
            var velocityOverLifetime = particleSystem.velocityOverLifetime;
            
            // Configure based on effect type
            switch (effectType)
            {
                case ParticleEffectType.ScoreGain:
                case ParticleEffectType.XPGain:
                case ParticleEffectType.CoinGain:
                case ParticleEffectType.StarAchievement:
                    // Rising particles - minimal gravity, upward velocity
                    main.gravityModifier = 0.1f; // Very light gravity
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-2f, 2f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(3f, 8f); // Rise up
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
                    break;
                    
                case ParticleEffectType.ScoreLoss:
                case ParticleEffectType.XPLoss:
                case ParticleEffectType.CoinLoss:
                case ParticleEffectType.StarLoss:
                    // Falling particles - normal gravity, downward velocity
                    main.gravityModifier = 1.0f; // Normal gravity
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-2f, 2f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-4f, -1f); // Fall down
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
                    break;
                    
                case ParticleEffectType.LevelUp:
                case ParticleEffectType.Milestone:
                case ParticleEffectType.StreakBonus:
                case ParticleEffectType.SpeedTier:
                case ParticleEffectType.PerfectAccuracy:
                case ParticleEffectType.PersonalBest:
                    // Floating particles - light gravity, gentle movement
                    main.gravityModifier = 0.3f; // Light gravity
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-1f, 2f); // Gentle floating
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
                    break;
                    
                default:
                    // Default - moderate gravity
                    main.gravityModifier = 0.5f;
                    break;
            }
            
            // Enable external forces for additional control
            externalForces.enabled = true;
            externalForces.multiplier = 1.0f;
        }

        /// <summary>
        /// Configure particle system for specific effect
        /// </summary>
        private void ConfigureParticleSystem(ParticleSystem particleSystem, ParticleEffectType effectType, Vector3 position)
        {
            if (particleSystem == null) return;
            
            // Set position with proper depth for visibility
            Vector3 spawnOffset = globalConfig?.defaultSpawnOffset ?? new Vector3(0, 0, 0);
            Vector3 finalPosition = position + spawnOffset;
            
            // Ensure particles are in front of camera and visible
            if (Camera.main != null)
            {
                // If position is behind camera, move it in front
                Vector3 cameraForward = Camera.main.transform.forward;
                Vector3 cameraPos = Camera.main.transform.position;
                
                if (Vector3.Dot(finalPosition - cameraPos, cameraForward) < 0)
                {
                    finalPosition = cameraPos + cameraForward * 5f;
                }
            }
            
            particleSystem.transform.position = finalPosition;
            
            Debug.Log($"🎆 Configured particle system at position: {particleSystem.transform.position}");
            
            // Ensure particles are visible by setting proper layer and renderer settings
            EnsureParticleVisibility(particleSystem);
            
            // Configure gravity and falling speed
            ConfigureParticleGravity(particleSystem, effectType);
            
            // Set burst count if global config is available
            if (globalConfig != null)
            {
                var emission = particleSystem.emission;
                int burstCount = globalConfig.GetAdjustedBurstCount(GetBaseBurstCount(effectType));
                var burst = new ParticleSystem.Burst(0.0f, burstCount);
                emission.SetBursts(new ParticleSystem.Burst[] { burst });
                
                // Set color
                var main = particleSystem.main;
                main.startColor = globalConfig.GetParticleColor(effectType);
            }
            
            // Ensure the particle system is active
            particleSystem.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Get base burst count for effect type
        /// </summary>
        private int GetBaseBurstCount(ParticleEffectType effectType)
        {
            if (globalConfig == null) return 20;
            
            switch (effectType)
            {
                case ParticleEffectType.ScoreGain:
                case ParticleEffectType.ScoreLoss:
                    return globalConfig.burstCounts.scoreParticles;
                    
                case ParticleEffectType.XPGain:
                case ParticleEffectType.XPLoss:
                    return globalConfig.burstCounts.xpParticles;
                    
                case ParticleEffectType.CoinGain:
                case ParticleEffectType.CoinLoss:
                    return globalConfig.burstCounts.coinParticles;
                    
                case ParticleEffectType.StarAchievement:
                case ParticleEffectType.StarLoss:
                    return globalConfig.burstCounts.starParticles;
                    
                case ParticleEffectType.LevelUp:
                    return globalConfig.burstCounts.levelUpParticles;
                    
                case ParticleEffectType.Milestone:
                    return globalConfig.burstCounts.achievementParticles;
                    
                default:
                    return 20;
            }
        }
        
        /// <summary>
        /// Return particle system to pool
        /// </summary>
        private System.Collections.IEnumerator ReturnParticleToPool(ParticleSystem particleSystem, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (particleSystem != null)
            {
                particleSystem.Stop();
                particleSystem.Clear();
                particleSystem.gameObject.SetActive(false);
                
                // Remove from active systems
                if (activeParticleSystems.Contains(particleSystem))
                {
                    activeParticleSystems.Remove(particleSystem);
                }
                
                if (enableParticlePooling)
                {
                    particlePool.Enqueue(particleSystem);
                    Debug.Log($"🎆 Returned particle system to pool");
                }
                else
                {
                    Destroy(particleSystem.gameObject);
                    Debug.Log($"🎆 Destroyed particle system");
                }
            }
        }
        
        /// <summary>
        /// Set particle gravity modifier for a specific particle system
        /// </summary>
        /// <param name="particleSystem">Target particle system</param>
        /// <param name="gravityModifier">Gravity strength (0 = no gravity, 1 = normal, 2 = double)</param>
        public void SetParticleGravity(ParticleSystem particleSystem, float gravityModifier)
        {
            if (particleSystem == null) return;
            
            var main = particleSystem.main;
            main.gravityModifier = Mathf.Clamp(gravityModifier, 0f, 5f);
            
            Debug.Log($"🎆 Set particle gravity to {gravityModifier} for {particleSystem.name}");
        }
        
        /// <summary>
        /// Set particle falling speed using velocity over lifetime
        /// </summary>
        /// <param name="particleSystem">Target particle system</param>
        /// <param name="fallSpeed">Falling speed (negative = down, positive = up)</param>
        /// <param name="speedVariation">Random variation in speed</param>
        public void SetParticleFallSpeed(ParticleSystem particleSystem, float fallSpeed, float speedVariation = 0f)
        {
            if (particleSystem == null) return;
            
            var velocityOverLifetime = particleSystem.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            
            // Set all velocity axes to maintain consistency
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(0f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(0f);
            
            if (speedVariation > 0f)
            {
                velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(fallSpeed - speedVariation, fallSpeed + speedVariation);
            }
            else
            {
                velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(fallSpeed);
            }
            
            Debug.Log($"🎆 Set particle fall speed to {fallSpeed} for {particleSystem.name}");
        }
        
        /// <summary>
        /// Set particle size/scale for a specific particle system
        /// </summary>
        /// <param name="particleSystem">Target particle system</param>
        /// <param name="size">Particle size (0.1 = tiny, 1.0 = normal, 3.0 = huge)</param>
        /// <param name="sizeVariation">Random variation in size (optional)</param>
        public void SetParticleSize(ParticleSystem particleSystem, float size, float sizeVariation = 0f)
        {
            if (particleSystem == null) return;
            
            var main = particleSystem.main;
            
            if (sizeVariation > 0f)
            {
                main.startSize = new ParticleSystem.MinMaxCurve(size - sizeVariation, size + sizeVariation);
            }
            else
            {
                main.startSize = new ParticleSystem.MinMaxCurve(size);
            }
            
            Debug.Log($"🎆 Set particle size to {size} for {particleSystem.name}");
        }
        
        /// <summary>
        /// Set particle size over lifetime for dynamic scaling effects
        /// </summary>
        /// <param name="particleSystem">Target particle system</param>
        /// <param name="startSize">Initial size</param>
        /// <param name="endSize">Final size</param>
        /// <param name="sizeCurve">Optional custom size curve</param>
        public void SetParticleSizeOverLifetime(ParticleSystem particleSystem, float startSize, float endSize, AnimationCurve sizeCurve = null)
        {
            if (particleSystem == null) return;
            
            var sizeOverLifetime = particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            
            if (sizeCurve != null)
            {
                sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
            }
            else
            {
                sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(startSize, endSize);
            }
            
            Debug.Log($"🎆 Set particle size over lifetime from {startSize} to {endSize} for {particleSystem.name}");
        }
        
        /// <summary>
        /// Create particles with custom gravity, size, and movement settings
        /// </summary>
        /// <param name="effectType">Type of particle effect</param>
        /// <param name="gravityModifier">Gravity strength (0-2)</param>
        /// <param name="fallSpeed">Vertical movement speed</param>
        /// <param name="particleSize">Particle size (0.1-5.0)</param>
        /// <param name="sizeVariation">Random size variation</param>
        public void PlayCustomParticles(ParticleEffectType effectType, float gravityModifier = 0.2f, float fallSpeed = 0f, float particleSize = 1.0f, float sizeVariation = 0f)
        {
            if (!enableGlobalSystem) return;
            
            // Get target position
            Vector3 position = GetTargetPositionForEffect(effectType);
            
            // Get particle system
            ParticleSystem particleSystem = GetParticleSystemFromPool(effectType);
            
            if (particleSystem != null)
            {
                // Configure position with proper depth for visibility
                Vector3 spawnOffset = globalConfig?.defaultSpawnOffset ?? new Vector3(0, 0, 0);
                Vector3 finalPosition = position + spawnOffset;
                
                // Ensure particles are in front of camera and visible
                if (Camera.main != null)
                {
                    // If position is behind camera, move it in front
                    Vector3 cameraForward = Camera.main.transform.forward;
                    Vector3 cameraPos = Camera.main.transform.position;
                    
                    if (Vector3.Dot(finalPosition - cameraPos, cameraForward) < 0)
                    {
                        finalPosition = cameraPos + cameraForward * 5f;
                    }
                }
                
                particleSystem.transform.position = finalPosition;
                
                // Configure custom gravity
                var main = particleSystem.main;
                main.gravityModifier = Mathf.Clamp(gravityModifier, 0f, 2f);
                
                // Configure custom size
                SetParticleSize(particleSystem, particleSize, sizeVariation);
                
                // Configure fall speed
                if (fallSpeed != 0f)
                {
                    var velocityOverLifetime = particleSystem.velocityOverLifetime;
                    velocityOverLifetime.enabled = true;
                    // Set all velocity axes to maintain consistency
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(0f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(fallSpeed);
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(0f);
                }
                
                // Play the system
                particleSystem.Play();
                
                // Add to active systems
                if (!activeParticleSystems.Contains(particleSystem))
                {
                    activeParticleSystems.Add(particleSystem);
                }
                
                // Return to pool after delay
                StartCoroutine(ReturnParticleToPool(particleSystem, globalConfig?.defaultParticleDuration ?? 2f));
                
                Debug.Log($"🎆 Playing custom particles with gravity {gravityModifier}, size {particleSize}, and fall speed {fallSpeed}");
            }
        }
        
        /// <summary>
        /// Create a particle system with comprehensive custom settings
        /// </summary>
        /// <param name="effectType">Type of particle effect</param>
        /// <param name="settings">Custom particle settings</param>
        public void PlayParticlesWithSettings(ParticleEffectType effectType, ParticleSettings settings)
        {
            if (!enableGlobalSystem) return;
            
            // Get target position
            Vector3 position = GetTargetPositionForEffect(effectType);
            
            // Get particle system
            ParticleSystem particleSystem = GetParticleSystemFromPool(effectType);
            
            if (particleSystem != null)
            {
                // Configure position with proper depth for visibility
                Vector3 spawnOffset = globalConfig?.defaultSpawnOffset ?? new Vector3(0, 0, 0);
                Vector3 finalPosition = position + spawnOffset;
                
                // Ensure particles are in front of camera and visible
                if (Camera.main != null)
                {
                    // If position is behind camera, move it in front
                    Vector3 cameraForward = Camera.main.transform.forward;
                    Vector3 cameraPos = Camera.main.transform.position;
                    
                    if (Vector3.Dot(finalPosition - cameraPos, cameraForward) < 0)
                    {
                        finalPosition = cameraPos + cameraForward * 5f;
                    }
                }
                
                particleSystem.transform.position = finalPosition;
                
                // Apply all custom settings
                ApplyParticleSettings(particleSystem, settings);
                
                // Play the system
                particleSystem.Play();
                
                // Add to active systems
                if (!activeParticleSystems.Contains(particleSystem))
                {
                    activeParticleSystems.Add(particleSystem);
                }
                
                // Return to pool after delay
                StartCoroutine(ReturnParticleToPool(particleSystem, globalConfig?.defaultParticleDuration ?? 2f));
                
                Debug.Log($"🎆 Playing particles with custom settings for {effectType}");
            }
        }
        
        /// <summary>
        /// Apply comprehensive particle settings to a particle system
        /// </summary>
        /// <param name="particleSystem">Target particle system</param>
        /// <param name="settings">Particle settings to apply</param>
        public void ApplyParticleSettings(ParticleSystem particleSystem, ParticleSettings settings)
        {
            if (particleSystem == null || settings == null) return;
            
            var main = particleSystem.main;
            var emission = particleSystem.emission;
            var sizeOverLifetime = particleSystem.sizeOverLifetime;
            var velocityOverLifetime = particleSystem.velocityOverLifetime;
            var colorOverLifetime = particleSystem.colorOverLifetime;
            
            // Apply gravity
            main.gravityModifier = Mathf.Clamp(settings.gravityModifier, 0f, 2f);
            
            // Apply size settings
            if (settings.sizeVariation > 0f)
            {
                main.startSize = new ParticleSystem.MinMaxCurve(settings.particleSize - settings.sizeVariation, settings.particleSize + settings.sizeVariation);
            }
            else
            {
                main.startSize = new ParticleSystem.MinMaxCurve(settings.particleSize);
            }
            
            // Apply size over lifetime
            if (settings.useSizeOverLifetime)
            {
                sizeOverLifetime.enabled = true;
                sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(settings.startSize, settings.endSize);
            }
            
            // Apply velocity settings
            if (settings.fallSpeed != 0f)
            {
                velocityOverLifetime.enabled = true;
                // Set all velocity axes to maintain consistency
                velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(0f);
                velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(settings.fallSpeed);
                velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(0f);
            }
            
            // Apply color settings
            if (settings.useCustomColor)
            {
                main.startColor = settings.particleColor;
            }
            
            // Apply emission settings
            if (settings.customBurstCount > 0)
            {
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, settings.customBurstCount) });
            }
            
            Debug.Log($"🎆 Applied comprehensive settings to {particleSystem.name}");
        }

        /// <summary>
        /// Convenience methods for common particle effects
        /// </summary>
        public void PlayScoreGainParticles() => PlayParticleEffect(ParticleEffectType.ScoreGain);
        public void PlayScoreLossParticles() => PlayParticleEffect(ParticleEffectType.ScoreLoss);
        public void PlayXPGainParticles() => PlayParticleEffect(ParticleEffectType.XPGain);
        public void PlayXPLossParticles() => PlayParticleEffect(ParticleEffectType.XPLoss);
        public void PlayCoinGainParticles() => PlayParticleEffect(ParticleEffectType.CoinGain);
        public void PlayCoinLossParticles() => PlayParticleEffect(ParticleEffectType.CoinLoss);
        public void PlayStarAchievementParticles() => PlayParticleEffect(ParticleEffectType.StarAchievement);
        public void PlayStarLossParticles() => PlayParticleEffect(ParticleEffectType.StarLoss);
        public void PlayLevelUpParticles() => PlayParticleEffect(ParticleEffectType.LevelUp);
        public void PlayMilestoneParticles() => PlayParticleEffect(ParticleEffectType.Milestone);
        
        /// <summary>
        /// Get system status
        /// </summary>
        public string GetSystemStatus()
        {
            return $"=== Global Particle VFX System Status ===\n" +
                   $"Enabled: {enableGlobalSystem}\n" +
                   $"Global Config: {(globalConfig != null ? "✅" : "❌")}\n" +
                   $"Prefab Collection: {(prefabCollection != null ? "✅" : "❌")}\n" +
                   $"Event System: {(eventSystem != null ? "✅" : "❌")}\n" +
                   $"Particle Pooling: {enableParticlePooling}\n" +
                   $"Pool Size: {particlePoolSize}\n" +
                   $"Active Particles: {activeParticleSystems.Count}\n" +
                   $"Pooled Particles: {particlePool.Count}\n" +
                   $"Cached Prefabs: {particlePrefabCache.Count}\n" +
                   $"Fallback Systems: {fallbackParticleSystems.Count}";
        }
        
        /// <summary>
        /// Test the global particle system
        /// </summary>
        [ContextMenu("Test Global Particle System")]
        public void TestGlobalParticleSystem()
        {
            Debug.Log("🎆 Testing Global Particle VFX System...");
            
            // Check camera settings first
            CheckCameraSettings();
            
            // Test all particle effect types
            foreach (ParticleEffectType effectType in System.Enum.GetValues(typeof(ParticleEffectType)))
            {
                PlayParticleEffect(effectType);
            }
            
            Debug.Log("🎆 Global particle system test completed!");
        }
        
        /// <summary>
        /// Check camera settings for particle visibility
        /// </summary>
        private void CheckCameraSettings()
        {
            if (Camera.main != null)
            {
                Camera cam = Camera.main;
                Debug.Log($"🎆 Camera Settings Check:");
                Debug.Log($"🎆 Camera Position: {cam.transform.position}");
                Debug.Log($"🎆 Camera Forward: {cam.transform.forward}");
                Debug.Log($"🎆 Camera Near Clip: {cam.nearClipPlane}");
                Debug.Log($"🎆 Camera Far Clip: {cam.farClipPlane}");
                Debug.Log($"🎆 Camera Field of View: {cam.fieldOfView}");
                Debug.Log($"🎆 Camera Culling Mask: {cam.cullingMask}");
            }
            else
            {
                Debug.LogWarning("🎆 No main camera found!");
            }
        }
        
        /// <summary>
        /// Test individual particle effects
        /// </summary>
        [ContextMenu("Test Score Particles")]
        public void TestScoreParticles()
        {
            Debug.Log("🎆 Testing Score Particles...");
            PlayScoreGainParticles();
            PlayScoreLossParticles();
        }
        
        [ContextMenu("Test XP Particles")]
        public void TestXPParticles()
        {
            Debug.Log("🎆 Testing XP Particles...");
            PlayXPGainParticles();
        }
        
        [ContextMenu("Test Coin Particles")]
        public void TestCoinParticles()
        {
            Debug.Log("🎆 Testing Coin Particles...");
            PlayCoinGainParticles();
        }
        
        /// <summary>
        /// Test different gravity settings
        /// </summary>
        [ContextMenu("Test Gravity Settings")]
        public void TestGravitySettings()
        {
            Debug.Log("🎆 Testing different gravity settings...");
            
            // Test no gravity (floating particles)
            PlayCustomParticles(ParticleEffectType.ScoreGain, 0f, 0f, 1.0f, 0.2f);
            
            // Test light gravity (gentle falling)
            PlayCustomParticles(ParticleEffectType.CoinGain, 0.3f, -1f, 1.0f, 0.2f);
            
            // Test normal gravity (natural falling)
            PlayCustomParticles(ParticleEffectType.StarAchievement, 1.0f, -3f, 1.0f, 0.2f);
            
            // Test heavy gravity (fast falling)
            PlayCustomParticles(ParticleEffectType.LevelUp, 2.0f, -5f, 1.0f, 0.2f);
        }
        
        /// <summary>
        /// Test different particle sizes
        /// </summary>
        [ContextMenu("Test Particle Sizes")]
        public void TestParticleSizes()
        {
            Debug.Log("🎆 Testing different particle sizes...");
            
            // Test tiny particles
            PlayCustomParticles(ParticleEffectType.ScoreGain, 0.1f, 0f, 0.2f, 0.1f);
            
            // Test small particles
            PlayCustomParticles(ParticleEffectType.CoinGain, 0.1f, 0f, 0.5f, 0.1f);
            
            // Test normal particles
            PlayCustomParticles(ParticleEffectType.StarAchievement, 0.1f, 0f, 1.0f, 0.2f);
            
            // Test large particles
            PlayCustomParticles(ParticleEffectType.LevelUp, 0.1f, 0f, 2.0f, 0.3f);
            
            // Test huge particles
            PlayCustomParticles(ParticleEffectType.PersonalBest, 0.1f, 0f, 4.0f, 0.5f);
        }
        
        /// <summary>
        /// Test combined gravity and size effects
        /// </summary>
        [ContextMenu("Test Gravity + Size Effects")]
        public void TestGravityAndSizeEffects()
        {
            Debug.Log("🎆 Testing gravity and size combinations...");
            
            // Tiny particles with heavy gravity (fast falling)
            PlayCustomParticles(ParticleEffectType.ScoreLoss, 1.5f, -4f, 0.3f, 0.1f);
            
            // Large particles with no gravity (floating)
            PlayCustomParticles(ParticleEffectType.StarAchievement, 0.0f, 0f, 3.0f, 0.5f);
            
            // Medium particles with light gravity (gentle falling)
            PlayCustomParticles(ParticleEffectType.CoinGain, 0.3f, -2f, 1.0f, 0.2f);
            
            // Huge particles with normal gravity (dramatic falling)
            PlayCustomParticles(ParticleEffectType.LevelUp, 1.0f, -3f, 4.0f, 0.8f);
        }

        /// <summary>
        /// Print system status to console
        /// </summary>
        [ContextMenu("Print System Status")]
        public void PrintSystemStatus()
        {
            Debug.Log(GetSystemStatus());
        }
        
        /// <summary>
        /// Force create fallback systems (for testing)
        /// </summary>
        [ContextMenu("Force Create Fallback Systems")]
        public void ForceCreateFallbackSystems()
        {
            CreateFallbackParticleSystems();
        }
    }
}
