using UnityEngine;
using System.Collections.Generic;

namespace ParticleVFXSystem
{
    /// <summary>
    /// Particle category configuration
    /// </summary>
    [System.Serializable]
    public class ParticleCategory
    {
        public string categoryName;
        public List<GameObject> particlePrefabs = new List<GameObject>();
        public bool useRandomSelection = false;
    }
    
    /// <summary>
    /// Collection of particle VFX prefabs for global use
    /// This asset can be shared across multiple games and mini-games
    /// </summary>
    [CreateAssetMenu(fileName = "ParticleVFXPrefabCollection", menuName = "Particle VFX System/Prefab Collection")]
    public class ParticleVFXPrefabCollection : ScriptableObject
    {
        [Header("Core Particle Systems")]
        [Tooltip("Score addition particle prefab")]
        public GameObject scoreAdditionPrefab;
        
        [Tooltip("Score subtraction particle prefab")]
        public GameObject scoreSubtractionPrefab;
        
        [Tooltip("XP gain particle prefab")]
        public GameObject xpGainPrefab;
        
        [Tooltip("XP loss particle prefab")]
        public GameObject xpLossPrefab;
        
        [Header("Currency & Achievement Particles")]
        [Tooltip("Coin gain particle prefab")]
        public GameObject coinGainPrefab;
        
        [Tooltip("Coin loss particle prefab")]
        public GameObject coinLossPrefab;
        
        [Tooltip("Star achievement particle prefab")]
        public GameObject starAchievementPrefab;
        
        [Tooltip("Star loss particle prefab")]
        public GameObject starLossPrefab;
        
        [Header("Progression Particles")]
        [Tooltip("Level up particle prefab")]
        public GameObject levelUpPrefab;
        
        [Tooltip("Milestone achievement particle prefab")]
        public GameObject milestonePrefab;
        
        [Tooltip("Streak bonus particle prefab")]
        public GameObject streakBonusPrefab;
        
        [Tooltip("Speed tier particle prefab")]
        public GameObject speedTierPrefab;
        
        [Header("Special Achievement Particles")]
        [Tooltip("Perfect accuracy particle prefab")]
        public GameObject perfectAccuracyPrefab;
        
        [Tooltip("Personal best particle prefab")]
        public GameObject personalBestPrefab;
        
        [Header("Particle Variants")]
        [Tooltip("Alternative particle styles for variety")]
        public List<GameObject> alternativeParticlePrefabs = new List<GameObject>();
        
        [Tooltip("Random particle selection for variety")]
        public bool useRandomParticleVariants = false;
        
        [Header("Particle Categories")]
        public List<ParticleCategory> particleCategories = new List<ParticleCategory>();
        
        /// <summary>
        /// Get particle prefab by effect type
        /// </summary>
        public GameObject GetParticlePrefab(ParticleEffectType effectType)
        {
            switch (effectType)
            {
                case ParticleEffectType.ScoreGain: return scoreAdditionPrefab;
                case ParticleEffectType.ScoreLoss: return scoreSubtractionPrefab;
                case ParticleEffectType.XPGain: return xpGainPrefab;
                case ParticleEffectType.XPLoss: return xpLossPrefab;
                case ParticleEffectType.CoinGain: return coinGainPrefab;
                case ParticleEffectType.CoinLoss: return coinLossPrefab;
                case ParticleEffectType.StarAchievement: return starAchievementPrefab;
                case ParticleEffectType.StarLoss: return starLossPrefab;
                case ParticleEffectType.LevelUp: return levelUpPrefab;
                case ParticleEffectType.Milestone: return milestonePrefab;
                case ParticleEffectType.StreakBonus: return streakBonusPrefab;
                case ParticleEffectType.SpeedTier: return speedTierPrefab;
                case ParticleEffectType.PerfectAccuracy: return perfectAccuracyPrefab;
                case ParticleEffectType.PersonalBest: return personalBestPrefab;
                default: return scoreAdditionPrefab;
            }
        }
        
        /// <summary>
        /// Get random particle prefab from alternative variants
        /// </summary>
        public GameObject GetRandomParticlePrefab(ParticleEffectType effectType)
        {
            if (!useRandomParticleVariants || alternativeParticlePrefabs.Count == 0)
            {
                return GetParticlePrefab(effectType);
            }
            
            return alternativeParticlePrefabs[Random.Range(0, alternativeParticlePrefabs.Count)];
        }
        
        /// <summary>
        /// Get particle prefab from specific category
        /// </summary>
        public GameObject GetParticlePrefabFromCategory(string categoryName)
        {
            foreach (var category in particleCategories)
            {
                if (category.categoryName == categoryName && category.particlePrefabs.Count > 0)
                {
                    if (category.useRandomSelection)
                    {
                        return category.particlePrefabs[Random.Range(0, category.particlePrefabs.Count)];
                    }
                    else
                    {
                        return category.particlePrefabs[0];
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Validate all required prefabs are assigned
        /// </summary>
        public bool ValidatePrefabCollection()
        {
            bool isValid = true;
            
            if (scoreAdditionPrefab == null) { Debug.LogWarning("Score Addition Prefab not assigned!"); isValid = false; }
            if (scoreSubtractionPrefab == null) { Debug.LogWarning("Score Subtraction Prefab not assigned!"); isValid = false; }
            if (xpGainPrefab == null) { Debug.LogWarning("XP Gain Prefab not assigned!"); isValid = false; }
            if (xpLossPrefab == null) { Debug.LogWarning("XP Loss Prefab not assigned!"); isValid = false; }
            if (coinGainPrefab == null) { Debug.LogWarning("Coin Gain Prefab not assigned!"); isValid = false; }
            if (coinLossPrefab == null) { Debug.LogWarning("Coin Loss Prefab not assigned!"); isValid = false; }
            if (starAchievementPrefab == null) { Debug.LogWarning("Star Achievement Prefab not assigned!"); isValid = false; }
            if (starLossPrefab == null) { Debug.LogWarning("Star Loss Prefab not assigned!"); isValid = false; }
            if (levelUpPrefab == null) { Debug.LogWarning("Level Up Prefab not assigned!"); isValid = false; }
            if (milestonePrefab == null) { Debug.LogWarning("Milestone Prefab not assigned!"); isValid = false; }
            
            return isValid;
        }
        
        /// <summary>
        /// Get collection status report
        /// </summary>
        public string GetCollectionStatus()
        {
            string status = "=== Particle VFX Prefab Collection Status ===\n";
            status += $"Score Addition: {(scoreAdditionPrefab != null ? "✅" : "❌")}\n";
            status += $"Score Subtraction: {(scoreSubtractionPrefab != null ? "✅" : "❌")}\n";
            status += $"XP Gain: {(xpGainPrefab != null ? "✅" : "❌")}\n";
            status += $"XP Loss: {(xpLossPrefab != null ? "✅" : "❌")}\n";
            status += $"Coin Gain: {(coinGainPrefab != null ? "✅" : "❌")}\n";
            status += $"Coin Loss: {(coinLossPrefab != null ? "✅" : "❌")}\n";
            status += $"Star Achievement: {(starAchievementPrefab != null ? "✅" : "❌")}\n";
            status += $"Star Loss: {(starLossPrefab != null ? "✅" : "❌")}\n";
            status += $"Level Up: {(levelUpPrefab != null ? "✅" : "❌")}\n";
            status += $"Milestone: {(milestonePrefab != null ? "✅" : "❌")}\n";
            status += $"Alternative Prefabs: {alternativeParticlePrefabs.Count}\n";
            status += $"Categories: {particleCategories.Count}\n";
            status += $"Random Variants: {(useRandomParticleVariants ? "Enabled" : "Disabled")}\n";
            
            return status;
        }
    }
}
