# Warning Sound Fixes

## Overview

This document outlines the fixes made to the warning sound system to ensure it plays continuously from when the warning threshold is reached until the time extension is completed or closed.

## Issues Identified

### 1. Warning Sound Stopped Too Early
- **ResetTimer()**: Warning sound was stopped when timer was reset
- **TriggerTimeUp()**: Warning sound was stopped immediately when time ran out
- **AddTime()**: Warning sound was stopped when time was added, but this was correct

### 2. Warning Sound Logic Problems
- Warning sound would stop during the warning period
- No mechanism to ensure continuous playback
- Warning sound state wasn't properly managed

### 3. Missing Integration Points
- TimeExtensionManager didn't properly handle warning sound continuation
- No clear lifecycle for warning sound management

### 4. **NEW: Unified Audio System Issue**
- **CRITICAL**: When using MiniGameAudioManager (unified audio), warning sounds were only played as one-shot sounds
- **CRITICAL**: No looping support in unified audio system for continuous warning sounds
- **CRITICAL**: Warning sound would play once and stop, not loop continuously during warning period

## Solution Implemented

### 1. Fixed Warning Sound Lifecycle

**Warning Sound Now Plays Continuously Until:**
- Time is added (ad watched or purchase made)
- Time extension panel is closed
- Game is completed
- Timer is manually stopped

**Warning Sound Starts When:**
- Timer reaches warning threshold
- `ShowWarning()` is called

### 2. **NEW: Enhanced MiniGameAudioManager**

Added new methods to support looping sounds:

```csharp
/// <summary>
/// Play a looping SFX sound using a pooled audio source
/// Returns the AudioSource so it can be stopped later
/// </summary>
public AudioSource PlayLoopingSFX(AudioClip sfxClip, float volumeScale = 1f)

/// <summary>
/// Stop a looping SFX sound and return the audio source to the pool
/// </summary>
public void StopLoopingSFX(AudioSource audioSource)
```

### 3. **NEW: Enhanced TimerManager Audio Integration**

Added proper support for looping warning sounds in unified audio system:

```csharp
// Audio source for looping warning sound (unified audio system)
private AudioSource loopingWarningSource = null;
```

**Updated Methods:**
- `ShowWarning()`: Now uses `PlayLoopingSFX()` for continuous warning sounds
- `EnsureWarningSoundContinues()`: Works with both unified and legacy systems
- `StopWarningSound()`: Properly stops looping sounds from unified system
- `StopWarningSoundOnTimeExtensionComplete()`: Handles both audio systems
- `StopTimer()`: Stops looping warning sounds from unified system

### 4. Updated Methods

#### **TimerManager.ResetTimer()**
```csharp
// OLD: Warning sound was stopped
if (warningAudioSource != null && warningSoundPlaying)
{
    warningAudioSource.Stop();
    warningSoundPlaying = false;
}

// NEW: Warning sound continues if warning is active
// Don't stop warning sound here - let it continue if warning is active
// Only stop if we're resetting during a warning period and want to clear it
```

#### **TimerManager.TriggerTimeUp()**
```csharp
// OLD: Warning sound stopped immediately
if (warningAudioSource != null && warningSoundPlaying)
{
    warningAudioSource.Stop();
    warningSoundPlaying = false;
}

// NEW: Warning sound continues for time extension
// Don't stop warning sound immediately - let TimeExtensionManager handle it
// The warning sound should continue until time extension is completed or closed
```

### 5. New Methods Added

#### **TimerManager.StopWarningSoundOnTimeExtensionComplete()**
```csharp
/// <summary>
/// Stop the warning sound when time extension is completed
/// This should be called when time is added or time extension panel is closed
/// </summary>
public void StopWarningSoundOnTimeExtensionComplete()
{
    // Stop unified audio system looping warning sound
    if (useUnifiedAudio && audioManager != null && loopingWarningSource != null)
    {
        audioManager.StopLoopingSFX(loopingWarningSource);
        loopingWarningSource = null;
        warningSoundPlaying = false;
        Debug.Log("TimerManager: Looping warning sound stopped - time extension completed");
    }
    // Stop legacy warning sound if playing
    else if (warningAudioSource != null && warningSoundPlaying)
    {
        warningAudioSource.Stop();
        warningSoundPlaying = false;
        Debug.Log("TimerManager: Warning sound stopped - time extension completed");
    }
    
    // Reset warning state
    warningShown = false;
}
```

#### **TimerManager.EnsureWarningSoundContinues()**
```csharp
/// <summary>
/// Check if warning sound should be playing and ensure it continues
/// This method ensures the warning sound continues during the warning period
/// </summary>
private void EnsureWarningSoundContinues()
{
    // Only check if we're in warning state
    if (!warningShown || currentTime <= 0) return;
    
    // Unified audio system
    if (useUnifiedAudio && audioManager != null)
    {
        // If warning sound should be playing but isn't, restart it
        if (!warningSoundPlaying && currentTime <= GetWarningThreshold() && currentTime > 0)
        {
            if (timeWarningSound != null)
            {
                // Stop any existing looping warning sound
                if (loopingWarningSource != null)
                {
                    audioManager.StopLoopingSFX(loopingWarningSource);
                    loopingWarningSource = null;
                }
                
                // Start new looping warning sound
                loopingWarningSource = audioManager.PlayLoopingSFX(timeWarningSound);
                warningSoundPlaying = (loopingWarningSource != null);
                Debug.Log("TimerManager: Restarted looping warning sound through MiniGameAudioManager");
            }
        }
    }
    // Legacy audio system (deprecated)
    else if (warningAudioSource != null && timeWarningSound != null)
    {
        // If warning sound should be playing but isn't, restart it
        if (!warningSoundPlaying && currentTime <= GetWarningThreshold() && currentTime > 0)
        {
            warningAudioSource.clip = timeWarningSound;
            warningAudioSource.loop = true;
            warningAudioSource.Play();
            warningSoundPlaying = true;
            Debug.Log("TimerManager: Restarted warning sound to ensure continuity");
        }
    }
}
```

### 6. **NEW: Enhanced Testing Methods**

Added new test methods for the looping warning sound system:

```csharp
[ContextMenu("Test Looping Warning Sound")]
public void TestLoopingWarningSound()

[ContextMenu("Test Unified Audio")]
public void TestUnifiedAudio()

[ContextMenu("Test Warning Sound")]
public void TestWarningSound()
```

### 7. Updated TimeExtensionManager Integration

The TimeExtensionManager now properly calls `StopWarningSoundOnTimeExtensionComplete()` when:
- Time extension panel is closed
- Time is added through ads or purchases
- Game is completed

## How It Works Now

### 1. **Unified Audio System (Recommended)**
1. When warning threshold is reached, `ShowWarning()` calls `audioManager.PlayLoopingSFX()`
2. This creates a pooled AudioSource with `loop = true`
3. The warning sound plays continuously until stopped
4. `EnsureWarningSoundContinues()` monitors and restarts if needed
5. When time extension completes, `StopWarningSoundOnTimeExtensionComplete()` stops the loop

### 2. **Legacy Audio System (Deprecated)**
1. Uses the assigned `warningAudioSource` with `loop = true`
2. Works as before but is deprecated

### 3. **Audio Source Pooling**
- MiniGameAudioManager manages a pool of AudioSource components
- Prevents audio source proliferation
- Automatically recycles audio sources when done

## Testing the Fix

### 1. **Test Looping Warning Sound**
- Right-click TimerManager in Inspector
- Select "Test Looping Warning Sound"
- Should start looping warning sound for 3 seconds

### 2. **Test Unified Audio**
- Right-click TimerManager in Inspector
- Select "Test Unified Audio"
- Should test both warning and time-up sounds

### 3. **Test Warning Sound**
- Right-click TimerManager in Inspector
- Select "Test Warning Sound"
- Should start looping warning sound

### 4. **Force Start Warning Sound**
- Right-click TimerManager in Inspector
- Select "Force Start Warning Sound"
- Should force start warning sound regardless of timer state

## Configuration

### 1. **Enable Unified Audio**
```csharp
timerManager.useUnifiedAudio = true;
timerManager.ConfigureUnifiedAudio(true);
```

### 2. **Assign Warning Sound**
```csharp
timerManager.timeWarningSound = yourWarningSoundClip;
```

### 3. **Set Warning Threshold**
```csharp
// Through DynamicScoreManager (recommended)
scoreManager.warningThreshold = 30f;

// Or directly on TimerManager (deprecated)
timerManager.warningThreshold = 30f;
```

## Troubleshooting

### 1. **Warning Sound Not Playing**
- Check if `timeWarningSound` is assigned
- Verify `useUnifiedAudio` is true
- Ensure `audioManager` is found
- Check console for debug messages

### 2. **Warning Sound Not Looping**
- Verify `useUnifiedAudio` is true
- Check if `PlayLoopingSFX` returns valid AudioSource
- Ensure `warningSoundPlaying` is set correctly

### 3. **Warning Sound Won't Stop**
- Check if `StopWarningSound()` is called
- Verify `loopingWarningSource` is properly nulled
- Ensure `StopLoopingSFX()` is called on audio manager

## Summary

The warning sound system has been completely overhauled to:

1. **Fix the critical looping issue** in the unified audio system
2. **Maintain backward compatibility** with legacy audio system
3. **Provide proper lifecycle management** for warning sounds
4. **Add comprehensive testing methods** for debugging
5. **Ensure continuous playback** during warning periods
6. **Properly integrate** with TimeExtensionManager

The warning sound will now play continuously from when the warning threshold is reached until the time extension is completed or closed, regardless of which audio system is used.
