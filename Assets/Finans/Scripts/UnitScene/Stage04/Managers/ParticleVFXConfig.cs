using UnityEngine;
using System.Collections.Generic;

namespace ParticleVFXSystem
{
    /// <summary>
    /// Default particle burst counts configuration
    /// </summary>
    [System.Serializable]
    public class ParticleBurstCounts
    {
        [Range(5, 50)]
        public int scoreParticles = 15;
        [Range(10, 100)]
        public int xpParticles = 25;
        [Range(20, 150)]
        public int achievementParticles = 50;
        [Range(30, 200)]
        public int levelUpParticles = 100;
        [Range(15, 75)]
        public int coinParticles = 20;
        [Range(20, 100)]
        public int starParticles = 40;
    }
    
    /// <summary>
    /// Default particle colors configuration
    /// </summary>
    [System.Serializable]
    public class ParticleColors
    {
        public Color scoreGain = Color.green;
        public Color scoreLoss = Color.red;
        public Color xpGain = Color.blue;
        public Color xpLoss = Color.orange;
        public Color coinGain = Color.yellow;
        public Color coinLoss = Color.red;
        public Color starAchievement = Color.magenta;
        public Color starLoss = Color.red;
        public Color levelUp = Color.cyan;
        public Color milestone = Color.purple;
    }
    
    /// <summary>
    /// Configuration asset for global particle VFX system
    /// This asset can be shared across multiple games and mini-games
    /// </summary>
    [CreateAssetMenu(fileName = "ParticleVFXConfig", menuName = "Particle VFX System/Global Config")]
    public class ParticleVFXConfig : ScriptableObject
    {
        [Header("Global Particle Settings")]
        [Tooltip("Default particle duration for all effects")]
        [Range(0.5f, 5f)]
        public float defaultParticleDuration = 2f;
        
        [Tooltip("Default auto-destroy setting for particles")]
        public bool defaultAutoDestroy = true;
        
        [Tooltip("Default particle pool size")]
        [Range(10, 100)]
        public int defaultParticlePoolSize = 50;
        
        [Tooltip("Enable particle pooling by default")]
        public bool defaultEnablePooling = true;
        
        [Header("Particle Spawn Settings")]
        [Tooltip("Default spawn offset from UI components")]
        public Vector3 defaultSpawnOffset = new Vector3(0, 50, 0);
        
        [Tooltip("Default particle burst counts")]
        public ParticleBurstCounts burstCounts = new ParticleBurstCounts();
        
        [Header("Particle Colors")]
        [Tooltip("Default particle colors for different effects")]
        public ParticleColors colors = new ParticleColors();
        
        [Header("Performance Settings")]
        [Tooltip("Maximum concurrent particles allowed")]
        [Range(50, 500)]
        public int maxConcurrentParticles = 200;
        
        [Tooltip("Particle quality level (affects particle count)")]
        [Range(0, 3)]
        public int particleQualityLevel = 2;
        
        [Tooltip("Enable particle culling for off-screen effects")]
        public bool enableParticleCulling = true;
        
        [Header("Audio Integration")]
        [Tooltip("Enable audio feedback for particle effects")]
        public bool enableParticleAudio = true;
        
        [Tooltip("Audio volume for particle effects")]
        [Range(0f, 1f)]
        public float particleAudioVolume = 0.3f;
        
        [Header("Mobile Optimization")]
        [Tooltip("Reduce particle count on mobile devices")]
        public bool mobileOptimization = true;
        
        [Tooltip("Mobile particle reduction factor")]
        [Range(0.1f, 1f)]
        public float mobileParticleFactor = 0.5f;
        
        /// <summary>
        /// Get particle burst count based on quality level
        /// </summary>
        public int GetAdjustedBurstCount(int baseCount)
        {
            if (mobileOptimization && Application.isMobilePlatform)
            {
                return Mathf.RoundToInt(baseCount * mobileParticleFactor);
            }
            
            float qualityMultiplier = 0.5f + (particleQualityLevel * 0.5f);
            return Mathf.RoundToInt(baseCount * qualityMultiplier);
        }
        
        /// <summary>
        /// Get particle color for specific effect type
        /// </summary>
        public Color GetParticleColor(ParticleEffectType effectType)
        {
            switch (effectType)
            {
                case ParticleEffectType.ScoreGain: return colors.scoreGain;
                case ParticleEffectType.ScoreLoss: return colors.scoreLoss;
                case ParticleEffectType.XPGain: return colors.xpGain;
                case ParticleEffectType.XPLoss: return colors.xpLoss;
                case ParticleEffectType.CoinGain: return colors.coinGain;
                case ParticleEffectType.CoinLoss: return colors.coinLoss;
                case ParticleEffectType.StarAchievement: return colors.starAchievement;
                case ParticleEffectType.StarLoss: return colors.starLoss;
                case ParticleEffectType.LevelUp: return colors.levelUp;
                case ParticleEffectType.Milestone: return colors.milestone;
                default: return Color.white;
            }
        }
    }
    
    /// <summary>
    /// Enumeration of particle effect types
    /// </summary>
    public enum ParticleEffectType
    {
        ScoreGain,
        ScoreLoss,
        XPGain,
        XPLoss,
        CoinGain,
        CoinLoss,
        StarAchievement,
        StarLoss,
        LevelUp,
        Milestone,
        StreakBonus,
        SpeedTier,
        PerfectAccuracy,
        PersonalBest
    }
}
