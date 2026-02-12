# Star Rating System Setup Guide

## Overview
This guide will help you set up the new Star Rating System in your Unity minigame to display stars based on player performance.

## Problem Solved
- **Issue**: Stars were not becoming visible after final scores
- **Root Cause**: No star rating system was implemented
- **Solution**: Complete star rating system with score-based evaluation and animations

## Setup Instructions

### Step 1: Add Star Rating System to Scene

1. **Create Star GameObjects**:
   - Create 3-5 empty GameObjects as children of your win panel
   - Name them: "Star1", "Star2", "Star3", etc.
   - Add Image components to each star GameObject
   - Assign star sprites (empty and filled)

2. **Add StarRatingSystem Component**:
   - Create an empty GameObject named "StarRatingSystem"
   - Add the `StarRatingSystem` component
   - Assign the star GameObjects to the `stars` array
   - Assign star sprites (empty, filled, perfect)

### Step 2: Configure Win Panel

1. **Add WinPanelController**:
   - Select your win panel GameObject
   - Add the `WinPanelController` component
   - Assign references:
     - Star Rating System
     - Score Manager
     - Coin Game Manager
     - UI Text components (score, XP, accuracy, time)

2. **Set Up UI Text Components**:
   - Create TextMeshPro components for:
     - Score display
     - XP display
     - Accuracy display
     - Time taken display
     - Performance message

### Step 3: Configure Score Thresholds

The star system uses these default thresholds:

```csharp
// Score thresholds (0-3 stars)
scoreThresholds = { 0, 200, 400, 600 }

// Accuracy thresholds for bonus stars
accuracyThresholds = { 0.5f, 0.7f, 0.9f }

// Time efficiency thresholds for bonus stars
timeEfficiencyThresholds = { 0.3f, 0.6f, 0.8f }
```

**Customize these values** in the StarRatingSystem component inspector.

### Step 4: Animation Settings

Configure these animation parameters:

- **Star Animation Delay**: 0.3s (delay between stars)
- **Star Scale Duration**: 0.5s (animation duration)
- **Star Scale Multiplier**: 1.3x (scale effect)
- **Enable Star Particles**: true (particle effects)

### Step 5: Integration Points

The system automatically integrates with:

1. **MinigameScoreManager**: Gets final score, accuracy, time efficiency
2. **MasterCoinGameManager**: Triggers win panel display
3. **EducationalPerformanceTracker**: Uses performance data

## Star Rating Logic

### Base Stars (0-3 stars)
- 0 stars: Score < 200
- 1 star: Score 200-399
- 2 stars: Score 400-599
- 3 stars: Score 600+

### Bonus Stars
- **Accuracy Bonus**: +1 star for 90%+ accuracy
- **Time Efficiency Bonus**: +1 star for 80%+ time saved
- **Mistake Penalty**: -1 star for 3+ mistakes

### Maximum Stars
- Default: 3 stars (configurable up to 5)

## Performance Messages

Default messages for different star ratings:

- 0 stars: "Keep practicing!"
- 1 star: "Good job!"
- 2 stars: "Excellent work!"
- 3 stars: "Outstanding performance!"
- 4+ stars: "Perfect! You're a star!"

## Testing the System

1. **Play the game** and complete it
2. **Check the win panel** - stars should appear with animation
3. **Verify score calculation** - stars should match performance
4. **Test different scores** - adjust thresholds if needed

## Troubleshooting

### Stars Not Appearing
- Check if `WinPanelController` is attached to win panel
- Verify `StarRatingSystem` has star GameObjects assigned
- Ensure score manager is properly connected

### Wrong Star Count
- Adjust score thresholds in `StarRatingSystem`
- Check accuracy and time efficiency calculations
- Verify mistake penalty logic

### Animation Issues
- Check star GameObject hierarchy
- Verify Image components are assigned
- Ensure sprites are properly assigned

## Customization Options

### Custom Star Sprites
- Assign different sprites for empty, filled, and perfect stars
- Use the `perfectStarSprite` for the highest rating

### Custom Performance Messages
- Modify the `performanceMessages` array in `WinPanelController`
- Add more messages for higher star counts

### Custom Animation Timing
- Adjust `starAnimationDelay` for faster/slower star appearance
- Modify `starScaleDuration` for different animation speed
- Change `starScaleMultiplier` for different scale effects

## Advanced Features

### Particle Effects
- Assign a ParticleSystem to `starParticleEffect`
- Particles will play when each star appears

### Custom Score Calculation
- Override `CalculateStarRating()` method for custom logic
- Add more performance metrics (streak, difficulty, etc.)

### Integration with Other Systems
- Use `OnStarRatingCalculated` event for additional feedback
- Connect to achievement systems or progress tracking

## File Structure

```
Assets/ExternalAssets/MiniGames/Scripts/Managers/
├── StarRatingSystem.cs          # Main star rating logic
├── WinPanelController.cs        # Win panel management
└── Star_System_Setup_Guide.md  # This guide
```

## Support

If you encounter issues:

1. Check the Unity Console for error messages
2. Verify all references are properly assigned
3. Test with different score values
4. Check the Debug.Log messages for system status

The star system is now fully integrated and should display stars based on player performance!
