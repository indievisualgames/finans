# Timer Integration & Conflict Prevention

This document explains the new timer integration features in the DynamicScoreManager and how conflicts between TimeExtensionManager and game completion are prevented.

## Overview

The system now supports both time-based and non-time-based games with intelligent conflict prevention between time extension panels and game completion UI.

## Key Features

### 1. Timer Enable/Disable System
- **Time-based games**: Countdown or count-up timers with configurable limits
- **Non-time-based games**: No timer functionality, pure score-based gameplay
- **Dynamic switching**: Change game type at runtime

### 2. Conflict Prevention
- **Game state awareness**: TimeExtensionManager checks if game is completed
- **Auto-close**: Time extension panel automatically closes when game completes
- **Smart showing**: Time extension only shows when safe (no conflicts)

### 3. Warning Threshold Synchronization
- **Unified settings**: TimerManager and TimeExtensionManager share warning thresholds
- **Auto-sync**: Changes automatically propagate between managers
- **Configurable**: Set thresholds through DynamicScoreManager

## Usage Examples

### Basic Timer Setup

```csharp
// Enable timer for time-based game
scoreManager.SetGameType(MiniGameDynamicScoreManager.GameType.TimeBased);
scoreManager.EnableTimer(true);

// Configure timer settings
scoreManager.ConfigureTimer(120f, 30f, true); // 120s limit, 30s warning, countdown
scoreManager.StartTimer();
```

### Non-Time-Based Game

```csharp
// Disable timer for non-time-based game
scoreManager.SetGameType(MiniGameDynamicScoreManager.GameType.NonTimeBased);
scoreManager.EnableTimer(false);
```

### Timer Control

```csharp
// Start/Stop/Pause/Resume timer
scoreManager.StartTimer();
scoreManager.PauseTimer();
scoreManager.ResumeTimer();
scoreManager.StopTimer();

// Check timer status
bool isRunning = scoreManager.IsTimerRunning();
bool isEnabled = scoreManager.IsTimerEnabled();
float remainingTime = scoreManager.GetRemainingTime();
```

## Configuration Options

### DynamicScoreManager Settings

```csharp
[Header("Timer Integration")]
public bool enableTimer = true;                    // Enable/disable timer functionality
public TimerManager timerManager;                  // Reference to TimerManager
public TimeExtensionManager timeExtensionManager;  // Reference to TimeExtensionManager
public bool autoFindManagers = true;               // Auto-find managers if not assigned
public GameType gameType = GameType.TimeBased;     // Game type (TimeBased/NonTimeBased)
public bool countDownTimer = true;                 // Count down or count up
public float initialTimeLimit = 120f;             // Initial time limit in seconds
public float warningThreshold = 30f;              // Warning threshold in seconds
```

### TimeExtensionManager Settings

```csharp
[Header("Game State Awareness")]
public MiniGameDynamicScoreManager scoreManager;   // Reference to score manager
public bool autoCloseOnGameComplete = true;        // Auto-close on game completion
public bool checkGameCompletionBeforeShowing = true; // Check before showing panel
```

## Conflict Prevention Logic

### 1. Before Showing Time Extension
- Check if game is completed
- Check if timer is running
- Check if panel is already active

### 2. During Game Completion
- Automatically close time extension panel
- Prevent new time extension requests
- Log conflict prevention actions

### 3. Warning Threshold Sync
- TimeExtensionManager reads from TimerManager
- Changes propagate automatically
- Prevents mismatched warning times

## Integration with Existing Systems

### TimerManager Integration
- **Event handling**: Listens to OnTimeUp and OnWarningTime events
- **Configuration**: Sets initial time, warning threshold, countdown mode
- **State management**: Controls timer start/stop/pause/resume

### TimeExtensionManager Integration
- **Conflict prevention**: Automatically closes when game completes
- **Threshold sync**: Uses same warning threshold as TimerManager
- **Game state awareness**: Checks completion status before showing

### DynamicScoreManager Integration
- **Unified control**: Single point for all timer operations
- **Game type management**: Switch between time-based and non-time-based
- **Event coordination**: Manages conflicts between all systems

## Best Practices

### 1. Initialization Order
```csharp
// 1. Set game type first
scoreManager.SetGameType(GameType.TimeBased);

// 2. Enable timer
scoreManager.EnableTimer(true);

// 3. Configure settings
scoreManager.ConfigureTimer(timeLimit, warningThreshold, countDown);

// 4. Start timer
scoreManager.StartTimer();
```

### 2. Conflict Prevention
- Always check `IsGameCompleted()` before showing time extension
- Use `CloseOnGameComplete()` when game ends
- Sync warning thresholds between managers

### 3. Error Handling
- Check if managers exist before using them
- Use `autoFindManagers = true` for automatic discovery
- Handle cases where timer is disabled

## Troubleshooting

### Common Issues

1. **Timer not working**
   - Check if `enableTimer = true`
   - Verify `gameType = GameType.TimeBased`
   - Ensure TimerManager is assigned

2. **Time extension conflicts**
   - Check if `autoCloseOnGameComplete = true`
   - Verify score manager reference
   - Check game completion status

3. **Warning threshold mismatch**
   - Use `SyncWarningThresholdWithTimer()`
   - Set thresholds through DynamicScoreManager
   - Check both manager settings

### Debug Information

Use the diagnostic report to check system status:

```csharp
MiniGameDynamicScoreManager.DiagnosticReport();
```

This will show:
- Timer integration status
- Manager assignments
- Game type and timer state
- Warning threshold values

## Example Scenarios

### Scenario 1: Time-Based Game with Extension
1. Game starts with 120-second countdown
2. At 30 seconds remaining, warning triggers
3. Time extension panel shows (if enabled)
4. User watches ad, gets +60 seconds
5. Game continues until completion
6. Time extension panel auto-closes

### Scenario 2: Non-Time-Based Game
1. Game starts without timer
2. No time warnings or extensions
3. Pure score-based gameplay
4. Game completes normally

### Scenario 3: Game Completion During Warning
1. Warning triggers at 30 seconds
2. Time extension panel shows
3. User completes game before time runs out
4. Panel automatically closes
5. No UI conflicts

## Performance Considerations

- **Auto-finding managers**: Only runs once at startup
- **Event handling**: Minimal overhead during gameplay
- **Conflict checks**: Lightweight game state verification
- **Memory usage**: No additional allocations during runtime

## Future Enhancements

- **Multiple timer support**: Handle multiple concurrent timers
- **Advanced scheduling**: Complex timer patterns and sequences
- **Performance metrics**: Timer-based performance analysis
- **Custom events**: User-defined timer events and triggers
