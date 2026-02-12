using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace ParticleVFXSystem
{
    /// <summary>
    /// Particle event category configuration
    /// </summary>
    [System.Serializable]
    public class ParticleEventCategory
    {
        public string categoryName;
        public UnityEvent<ParticleEffectType, Vector3> categoryEvent;
        public bool enabled = true;
    }
    
    /// <summary>
    /// Event system for particle VFX effects
    /// Allows external systems to listen to particle events
    /// </summary>
    [CreateAssetMenu(fileName = "ParticleVFXEventSystem", menuName = "Particle VFX System/Event System")]
    public class ParticleVFXEventSystem : ScriptableObject
    {
        [Header("Particle VFX Events")]
        [Tooltip("Event triggered when any particle effect plays")]
        public UnityEvent<ParticleEffectType, Vector3> onParticleEffectPlayed;
        
        [Tooltip("Event triggered when score particles play")]
        public UnityEvent<int, Vector3> onScoreParticlePlayed;
        
        [Tooltip("Event triggered when XP particles play")]
        public UnityEvent<int, Vector3> onXPParticlePlayed;
        
        [Tooltip("Event triggered when coin particles play")]
        public UnityEvent<int, Vector3> onCoinParticlePlayed;
        
        [Tooltip("Event triggered when star particles play")]
        public UnityEvent<int, Vector3> onStarParticlePlayed;
        
        [Tooltip("Event triggered when achievement particles play")]
        public UnityEvent<string, Vector3> onAchievementParticlePlayed;
        
        [Header("Particle Effect Categories")]
        public List<ParticleEventCategory> eventCategories = new List<ParticleEventCategory>();
        
        [Header("Event Settings")]
        [Tooltip("Enable event logging for debugging")]
        public bool enableEventLogging = false;
        
        [Tooltip("Event logging level")]
        [Range(0, 3)]
        public int eventLogLevel = 1;
        
        /// <summary>
        /// Trigger particle effect event
        /// </summary>
        public void TriggerParticleEvent(ParticleEffectType effectType, Vector3 position)
        {
            if (enableEventLogging && eventLogLevel >= 1)
            {
                Debug.Log($"🎆 Particle Event: {effectType} at position {position}");
            }
            
            // Trigger main event
            onParticleEffectPlayed?.Invoke(effectType, position);
            
            // Trigger category-specific events
            foreach (var category in eventCategories)
            {
                if (category.enabled)
                {
                    category.categoryEvent?.Invoke(effectType, position);
                }
            }
            
            // Trigger specific effect events
            switch (effectType)
            {
                case ParticleEffectType.ScoreGain:
                case ParticleEffectType.ScoreLoss:
                    onScoreParticlePlayed?.Invoke((int)effectType, position);
                    break;
                    
                case ParticleEffectType.XPGain:
                case ParticleEffectType.XPLoss:
                    onXPParticlePlayed?.Invoke((int)effectType, position);
                    break;
                    
                case ParticleEffectType.CoinGain:
                case ParticleEffectType.CoinLoss:
                    onCoinParticlePlayed?.Invoke((int)effectType, position);
                    break;
                    
                case ParticleEffectType.StarAchievement:
                case ParticleEffectType.StarLoss:
                    onStarParticlePlayed?.Invoke((int)effectType, position);
                    break;
                    
                case ParticleEffectType.LevelUp:
                case ParticleEffectType.Milestone:
                case ParticleEffectType.StreakBonus:
                case ParticleEffectType.SpeedTier:
                case ParticleEffectType.PerfectAccuracy:
                case ParticleEffectType.PersonalBest:
                    onAchievementParticlePlayed?.Invoke(effectType.ToString(), position);
                    break;
            }
        }
        
        /// <summary>
        /// Trigger score particle event
        /// </summary>
        public void TriggerScoreParticleEvent(int scoreChange, Vector3 position)
        {
            ParticleEffectType effectType = scoreChange > 0 ? ParticleEffectType.ScoreGain : ParticleEffectType.ScoreLoss;
            TriggerParticleEvent(effectType, position);
        }
        
        /// <summary>
        /// Trigger XP particle event
        /// </summary>
        public void TriggerXPParticleEvent(int xpChange, Vector3 position)
        {
            ParticleEffectType effectType = xpChange > 0 ? ParticleEffectType.XPGain : ParticleEffectType.XPLoss;
            TriggerParticleEvent(effectType, position);
        }
        
        /// <summary>
        /// Trigger coin particle event
        /// </summary>
        public void TriggerCoinParticleEvent(int coinChange, Vector3 position)
        {
            ParticleEffectType effectType = coinChange > 0 ? ParticleEffectType.CoinGain : ParticleEffectType.CoinLoss;
            TriggerParticleEvent(effectType, position);
        }
        
        /// <summary>
        /// Trigger star particle event
        /// </summary>
        public void TriggerStarParticleEvent(int starChange, Vector3 position)
        {
            ParticleEffectType effectType = starChange > 0 ? ParticleEffectType.StarAchievement : ParticleEffectType.StarLoss;
            TriggerParticleEvent(effectType, position);
        }
        
        /// <summary>
        /// Trigger achievement particle event
        /// </summary>
        public void TriggerAchievementParticleEvent(string achievementName, Vector3 position)
        {
            if (enableEventLogging && eventLogLevel >= 2)
            {
                Debug.Log($"🏆 Achievement Particle Event: {achievementName} at position {position}");
            }
            
            onAchievementParticlePlayed?.Invoke(achievementName, position);
        }
        
        /// <summary>
        /// Add event listener to specific category
        /// </summary>
        public void AddCategoryListener(string categoryName, UnityAction<ParticleEffectType, Vector3> listener)
        {
            foreach (var category in eventCategories)
            {
                if (category.categoryName == categoryName)
                {
                    category.categoryEvent.AddListener(listener);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Remove event listener from specific category
        /// </summary>
        public void RemoveCategoryListener(string categoryName, UnityAction<ParticleEffectType, Vector3> listener)
        {
            foreach (var category in eventCategories)
            {
                if (category.categoryName == categoryName)
                {
                    category.categoryEvent.RemoveListener(listener);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Enable or disable specific event category
        /// </summary>
        public void SetCategoryEnabled(string categoryName, bool enabled)
        {
            foreach (var category in eventCategories)
            {
                if (category.categoryName == categoryName)
                {
                    category.enabled = enabled;
                    break;
                }
            }
        }
        
        /// <summary>
        /// Get event system status
        /// </summary>
        public string GetEventSystemStatus()
        {
            string status = "=== Particle VFX Event System Status ===\n";
            status += $"Event Logging: {(enableEventLogging ? "Enabled" : "Disabled")}\n";
            status += $"Event Log Level: {eventLogLevel}\n";
            status += $"Event Categories: {eventCategories.Count}\n";
            
            foreach (var category in eventCategories)
            {
                status += $"  {category.categoryName}: {(category.enabled ? "✅" : "❌")}\n";
            }
            
            return status;
        }
    }
}
